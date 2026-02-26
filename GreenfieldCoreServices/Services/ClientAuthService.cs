using System.IdentityModel.Tokens.Jwt;
using System.Net;
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
    
    public async Task<Result<(Client client, string secret)>> RegisterClient(string clientName, List<string> roles)
    {
        var generatedSecret = GenerateClientSecret();
        
        uow.BeginTransaction();
        var repo = uow.Repository<IClientRepository>();
        
        var newClient = (await repo.RegisterClient(clientName, generatedSecret.hashedSecret, generatedSecret.salt)).GetOrThrow();

        var assignedRoles = new List<string>();
        foreach (var role in roles)
        {
            var assignedRoleResult = await repo.AssignRoleToClient(newClient.Item1, role);
            if (assignedRoleResult.IsSuccessful) assignedRoles.Add(role);
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
        
        return Result<(Client client, string secret)>.Success((registeredClient, generatedSecret.secret));
    }

    public async Task<Result<string>> AuthenticateLogin(Guid clientId, string clientSecret)
    {
        var repo = uow.Repository<IClientRepository>();
        var selectResult = await repo.SelectClientById(clientId);
        
        if (!selectResult.TryGetDataNonNull(out var client)) 
            return Result<string>.Failure("Client not found.", HttpStatusCode.NotFound);
        
        var hashedSecret = HashClientSecret(clientSecret, client.Salt);
        
        var isValid = (await repo.VerifyClientCredentials(clientId, hashedSecret.hash, hashedSecret.salt)).GetOrThrow();

        var roles = (await repo.SelectClientRoles(clientId)).GetNonNullOrThrow().Select(r => r.RoleName).ToList();
        
        cache.SetValue(client.ClientId, new Client
        {
            ClientId = client.ClientId,
            ClientName = client.ClientName,
            CreatedOn = client.CreatedOn,
            Roles = roles
        });
        
        return !isValid ? Result<string>.Failure("Invalid credentials.", HttpStatusCode.Unauthorized) : Result<string>.Success(GenerateToken(clientId));
    }

    public async Task<Result<IEnumerable<Client>>> GetAllClients()
    {
        var repo = uow.Repository<IClientRepository>();

        var foundClients = (await repo.SelectClients()).GetNonNullOrThrow();
        
        var clients = new List<Client>();
        
        foreach (var client in foundClients)
        {
            var roles = (await repo.SelectClientRoles(client.ClientId)).GetNonNullOrThrow().Select(r => r.RoleName).ToList();
            clients.Add(new Client
            {
                ClientId = client.ClientId,
                ClientName = client.ClientName,
                CreatedOn = client.CreatedOn,
                Roles = roles
            });
        }
        
        return Result<IEnumerable<Client>>.Success(clients);
    }

    public async Task<Result<Client>> GetClientById(Guid clientId)
    {
        if (cache.TryGetValue(clientId, out var value))
            return Result<Client>.Success(value);
        
        var repo = uow.Repository<IClientRepository>();
        
        var foundClient = (await repo.SelectClientById(clientId)).GetOrThrow();
        if (foundClient is null) return Result<Client>.Failure("Client not found.", HttpStatusCode.NotFound);
        
        var roles = await repo.SelectClientRoles(clientId);
        var roleNames = roles.GetNonNullOrThrow().Select(r => r.RoleName).ToList();
        
        var client = new Client
        {
            ClientId = foundClient.ClientId,
            ClientName = foundClient.ClientName,
            CreatedOn = foundClient.CreatedOn,
            Roles = roleNames
        };
        
        cache.SetValue(client.ClientId, client);
        
        return Result<Client>.Success(client);
    }

    public async Task<Result<Client>> GetClientByName(string clientName)
    {
        if (cache.TryGetValue(c => c.ClientName.Equals(clientName, StringComparison.OrdinalIgnoreCase), out var cachedClient))
            return Result<Client>.Success(cachedClient);
        
        var repo = uow.Repository<IClientRepository>();
        
        var foundClient = (await repo.SelectClientByName(clientName)).GetOrThrow();
        if (foundClient is null) return Result<Client>.Failure("Client not found.", HttpStatusCode.NotFound);
        
        var roles = await repo.SelectClientRoles(foundClient.ClientId);
        var roleNames = roles.GetNonNullOrThrow().Select(r => r.RoleName).ToList();
        
        var client = new Client
        {
            ClientId = foundClient.ClientId,
            ClientName = foundClient.ClientName,
            CreatedOn = foundClient.CreatedOn,
            Roles = roleNames
        };
        
        cache.SetValue(client.ClientId, client);
        
        return Result<Client>.Success(client);
    }

    public async Task<Result<Client>> DeleteClient(Guid clientId)
    {
        var foundClientResult = await GetClientById(clientId);
        if (!foundClientResult.IsSuccessful) return foundClientResult;
        var foundClient = foundClientResult.GetNonNullOrThrow(nullDataMessage: "GetClientById returned null client despite being successful.");
        
        uow.BeginTransaction();
        var deletedResult = await uow.Repository<IClientRepository>().DeleteClient(clientId);
        if (!deletedResult.IsSuccessful) return Result<Client>.Failure("Client could not be deleted.");
        uow.CompleteAndCommit();
        
        cache.RemoveValue(clientId);
        
        return Result<Client>.Success(foundClient);
    }

    public async Task<Result<Client>> UpdateClientRoles(Guid clientId, List<string> roles)
    {
        var foundClientResult = await GetClientById(clientId);
        if (!foundClientResult.IsSuccessful) return foundClientResult;
        var foundClient = foundClientResult.GetNonNullOrThrow(nullDataMessage: "GetClientById returned null client despite being successful.");
        
        var repo = uow.Repository<IClientRepository>();
        var currentRoles = foundClient.Roles;
        var rolesToAdd = roles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(roles).ToList();
        
        uow.BeginTransaction();
        
        foreach (var role in rolesToAdd)
        {
            var addedRoleResult = await repo.AssignRoleToClient(clientId, role);
            if (addedRoleResult.IsSuccessful) currentRoles.Add(role);
        }

        foreach (var role in rolesToRemove)
        {
            var removedRoleResult = await repo.RemoveRoleFromClient(clientId, role);
            if (removedRoleResult.IsSuccessful) currentRoles.Remove(role);
        }
        
        uow.CompleteAndCommit();
        
        foundClient.Roles = currentRoles;
        cache.SetValue(foundClient.ClientId, foundClient);
        
        return Result<Client>.Success(foundClient);
    }

    public async Task<Result<string>> RefreshClientSecret(Guid clientId)
    {
        var foundClientResult = await GetClientById(clientId);
        if (!foundClientResult.IsSuccessful || foundClientResult.IsDataNull()) return Result<string>.Failure("Client not found.", HttpStatusCode.NotFound);
        
        var newSecret = GenerateClientSecret();
        
        uow.BeginTransaction();
        
        var updatedResult = await uow.Repository<IClientRepository>().UpdateClientSecret(clientId, newSecret.hashedSecret, newSecret.salt);
        if (!updatedResult.IsSuccessful) return Result<string>.Failure("Client secret could not be updated.");
        
        uow.CompleteAndCommit();
        
        return Result<string>.Success(newSecret.secret);
    }

    public async Task<Result<Client>> UpdateClientName(Guid clientId, string newName)
    {
        var foundClientResult = await GetClientById(clientId);
        if (!foundClientResult.IsSuccessful) return foundClientResult;
        var foundClient = foundClientResult.GetNonNullOrThrow(nullDataMessage: "GetClientById returned null client despite being successful.");
        
        uow.BeginTransaction();
        
        var updateResult = await uow.Repository<IClientRepository>().UpdateClientName(clientId, newName);
        if (!updateResult.IsSuccessful) return Result<Client>.Failure("Client name could not be updated.");
        
        uow.CompleteAndCommit();
        
        foundClient.ClientName = newName;
        
        cache.SetValue(foundClient.ClientId, foundClient);
        
        return Result<Client>.Success(foundClient);
    }

    public async Task<Result<Client>> ClearClientRoles(Guid clientId)
    {
        var foundClientResult = await GetClientById(clientId);
        if (!foundClientResult.IsSuccessful) return foundClientResult;
        var foundClient = foundClientResult.GetNonNullOrThrow(nullDataMessage: "GetClientById returned null client despite being successful.");
        
        uow.BeginTransaction();
        
        var removalCount = (await uow.Repository<IClientRepository>().DeleteClientRoles(clientId)).GetOrThrow();
        if (removalCount == 0) return Result<Client>.Failure("There were no roles removed from the client.");
        
        uow.CompleteAndCommit();

        if (foundClient.Roles.Count != removalCount) return await GetClientById(clientId);
        
        foundClient.Roles.Clear();
        
        cache.SetValue(foundClient.ClientId, foundClient);
        
        return Result<Client>.Success(foundClient);
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