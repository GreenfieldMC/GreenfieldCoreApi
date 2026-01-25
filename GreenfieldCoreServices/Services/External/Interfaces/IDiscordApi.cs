using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Connections.Discord;
using GreenfieldCoreServices.Models.Discord;
using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreServices.Services.External.Interfaces;

/// <summary>
/// Discord OAuth API wrapper for token exchange/refresh, identity fetch, and linking to local users.
/// </summary>
public interface IDiscordApi
{
    /// <summary>
    /// Calls Discord /users/@me using a bearer access token.
    /// </summary>
    /// <param name="accessToken">The Discord access token.</param>
    Task<Result<DiscordIdentityResponse>> GetDiscordIdentity(string accessToken);

    /// <summary>
    /// Exchanges an authorization code for access and refresh tokens.
    /// </summary>
    /// <param name="authorizationCode">The code returned from Discord's authorize step.</param>
    Task<Result<DiscordOAuthTokenResponse>> CreateDiscordAccessTokenAsync(string authorizationCode);

    /// <summary>
    /// Refreshes Discord access/refresh tokens using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token previously issued by Discord.</param>
    Task<Result<DiscordOAuthTokenResponse>> RefreshDiscordAccessTokenAsync(string refreshToken);

    /// <summary>
    /// Links a Discord account to a user by exchanging the code and persisting the account.
    /// </summary>
    /// <param name="userId">Internal user ID to link.</param>
    /// <param name="code">Discord authorization code.</param>
    Task<Result<UserDiscordConnection>> LinkDiscordAccountToUser(long userId, string code);
}
