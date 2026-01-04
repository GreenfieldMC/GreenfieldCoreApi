using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IPatreonConnectionRepository
{
    /// <summary>
    /// Inserts a new Patreon connection record into the database.
    /// </summary>
    /// <param name="refreshToken">The refresh token for the Patreon connection.</param>
    /// <param name="accessToken">The access token for the Patreon connection.</param>
    /// <param name="tokenType">The type of the token.</param>
    /// <param name="tokenExpiry">The expiration date of the token.</param>
    /// <param name="scope">The scope of the connection.</param>
    /// <param name="patreonId">The Patreon ID of the user.</param>
    /// <param name="fullName">The full name of the user.</param>
    /// <param name="pledge">The pledge amount of the user.</param>
    /// <returns></returns>
    Task<Result<PatreonConnectionEntity>> InsertConnection(string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope, long patreonId, string fullName, decimal? pledge);
    
    /// <summary>
    /// Deletes a Patreon connection record from the database.
    /// </summary>
    /// <param name="patreonConnectionId">The ID of the Patreon connection to delete.</param>
    /// <returns></returns>
    Task<Result> DeleteConnection(long patreonConnectionId);
    
    /// <summary>
    /// Selects all Patreon connection records from the database.
    /// </summary>
    /// <returns></returns>
    Task<Result<IEnumerable<PatreonConnectionEntity>>> SelectAllConnections();

    /// <summary>
    /// Selects a Patreon connection record by its ID.
    /// </summary>
    /// <param name="patreonConnectionId">The ID of the Patreon connection to select.</param>
    /// <returns></returns>
    Task<Result<PatreonConnectionEntity>> SelectConnectionById(long patreonConnectionId);
    
    /// <summary>
    /// Selects a Patreon connection record by its Patreon ID.
    /// </summary>
    /// <param name="patreonId">The Patreon ID of the connection.</param>
    /// <returns></returns>
    Task<Result<PatreonConnectionEntity>> SelectConnectionByPatreonId(long patreonId);
    
    /// <summary>
    /// Updates OAuth token fields for a Patreon connection.
    /// </summary>
    /// <param name="patreonConnectionId">The ID of the Patreon connection to update.</param>
    /// <param name="refreshToken">The new refresh token.</param>
    /// <param name="accessToken">The new access token.</param>
    /// <param name="tokenType">The new token type.</param>
    /// <param name="tokenExpiry">The new token expiry date.</param>
    /// <param name="scope">The new scope.</param>
    /// <returns></returns>
    Task<Result> UpdateConnectionTokens(long patreonConnectionId, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope);
    
    /// <summary>
    /// Updates profile fields for a Patreon connection.
    /// </summary>
    /// <param name="patreonConnectionId">The ID of the Patreon connection to update.</param>
    /// <param name="fullName">The new full name.</param>
    /// <param name="pledge">The new pledge amount.</param>
    /// <returns></returns>
    Task<Result> UpdateConnectionProfile(long patreonConnectionId, string fullName, decimal? pledge);
    
    /// <summary>
    /// Inserts a new user-Patreon connection link.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="patreonConnectionId"></param>
    /// <returns></returns>
    Task<Result<UserPatreonConnectionEntity>> InsertUserPatreonConnection(long userId, long patreonConnectionId);
    
    /// <summary>
    /// Deletes a user-Patreon connection link.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="patreonConnectionId"></param>
    /// <returns></returns>
    Task<Result> DeleteUserPatreonConnection(long userId, long patreonConnectionId);
    
    /// <summary>
    /// Gets all Patreon connections for a user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<Result<IEnumerable<UserPatreonConnectionEntity>>> SelectUserPatreonConnections(long userId);
    
    /// <summary>
    /// Gets all users linked to a Patreon connection.
    /// </summary>
    /// <param name="patreonConnectionId"></param>
    /// <returns></returns>
    Task<Result<IEnumerable<UserByPatreonConnectionEntity>>> SelectUsersByPatreonConnection(long patreonConnectionId);
    
}