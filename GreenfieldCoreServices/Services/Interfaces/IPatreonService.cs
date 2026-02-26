using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Connections.Patreon;
using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IPatreonService
{
    
    /// <summary>
    /// Links a Patreon account to a user.
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="accessToken">The access token</param>
    /// <param name="tokenType">The token type</param>
    /// <param name="tokenExpiry">The token expiry date</param>
    /// <param name="scope">The scope</param>
    /// <param name="patreonId">The Patreon ID</param>
    /// <param name="fullName">The full name of the Patreon account</param>
    /// <param name="pledge">The pledge amount</param>
    /// <returns>The linked UserPatreonAccount</returns>
    public Task<Result<PatreonConnection>> CreatePatreonConnection(string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope, long patreonId, string fullName, decimal? pledge);

    /// <summary>
    /// Deletes a Patreon connection record. This will also unlink it from any users.
    /// </summary>
    /// <param name="patreonConnectionId">The ID of the Patreon connection to delete</param>
    /// <returns></returns>
    public Task<Result> DeletePatreonConnection(long patreonConnectionId);

    /// <summary>
    /// Links a user to a Patreon connection.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonConnectionId">The ID of the Patreon connection</param>
    /// <returns></returns>
    public Task<Result<UserPatreonConnection>> LinkUserToPatreonConnection(long userId, long patreonConnectionId);
    
    /// <summary>
    /// Unlinks a user from a Patreon connection.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonConnectionId">The ID of the Patreon connection</param>
    /// <returns></returns>
    public Task<Result> UnlinkUserPatreonConnection(long userId, long patreonConnectionId);

    /// <summary>
    /// Updates the tokens for a linked Patreon account.
    /// </summary>
    /// <param name="patreonConnectionId">The ID of the Patreon connection</param>
    /// <param name="refreshToken">The refresh token</param>
    /// <param name="accessToken">The access token</param>
    /// <param name="tokenType">The token type</param>
    /// <param name="tokenExpiry">The token expiry date</param>
    /// <param name="scope">The scope</param>
    /// <returns></returns>
    public Task<Result<PatreonConnection>> UpdatePatreonConnectionTokens(long patreonConnectionId, string refreshToken,
        string accessToken, string tokenType, DateTime tokenExpiry, string scope);

    /// <summary>
    /// Updates non-token info for a linked Patreon account.
    /// </summary>
    /// <param name="patreonConnectionId"></param>
    /// <param name="fullName">The full name of the Patreon account</param>
    /// <param name="pledge">The pledge amount</param>
    /// <returns></returns>
    public Task<Result<PatreonConnection>> UpdatePatreonConnectionProfile(long patreonConnectionId, string fullName, decimal? pledge);
    
    /// <summary>
    /// Gets a linked Patreon account for a given user.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <param name="patreonConnectionId">The internal Patreon connection ID</param>
    /// <returns></returns>
    public Task<Result<UserPatreonConnection>> GetUserPatreonConnection(long userId, long patreonConnectionId);
    
    /// <summary>
    /// Gets all linked Patreon accounts for a given user.
    /// </summary>
    /// <param name="userId">The internal user ID</param>
    /// <returns></returns>
    public Task<Result<IEnumerable<UserPatreonConnection>>> GetUserPatreonConnections(long userId);
    
    /// <summary>
    /// Gets all linked Patreon accounts.
    /// </summary>
    /// <returns></returns>
    public Task<Result<IEnumerable<PatreonConnection>>> GetAllPatreonConnections();

    /// <summary>
    /// Gets all linked Patreon accounts by Patreon ID.
    /// </summary>
    /// <param name="patreonId">The Patreon ID</param>
    /// <returns>>A list of linked UserPatreonAccount objects with the same PatreonId</returns>
    public Task<Result<PatreonConnection>> GetPatreonConnectionByPatreonId(long patreonId);

    /// <summary>
    /// Gets a Patreon connection by its internal ID.
    /// </summary>
    /// <param name="patreonConnectionId">The internal Patreon connection ID</param>
    /// <returns></returns>
    public Task<Result<PatreonConnection>> GetPatreonConnection(long patreonConnectionId);
    
    /// <summary>
    /// Gets all users linked to a given Patreon connection ID.
    /// </summary>
    /// <param name="patreonConnectionId">Internal Patreon connection ID.</param>
    /// <returns></returns>
    public Task<Result<IEnumerable<UserPatreonConnection>>> GetUsersByPatreonConnectionId(long patreonConnectionId);

}