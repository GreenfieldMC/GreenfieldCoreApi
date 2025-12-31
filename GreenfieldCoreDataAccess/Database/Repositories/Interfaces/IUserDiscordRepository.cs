using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

/// <summary>
/// Data access for user-to-Discord account links.
/// </summary>
public interface IUserDiscordRepository
{
    /// <summary>
    /// Creates a new user-Discord link with OAuth token data.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    /// <param name="discordUsername">Discord username or display name.</param>
    /// <param name="refreshToken">Refresh token issued by Discord.</param>
    /// <param name="accessToken">Access token issued by Discord.</param>
    /// <param name="tokenType">Token type (e.g., Bearer).</param>
    /// <param name="tokenExpiry">Token expiry time.</param>
    /// <param name="scope">Scopes granted by Discord.</param>
    Task<Result<UserDiscordEntity>> InsertUserDiscordReference(long userId, ulong discordSnowflake, string? discordUsername, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope);

    /// <summary>
    /// Gets all Discord links for a user.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    Task<Result<IEnumerable<UserDiscordEntity>>> SelectUserDiscordReferences(long userId);

    /// <summary>
    /// Gets a specific Discord link by user and snowflake.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    Task<Result<UserDiscordEntity>> SelectUserDiscordAccount(long userId, ulong discordSnowflake);

    /// <summary>
    /// Updates OAuth token fields for a user-Discord link.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    /// <param name="refreshToken">Refresh token issued by Discord.</param>
    /// <param name="accessToken">Access token issued by Discord.</param>
    /// <param name="tokenType">Token type (e.g., Bearer).</param>
    /// <param name="tokenExpiry">Token expiry time.</param>
    /// <param name="scope">Scopes granted by Discord.</param>
    Task<Result<bool>> UpdateUserDiscordTokens(long userId, ulong discordSnowflake, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope);

    /// <summary>
    /// Updates Discord profile data (e.g., username) for a user-Discord link.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    /// <param name="discordUsername">Discord username or display name.</param>
    Task<Result<bool>> UpdateUserDiscordProfile(long userId, ulong discordSnowflake, string? discordUsername);

    /// <summary>
    /// Deletes a user-Discord link.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    Task<Result<bool>> DeleteUserDiscordReference(long userId, ulong discordSnowflake);

    /// <summary>
    /// Gets all Discord links in the system.
    /// </summary>
    Task<Result<IEnumerable<UserDiscordEntity>>> SelectAllDiscordAccounts();
    
    /// <summary>
    /// Gets all Discord links by Discord snowflake.
    /// </summary>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    /// <returns></returns>
    Task<Result<IEnumerable<UserDiscordEntity>>> SelectDiscordAccountsBySnowflake(ulong discordSnowflake);
}
