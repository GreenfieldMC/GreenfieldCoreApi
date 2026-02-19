using System.Data.Common;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Procedures;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class PatreonConnectionRepository(IUnitOfWork uow, ILogger<IPatreonConnectionRepository> logger) : BaseRepository(uow), IPatreonConnectionRepository
{
    public async Task<Result<PatreonConnectionEntity>> InsertConnection(string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope, long patreonId, string fullName, decimal? pledge)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Connections.Patreon.InsertPatreonConnection, (refreshToken, accessToken, tokenType, tokenExpiry, scope, patreonId, fullName, pledge), Transaction);
            return result is null
                ? Result<PatreonConnectionEntity>.Failure("Failed to insert Patreon connection: No result returned.")
                : Result<PatreonConnectionEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<PatreonConnectionEntity>.Failure($"Failed to insert Patreon connection: {ex.Message}");
        }
    }

    public async Task<Result> DeleteConnection(long patreonConnectionId)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Connections.Patreon.DeletePatreonConnection, patreonConnectionId, Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No rows were deleted.");
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete Patreon connection: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<PatreonConnectionEntity>>> SelectAllConnections()
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Connections.Patreon.SelectAllPatreonConnections, Transaction);
            return Result<IEnumerable<PatreonConnectionEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<PatreonConnectionEntity>>.Failure($"Failed to select all Patreon connections: {ex.Message}");
        }
    }

    public async Task<Result<PatreonConnectionEntity>> SelectConnectionById(long patreonConnectionId)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Connections.Patreon.SelectPatreonConnection, patreonConnectionId, Transaction);
            return result is null
                ? Result<PatreonConnectionEntity>.Failure("Patreon connection not found.")
                : Result<PatreonConnectionEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<PatreonConnectionEntity>.Failure($"Failed to select Patreon connection by ID: {ex.Message}");
        }
    }

    public async Task<Result<PatreonConnectionEntity>> SelectConnectionByPatreonId(long patreonId)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Connections.Patreon.SelectPatreonConnectionByPatreonId, patreonId, Transaction);
            return result is null
                ? Result<PatreonConnectionEntity>.Failure("Patreon connection not found.")
                : Result<PatreonConnectionEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<PatreonConnectionEntity>.Failure($"Failed to select Patreon connection by Patreon ID: {ex.Message}");
        }
    }

    public async Task<Result> UpdateConnectionTokens(long patreonConnectionId, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Connections.Patreon.UpdatePatreonConnectionTokens, (patreonConnectionId, refreshToken, accessToken, tokenType, tokenExpiry, scope), Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No rows were updated.");
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to update Patreon connection tokens: {ex.Message}");
        }
    }

    public async Task<Result> UpdateConnectionProfile(long patreonConnectionId, string fullName, decimal? pledge)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Connections.Patreon.UpdatePatreonConnectionProfile, (patreonConnectionId, fullName, pledge), Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No rows were updated.");
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to update Patreon connection profile: {ex.Message}");
        }
    }

    public async Task<Result<UserPatreonConnectionEntity>> InsertUserPatreonConnection(long userId, long patreonConnectionId)
    {
        try 
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Users.InsertUserPatreonConnection, (userId, patreonConnectionId), Transaction);
            return Result<UserPatreonConnectionEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<UserPatreonConnectionEntity>.Failure($"Failed to insert user Patreon connection: {ex.Message}");
        }
    }

    public async Task<Result> DeleteUserPatreonConnection(long userId, long patreonConnectionId)
    {
        try 
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Users.DeleteUserPatreonConnection, (userId, patreonConnectionId), Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No rows were deleted.");
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete user Patreon connection: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserPatreonConnectionEntity>>> SelectUserPatreonConnections(long userId)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Users.SelectPatreonConnectionsByUserId, userId, Transaction);
            return Result<IEnumerable<UserPatreonConnectionEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<UserPatreonConnectionEntity>>.Failure($"Failed to select user Patreon connections: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserByPatreonConnectionEntity>>> SelectUsersByPatreonConnection(long patreonConnectionId)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Users.SelectUsersByPatreonConnectionId, patreonConnectionId, Transaction);
            return Result<IEnumerable<UserByPatreonConnectionEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<UserByPatreonConnectionEntity>>.Failure($"Failed to select users by Patreon connection: {ex.Message}");
        }
    }
}