using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IDiscordConnectionRepository
{
    /// <summary>
    /// Inserts a new Discord connection record into the database.
    /// </summary>
    /// <param name="refreshToken">The refresh token for the Discord connection.</param>
    /// <param name="accessToken">The access token for the Discord connection.</param>
    /// <param name="tokenType">The type of the token.</param>
    /// <param name="tokenExpiry">The expiration date of the token.</param>
    /// <param name="scope">The scope of the connection.</param>
    /// <param name="discordSnowflake">The Discord snowflake ID of the user.</param>
    /// <param name="discordUsername">The Discord username of the user.</param>
    /// <returns></returns>
    Task<Result<DiscordConnectionEntity>> InsertConnection(string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope, ulong discordSnowflake, string discordUsername);
    
    /// <summary>
    /// Deletes a Discord connection record from the database.
    /// </summary>
    /// <param name="discordConnectionId">The ID of the Discord connection to delete.</param>
    /// <returns></returns>
    Task<Result> DeleteConnection(long discordConnectionId);
    
    /// <summary>
    /// Selects all Discord connection records from the database.
    /// </summary>
    /// <returns></returns>
    Task<Result<IEnumerable<DiscordConnectionEntity>>> SelectAllConnections();
    
    /// <summary>
    /// Selects a Discord connection record by its ID.
    /// </summary>
    /// <param name="discordConnectionId">The ID of the Discord connection to select.</param>
    /// <returns></returns>
    Task<Result<DiscordConnectionEntity>> SelectConnectionById(long discordConnectionId);
    
    /// <summary>
    /// Selects a Discord connection record by its Discord snowflake ID.
    /// </summary>
    /// <param name="discordSnowflake"></param>
    /// <returns></returns>
    Task<Result<DiscordConnectionEntity>> SelectConnectionBySnowflake(ulong discordSnowflake);
    
    /// <summary>
    /// Updates OAuth token fields for a Discord connection.
    /// </summary>
    /// <param name="discordConnectionId">The ID of the Discord connection to update.</param>
    /// <param name="refreshToken">The new refresh token.</param>
    /// <param name="accessToken">The new access token.</param>
    /// <param name="tokenType">The new token type.</param>
    /// <param name="tokenExpiry">The new token expiry date.</param>
    /// <param name="scope">The new scope.</param>
    /// <returns></returns>
    Task<Result> UpdateConnectionTokens(long discordConnectionId, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope);
    
    /// <summary>
    /// Updates profile fields for a Discord connection.
    /// </summary>
    /// <param name="discordConnectionId">The ID of the Discord connection to update.</param>
    /// <param name="discordUsername">The new Discord username.</param>
    /// <returns></returns>
    Task<Result> UpdateConnectionProfile(long discordConnectionId, string discordUsername);
    
    /// <summary>
    /// Inserts a new user-Discord connection link.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="discordConnectionId"></param>
    /// <returns></returns>
    Task<Result<UserDiscordConnectionEntity>> InsertUserDiscordConnection(long userId, long discordConnectionId);
    
    /// <summary>
    /// Deletes a user-Discord connection link.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="discordConnectionId"></param>
    /// <returns></returns>
    Task<Result> DeleteUserDiscordConnection(long userId, long discordConnectionId);
    
    /// <summary>
    /// Selects all Discord connections linked to a user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<Result<IEnumerable<UserDiscordConnectionEntity>>> SelectUserDiscordConnections(long userId);

    /// <summary>
    /// Selects all users linked to a Discord connection.
    /// </summary>
    /// <param name="discordConnectionId"></param>
    /// <returns></returns>
    Task<Result<IEnumerable<UserByDiscordConnectionEntity>>> SelectUsersByDiscordConnection(long discordConnectionId);

}