using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreServices.Services.Interfaces;

/// <summary>
/// Domain-level operations for linking and maintaining Discord account references.
/// </summary>
public interface IDiscordService
{
    /// <summary>
    /// Creates a reference for a Discord account linked to a user.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    /// <param name="discordUsername">Discord username or global display name.</param>
    /// <param name="refreshToken">Refresh token issued by Discord.</param>
    /// <param name="accessToken">Access token issued by Discord.</param>
    /// <param name="tokenType">Token type (e.g., Bearer).</param>
    /// <param name="tokenExpiry">Token expiry time.</param>
    /// <param name="scope">Scopes granted by Discord.</param>
    Task<Result<UserDiscordAccount>> CreateDiscordAccountReference(long userId, ulong discordSnowflake, string? discordUsername, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope);

    /// <summary>
    /// Updates OAuth token fields for a linked Discord account.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    /// <param name="refreshToken">Refresh token issued by Discord.</param>
    /// <param name="accessToken">Access token issued by Discord.</param>
    /// <param name="tokenType">Token type (e.g., Bearer).</param>
    /// <param name="tokenExpiry">Token expiry time.</param>
    /// <param name="scope">Scopes granted by Discord.</param>
    Task<Result<UserDiscordAccount>> UpdateDiscordAccountTokens(long userId, ulong discordSnowflake, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope);

    /// <summary>
    /// Updates Discord profile details (e.g., username) for a linked account.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    /// <param name="discordUsername">Discord username or global display name.</param>
    Task<Result<UserDiscordAccount>> UpdateDiscordAccountProfile(long userId, ulong discordSnowflake, string? discordUsername);

    /// <summary>
    /// Unlinks a Discord account reference from a user.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    Task<Result> UnlinkDiscordAccountReference(long userId, ulong discordSnowflake);

    /// <summary>
    /// Retrieves a Discord account by user ID and Discord snowflake ID.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    Task<Result<UserDiscordAccount>> GetDiscordAccountByUserIdAndSnowflake(long userId, ulong discordSnowflake);

    /// <summary>
    /// Retrieves all Discord accounts linked to a user.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    Task<Result<IEnumerable<UserDiscordAccount>>> GetDiscordAccountsByUserId(long userId);

    /// <summary>
    /// Retrieves all Discord accounts in the system.
    /// </summary>
    Task<Result<IEnumerable<UserDiscordAccount>>> GetAllDiscordAccounts();
    
    /// <summary>
    /// Retrieves all Discord accounts by Discord snowflake ID.
    /// </summary>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    /// <returns></returns>
    Task<Result<IEnumerable<UserDiscordAccount>>> GetDiscordAccountsBySnowflake(ulong discordSnowflake);
}
