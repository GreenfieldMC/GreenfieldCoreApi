using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Connections.Discord;

namespace GreenfieldCoreServices.Services.Interfaces;

/// <summary>
/// Domain-level operations for linking and maintaining Discord account references.
/// </summary>
public interface IDiscordService
{
    
    /// <summary>
    /// Creates a new Discord connection record.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="tokenType">The token type (e.g., Bearer).</param>
    /// <param name="tokenExpiry">The token expiry time.</param>
    /// <param name="scope">The scopes granted by Discord.</param>
    /// <param name="discordSnowflake">The Discord user snowflake.</param>
    /// <param name="discordUsername">The Discord username or global display name.</param>
    /// <returns></returns>
    Task<Result<DiscordConnection>> CreateDiscordConnection(string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope, ulong discordSnowflake, string discordUsername);
    
    /// <summary>
    /// Deletes a Discord connection record. This will also unlink it from any users.
    /// </summary>
    /// <param name="discordConnectionId">The internal Discord connection ID.</param>
    /// <returns></returns>
    Task<Result> DeleteDiscordConnection(long discordConnectionId);
    
    /// <summary>
    /// Links an existing Discord connection to a user.
    /// </summary>
    /// <param name="userId">The internal user ID.</param>
    /// <param name="discordConnectionId">The internal Discord connection ID.</param>
    /// <returns></returns>
    Task<Result<UserDiscordConnection>> LinkUserToDiscordConnection(long userId, long discordConnectionId);

    /// <summary>
    /// Unlinks a Discord account reference from a user.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordConnectionId">Internal Discord connection ID.</param>
    Task<Result> UnlinkUserDiscordConnection(long userId, long discordConnectionId);

    /// <summary>
    /// Updates OAuth token fields for a linked Discord account.
    /// </summary>
    /// <param name="discordConnectionId">Internal Discord connection ID.</param>
    /// <param name="refreshToken">Refresh token issued by Discord.</param>
    /// <param name="accessToken">Access token issued by Discord.</param>
    /// <param name="tokenType">Token type (e.g., Bearer).</param>
    /// <param name="tokenExpiry">Token expiry time.</param>
    /// <param name="scope">Scopes granted by Discord.</param>
    Task<Result<DiscordConnection>> UpdateDiscordConnectionTokens(long discordConnectionId, string refreshToken,
        string accessToken, string tokenType, DateTime tokenExpiry, string scope);

    /// <summary>
    /// Updates Discord profile details (e.g., username) for a linked account.
    /// </summary>
    /// <param name="discordConnectionId"></param>
    /// <param name="discordUsername">Discord username or global display name.</param>
    Task<Result<DiscordConnection>> UpdateDiscordConnectionProfile(long discordConnectionId, string discordUsername);

    /// <summary>
    /// Retrieves a Discord account by user ID and Discord connection ID.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    /// <param name="discordConnectionId">>Internal Discord connection ID.</param>
    Task<Result<UserDiscordConnection>> GetUserDiscordConnection(long userId, long discordConnectionId);

    /// <summary>
    /// Retrieves all Discord accounts linked to a user.
    /// </summary>
    /// <param name="userId">Internal user ID.</param>
    Task<Result<IEnumerable<UserDiscordConnection>>> GetUserDiscordConnections(long userId);

    /// <summary>
    /// Retrieves all Discord accounts in the system.
    /// </summary>
    Task<Result<IEnumerable<DiscordConnection>>> GetAllDiscordConnections();

    /// <summary>
    /// Retrieves a Discord account by its Discord snowflake ID.
    /// </summary>
    /// <param name="discordSnowflake">Discord user snowflake.</param>
    /// <returns></returns>
    Task<Result<DiscordConnection>> GetDiscordConnectionBySnowflake(ulong discordSnowflake);
    
    /// <summary>
    /// Retrieves a Discord account by its internal connection ID.
    /// </summary>
    /// <param name="discordConnectionId">Internal Discord connection ID.</param>
    /// <returns></returns>
    Task<Result<DiscordConnection>> GetDiscordConnection(long discordConnectionId);

    /// <summary>
    /// Retrieves all users linked to a specific Discord connection ID.
    /// </summary>
    /// <param name="discordConnectionId">Internal Discord connection ID.</param>
    /// <returns></returns>
    Task<Result<IEnumerable<UserDiscordConnection>>> GetUsersByDiscordConnectionId(long discordConnectionId);
}
