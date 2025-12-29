using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IPatreonService
{
    
    /// <summary>
    /// Gets all linked Patreon accounts.
    /// </summary>
    /// <returns></returns>
    public Task<Result<IEnumerable<UserPatreonAccount>>> GetAllPatreonAccounts();

    /// <summary>
    /// Links a Patreon account to a user.
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
    /// <returns>The linked UserPatreonAccount</returns>
    public Task<Result<UserPatreonAccount>> CreatePatreonAccountReference(long userId, long patreonId,
        string refreshToken,
        string accessToken, string tokenType, DateTime tokenExpiry, string scope, string fullName, decimal? pledge);

    /// <summary>
    /// Updates the tokens for a linked Patreon account.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonId">The Patreon ID</param>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="accessToken">The access token</param>
    /// <param name="tokenType">The token type</param>
    /// <param name="tokenExpiry">The token expiry date</param>
    /// <param name="scope">The scope</param>
    /// <returns></returns>
    public Task<Result<UserPatreonAccount>> UpdatePatreonAccountTokens(long userId, long patreonId, string refreshToken,
        string accessToken, string tokenType, DateTime tokenExpiry, string scope);
    
    /// <summary>
    /// Updates non-token info for a linked Patreon account.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonId">The Patreon ID</param>
    /// <param name="fullName">The full name of the Patreon account</param>
    /// <param name="pledge">The pledge amount</param>
    /// <returns></returns>
    public Task<Result<UserPatreonAccount>> UpdatePatreonAccountInfo(long userId, long patreonId, string fullName, decimal? pledge);
    
    /// <summary>
    /// Unlinks a Patreon account from a user.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonId">The Patreon ID</param>
    /// <returns></returns>
    public Task<Result> UnlinkPatreonAccountReference(long userId, long patreonId);
    
    /// <summary>
    /// Gets a linked Patreon account by Patreon ID for a given user.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonId">The Patreon ID</param>
    /// <returns></returns>
    public Task<Result<UserPatreonAccount>> GetPatreonAccountByUserIdAndPatreonId(long userId, long patreonId);
    
    /// <summary>
    /// Gets all linked Patreon accounts for a given user.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <returns>A list of linked UserPatreonAccount objects</returns>
    public Task<Result<IEnumerable<UserPatreonAccount>>> GetPatreonAccountsByUserId(long userId);
    
}