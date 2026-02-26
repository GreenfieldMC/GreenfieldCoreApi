using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GreenfieldCoreServices.Models.Clients;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace GreenfieldCoreApi.Transformers;

public class RoleClaimTransformer : IClaimsTransformation
{
    
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ICacheService<Guid, Client> _clientCache;
    
    public RoleClaimTransformer(ICacheService<Guid, Client> clientCache, IServiceScopeFactory serviceScopeFactory)
    {
        _clientCache = clientCache;
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } ci)
            return principal;
        var subClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (subClaim == null || !Guid.TryParse(subClaim.Value, out var clientId))
            return principal;
        
        if (!_clientCache.TryGetValue(clientId, out var client))
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var clientAuthService = scope.ServiceProvider.GetRequiredService<IClientAuthService>();
            client = (await clientAuthService.GetClientById(clientId)).Data;
        }
        
        var roles = client?.Roles;
        if (roles is null)
            return principal;

        var newIdentity = new ClaimsIdentity(
            ci.Claims.Where(c => c.Type != ClaimTypes.Role),
            ci.AuthenticationType,
            ci.NameClaimType,
            ClaimTypes.Role
        );
        
        foreach (var role in roles)
            newIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
        
        return new ClaimsPrincipal(newIdentity);
    }
}