using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IUserPatreonRepository
{
    /// <summary>
    /// Create a reference between a user and their Patreon account.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonId">The Patreon ID</param>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="accessToken">The access token</param>
    /// <param name="tokenType">The token type</param>
    /// <param name="tokenExpiry">The token expiry date</param>
    /// <param name="scope">The scope</param>
    /// <param name="fullName"></param>
    /// <param name="pledge">The pledge amount</param>
    /// <returns></returns>
    Task<Result<UserPatreonEntity>> InsertUserPatreonReference(long userId, long patreonId, string refreshToken,
        string accessToken, string tokenType, DateTime tokenExpiry, string scope, string fullName, decimal? pledge);
    
    /// <summary>
    /// Get all Patreon references for a given user by their internal user ID.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <returns></returns>
    Task<Result<IEnumerable<UserPatreonEntity>>> SelectUserPatreonReferences(long userId);

    /// <summary>
    /// Update a user's Patreon tokens.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonId">The Patreon ID</param>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="accessToken">The access token</param>
    /// <param name="tokenType">The token type</param>
    /// <param name="tokenExpiry">The token expiry date</param>
    /// <param name="scope">The scope</param>
    /// <returns></returns>
    Task<Result<bool>> UpdateUserPatreonTokens(long userId, long patreonId, string refreshToken, string accessToken,
        string tokenType, DateTime tokenExpiry, string scope);

    /// <summary>
    /// Delete a user's Patreon reference.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonId">The Patreon ID</param>
    /// <returns></returns>
    Task<Result<bool>> DeleteUserPatreonReference(long userId, long patreonId);

    /// <summary>
    /// Update a user's Patreon pledge amount.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonId">The Patreon ID</param>
    /// <param name="fullName"></param>
    /// <param name="pledge">The new pledge amount</param>
    /// <returns></returns>
    Task<Result<UserPatreonEntity>> UpdateUserPatreonInfo(long userId, long patreonId, string fullName,
        decimal? pledge);

    /// <summary>
    /// Get a user's Patreon account by their internal user ID and Patreon ID.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonId">The Patreon ID</param>
    /// <returns></returns>
    Task<Result<UserPatreonEntity>> SelectUserPatreonAccount(long userId, long patreonId);
    
    /// <summary>
    /// Get all Patreon accounts.
    /// </summary>
    /// <returns></returns>
    Task<Result<IEnumerable<UserPatreonEntity>>> SelectAllPatreonAccounts();

    /// <summary>
    /// Get a user's Patreon account by their Patreon ID.
    /// </summary>
    /// <param name="patreonId">The Patreon ID</param>
    /// <returns></returns>
    Task<Result<IEnumerable<UserPatreonEntity>>> SelectUserPatreonAccountByPatreonId(long patreonId);
}