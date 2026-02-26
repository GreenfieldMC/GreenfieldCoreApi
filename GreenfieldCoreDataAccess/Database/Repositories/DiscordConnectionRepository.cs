using System.Data.Common;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Procedures;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class DiscordConnectionRepository(IUnitOfWork uow, ILogger<IDiscordConnectionRepository> logger) : BaseRepository(uow), IDiscordConnectionRepository
{
    public async Task<Result<DiscordConnectionEntity>> InsertConnection(string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope, ulong discordSnowflake, string discordUsername)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Connections.Discord.InsertDiscordConnection, (refreshToken, accessToken, tokenType, tokenExpiry, scope, discordSnowflake, discordUsername), Transaction);
            return result is null
                ? Result<DiscordConnectionEntity>.Failure("Failed to insert Discord connection: No result returned.")
                : Result<DiscordConnectionEntity>.Success(result);
        } catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<DiscordConnectionEntity>.Failure($"Failed to insert Discord connection: {ex.Message}");
        }
    }

    public async Task<Result> DeleteConnection(long discordConnectionId)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Connections.Discord.DeleteDiscordConnection, discordConnectionId, Transaction);
            return rows > 0 
                ? Result.Success() 
                : Result.Failure("No rows were deleted.");
        } catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete Discord connection: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<DiscordConnectionEntity>>> SelectAllConnections()
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Connections.Discord.SelectAllDiscordConnections, Transaction);
            return Result<IEnumerable<DiscordConnectionEntity>>.Success(result);
        } catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<DiscordConnectionEntity>>.Failure($"Failed to select all Discord connections: {ex.Message}");
        }
    }

    public async Task<Result<DiscordConnectionEntity>> SelectConnectionById(long discordConnectionId)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Connections.Discord.SelectDiscordConnection, discordConnectionId, Transaction);
            return result is null
                ? Result<DiscordConnectionEntity>.Failure("Discord connection not found.")
                : Result<DiscordConnectionEntity>.Success(result);
        } catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<DiscordConnectionEntity>.Failure($"Failed to select Discord connection by ID: {ex.Message}");
        }
    }

    public async Task<Result<DiscordConnectionEntity>> SelectConnectionBySnowflake(ulong discordSnowflake)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Connections.Discord.SelectDiscordConnectionBySnowflake, discordSnowflake, Transaction);
            return result is null
                ? Result<DiscordConnectionEntity>.Failure("Discord connection not found.")
                : Result<DiscordConnectionEntity>.Success(result);
        } catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<DiscordConnectionEntity>.Failure($"Failed to select Discord connection by snowflake: {ex.Message}");
        }
    }

    public async Task<Result> UpdateConnectionTokens(long discordConnectionId, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Connections.Discord.UpdateDiscordConnectionTokens, (discordConnectionId, refreshToken, accessToken, tokenType, tokenExpiry, scope), Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No rows were updated.");
        } catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to update Discord connection tokens: {ex.Message}");
        }
    }

    public async Task<Result> UpdateConnectionProfile(long discordConnectionId, string discordUsername)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Connections.Discord.UpdateDiscordConnectionProfile, (discordConnectionId, discordUsername), Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No rows were updated.");
        } catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to update Discord connection profile: {ex.Message}");
        }
    }

    public async Task<Result<UserDiscordConnectionEntity>> InsertUserDiscordConnection(long userId, long discordConnectionId)
    {
        try 
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Users.InsertUserDiscordConnection, (userId, discordConnectionId), Transaction);
            return Result<UserDiscordConnectionEntity>.Success(result);
        } catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<UserDiscordConnectionEntity>.Failure($"Failed to insert user discord connection: {ex.Message}");
        }
    }

    public async Task<Result> DeleteUserDiscordConnection(long userId, long discordConnectionId)
    {
        try 
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Users.DeleteUserDiscordConnection, (userId, discordConnectionId), Transaction);
            return rows > 0 
                ? Result.Success() 
                : Result.Failure("No rows were deleted.");
        } catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete user discord connection: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserDiscordConnectionEntity>>> SelectUserDiscordConnections(long userId)
    {
        try 
        {
            var result = await Connection.QueryProcedure(StoredProcs.Users.SelectDiscordConnectionsByUserId, userId, Transaction);
            return Result<IEnumerable<UserDiscordConnectionEntity>>.Success(result);
        } catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<UserDiscordConnectionEntity>>.Failure($"Failed to select user discord connections: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserByDiscordConnectionEntity>>> SelectUsersByDiscordConnection(long discordConnectionId)
    {
        try 
        {
            var result = await Connection.QueryProcedure(StoredProcs.Users.SelectUsersByDiscordConnectionId, discordConnectionId, Transaction);
            return Result<IEnumerable<UserByDiscordConnectionEntity>>.Success(result);
        } catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<UserByDiscordConnectionEntity>>.Failure($"Failed to select users by discord connection: {ex.Message}");
        }
    }
}