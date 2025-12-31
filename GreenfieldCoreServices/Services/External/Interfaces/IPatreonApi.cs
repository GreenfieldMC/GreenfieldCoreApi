using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Patreon;
using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreServices.Services.External.Interfaces;

public interface IPatreonApi
{
    
    /// <summary>
    /// Calls to the Patreon API to get the identity of the user associated with the given access token. /oauth2/v2/identity
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    Task<Result<PatreonUserIdentityModel>> GetPatreonIdentity(string accessToken);

    /// <summary>
    /// Calls to the Patreon API to create an access token for the given user. /oauth2/token
    /// </summary>
    /// <param name="authorizationCode"></param>
    /// <returns></returns>
    Task<Result<PatreonOAuthTokenResponse>> CreatePatreonAccessTokenAsync(string authorizationCode);
    
    Task<Result<PatreonOAuthTokenResponse>> RefreshPatreonAccessTokenAsync(string refreshToken);

    /// <summary>
    /// Links a Patreon account to a user in the system.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="code"></param>
    /// <returns></returns>
    Task<Result<UserPatreonAccount>> LinkPatreonAccountToUser(long userId, string code);

    /// <summary>
    /// Attempts to resolve the Patreon campaign ID associated with the configured Patreon integration.
    /// </summary>
    /// <returns></returns>
    Task<Result<string>> ResolveCampaignId();

}