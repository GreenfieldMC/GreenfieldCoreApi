using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Clients;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GreenfieldCoreServices.Services;

public class ClientAuthService(IUnitOfWork uow, IConfiguration config, ICacheService<Guid, Client> cache) : IClientAuthService
{
    
    public async Task<(Client client, string secret)> RegisterClient(string clientName, List<string> roles)
    {
        var generatedSecret = GenerateClientSecret();
        
        uow.BeginTransaction();
        var repo = uow.Repository<IClientRepository>();
        
        var newClient = (await repo.RegisterClient(clientName, generatedSecret.hashedSecret, generatedSecret.salt)).GetOrThrow();

        var assignedRoles = new List<string>();
        foreach (var role in roles)
        {
            var assignedRole = (await repo.AssignRoleToClient(newClient.Item1, role)).GetOrThrow();
            if (assignedRole) assignedRoles.Add(role);
        }
        
        uow.CompleteAndCommit();

        var registeredClient = new Client
        {
            ClientId = newClient.Item1,
            ClientName = clientName,
            CreatedOn = newClient.Item2,
            Roles = assignedRoles
        };
        
        cache.SetValue(registeredClient.ClientId, registeredClient);
        
        return (registeredClient, generatedSecret.secret);
    }

    public async Task<string> AuthenticateLogin(Guid clientId, string clientSecret)
    {
        var repo = uow.Repository<IClientRepository>();
        var client = (await repo.GetClientById(clientId)).GetOrThrow();
        
        if (client is null) throw new Exception("Client not found");
        
        var hashedSecret = HashClientSecret(clientSecret, client.Salt);
        
        var isValid = (await repo.VerifyClientCredentials(clientId, hashedSecret.hash, hashedSecret.salt)).GetOrThrow();

        var roles = (await repo.GetClientRoles(clientId)).GetOrThrow().Select(r => r.RoleName).ToList();
        
        cache.SetValue(client.ClientId, new Client
        {
            ClientId = client.ClientId,
            ClientName = client.ClientName,
            CreatedOn = client.CreatedOn,
            Roles = roles
        });
        
        return !isValid ? throw new Exception("Invalid credentials") : GenerateToken(clientId);
    }

    public async Task<IEnumerable<Client>> GetAllClients()
    {
        var repo = uow.Repository<IClientRepository>();

        var foundClients = (await repo.GetAllClients()).GetOrThrow();
        
        var clients = new List<Client>();
        
        foreach (var client in foundClients)
        {
            var roles = (await repo.GetClientRoles(client.ClientId)).GetOrThrow().Select(r => r.RoleName).ToList();
            clients.Add(new Client
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                CreatedOn = client.CreatedOn,
                Roles = roles
            });
        }
        
        return clients;
    }

    public async Task<Client?> GetClientById(Guid clientId)
    {
        if (cache.TryGetValue(clientId, out var value))
            return value;
        
        var repo = uow.Repository<IClientRepository>();
        
        var foundClient = (await repo.GetClientById(clientId)).GetOrThrow();
        
        if (foundClient == null) return null;
        
        var roles = await repo.GetClientRoles(clientId);
        var roleNames = roles.GetOrThrow().Select(r => r.RoleName).ToList();
        
        var client = new Client
        {
            ClientId = foundClient.ClientId,
            ClientName = foundClient.ClientName,
            CreatedOn = foundClient.CreatedOn,
            Roles = roleNames
        };
        
        cache.SetValue(client.ClientId, client);
        
        return client;
    }

    public async Task<Client?> GetClientByName(string clientName)
    {
        if (cache.TryGetValue(c => c.ClientName.Equals(clientName, StringComparison.OrdinalIgnoreCase), out var cachedClient))
            return cachedClient;
        
        var repo = uow.Repository<IClientRepository>();
        
        var foundClient = (await repo.GetClientByName(clientName)).GetOrThrow();
        
        if (foundClient == null) return null;
        
        var roles = await repo.GetClientRoles(foundClient.ClientId);
        var roleNames = roles.GetOrThrow().Select(r => r.RoleName).ToList();
        
        var client = new Client
        {
            ClientId = foundClient.ClientId,
            ClientName = foundClient.ClientName,
            CreatedOn = foundClient.CreatedOn,
            Roles = roleNames
        };
        
        cache.SetValue(client.ClientId, client);
        
        return client;
    }

    public async Task<Client?> DeleteClient(Guid clientId)
    {
        var foundClient = await GetClientById(clientId);
        if (foundClient == null) return null;
        
        uow.BeginTransaction();
        var deleted = (await uow.Repository<IClientRepository>().DeleteClient(clientId)).GetOrThrow();
        
        if (!deleted) return null;
        
        uow.CompleteAndCommit();
        
        cache.RemoveValue(clientId);
        
        return foundClient;
    }

    public async Task<Client?> UpdateClientRoles(Guid clientId, List<string> roles)
    {
        var foundClient = await GetClientById(clientId);
        if (foundClient == null) return null;
        
        var repo = uow.Repository<IClientRepository>();
        var currentRoles = foundClient.Roles;
        var rolesToAdd = roles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(roles).ToList();
        
        uow.BeginTransaction();
        
        foreach (var role in rolesToAdd)
        {
            var addedRole = (await repo.AssignRoleToClient(clientId, role)).GetOrThrow();
            if (addedRole) currentRoles.Add(role);
        }

        foreach (var role in rolesToRemove)
        {
            var removedRole = (await repo.RemoveRoleFromClient(clientId, role)).GetOrThrow();
            if (removedRole) currentRoles.Remove(role);
        }
        
        uow.CompleteAndCommit();
        
        foundClient.Roles = currentRoles;
        
        cache.SetValue(foundClient.ClientId, foundClient);
        
        return foundClient;
    }

    public async Task<string?> RefreshClientSecret(Guid clientId)
    {
        var foundClient = await GetClientById(clientId);
        if (foundClient == null) return null;
        
        var newSecret = GenerateClientSecret();
        
        uow.BeginTransaction();
        
        await uow.Repository<IClientRepository>().UpdateClientSecret(clientId, newSecret.hashedSecret, newSecret.salt);
        
        uow.CompleteAndCommit();
        
        return newSecret.secret;
    }

    public async Task<Client?> UpdateClientName(Guid clientId, string newName)
    {
        var foundClient = await GetClientById(clientId);
        if (foundClient == null) return null;
        
        uow.BeginTransaction();
        
        var updateResult = (await uow.Repository<IClientRepository>().UpdateClientName(clientId, newName)).GetOrThrow();
        if (!updateResult) return null;
        
        uow.CompleteAndCommit();
        
        foundClient.ClientName = newName;
        
        cache.SetValue(foundClient.ClientId, foundClient);
        
        return foundClient;
    }

    public async Task<Client?> ClearClientRoles(Guid clientId)
    {
        var foundClient = await GetClientById(clientId);
        if (foundClient == null) return null;
        
        uow.BeginTransaction();
        
        var removalCount = (await uow.Repository<IClientRepository>().ClearClientRoles(clientId)).GetOrThrow();
        
        if (removalCount == 0) return null;
        
        uow.CompleteAndCommit();

        if (foundClient.Roles.Count != removalCount) return await GetClientById(clientId);
        
        foundClient.Roles.Clear();
        
        cache.SetValue(foundClient.ClientId, foundClient);
        
        return foundClient;
    }

    private static (string secret, string hashedSecret, string salt) GenerateClientSecret()
    {
        var clientSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var (hashedSecret, salt) = HashClientSecret(clientSecret);
        return (clientSecret, hashedSecret, salt);
    }

    private static (string hash, string salt) HashClientSecret(string clientSecret, string? salt = null)
    {
        var actualSalt = salt != null ? Convert.FromBase64String(salt) : RandomNumberGenerator.GetBytes(16);
        using var derived = new Rfc2898DeriveBytes(clientSecret, actualSalt, 10000, HashAlgorithmName.SHA256);
        var actualHash = derived.GetBytes(32);

        return (Convert.ToBase64String(actualHash), Convert.ToBase64String(actualSalt));
    }

    private string GenerateToken(Guid clientId)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.GetValue<string>("jwtSettings:key") ?? throw new ArgumentException("JWT key not found in configuration.")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, clientId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var token = new JwtSecurityToken(
            issuer: config.GetValue<string>("jwtSettings:issuer"),
            audience: config.GetValue<string>("jwtSettings:audience"),
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(config.GetValue<int>("jwtSettings:expiryInMinutes")),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
}