using System.Data;
using System.Data.Common;
using Dapper;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class UserRepository(IUnitOfWork uow) : BaseRepository(uow), IUserRepository
{
    private const string InsertUserProc = "usp_InsertUser";
    private const string SelectUserByUserIdProc = "usp_SelectUserByUserId";
    private const string SelectUserByUuidProc = "usp_SelectUserByUuid";
    private const string UpdateUsernameProc = "usp_UpdateUsername";
    
    private const string InsertUserDiscordAccountProc = "usp_InsertUserDiscordAccount";
    private const string SelectUserDiscordAccountsProc = "usp_SelectUserDiscordAccounts";
    private const string DeleteUserDiscordAccountProc = "usp_DeleteUserDiscordAccount";
    
    public async Task<Result<UserEntity?>> GetUserByUserId(long userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        try {
            var result = await Connection.QuerySingleOrDefaultAsync<UserEntity?>(SelectUserByUserIdProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<UserEntity?>.Success(result);
        } catch (DbException ex) {
            return Result<UserEntity?>.Failure($"Failed to get user by ID: {ex.Message}");
        }
    }

    public async Task<Result<UserEntity?>> GetUserByUuid(Guid minecraftUuid)
    {
        var parameters = new DynamicParameters();
        parameters.Add("u_MinecraftUuid", minecraftUuid, DbType.Guid);
        try {
            var result = await Connection.QuerySingleOrDefaultAsync<UserEntity?>(SelectUserByUuidProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<UserEntity?>.Success(result);
        } catch (DbException ex) {
            return Result<UserEntity?>.Failure($"Failed to get user by UUID: {ex.Message}");
        }
    }

    public async Task<Result<UserEntity?>> CreateUser(Guid minecraftUuid, string minecraftUsername)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_MinecraftUuid", minecraftUuid, DbType.Guid);
        parameters.Add("p_MinecraftUsername", minecraftUsername, DbType.String, size: 16);
        try {
            var result = await Connection.QuerySingleOrDefaultAsync<UserEntity?>(
                InsertUserProc,
                parameters,
                commandType: CommandType.StoredProcedure,
                transaction: Transaction);
            return Result<UserEntity?>.Success(result);
        } catch (DbException ex) {
            return Result<UserEntity?>.Failure($"Failed to create user: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateUsername(Guid minecraftUuid, string newMinecraftUsername)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_MinecraftUuid", minecraftUuid, DbType.Guid);
        parameters.Add("p_NewUsername", newMinecraftUsername, DbType.String, size: 16);
        try {
            var rows = await Connection.ExecuteAsync(UpdateUsernameProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(rows > 0);
        } catch (DbException ex) {
            return Result<bool>.Failure($"Failed to update username: {ex.Message}");
        }
    }

    public async Task<Result<UserDiscordEntity?>> CreateUserDiscordReference(long userId, ulong discordSnowflake)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_DiscordSnowflake", discordSnowflake, DbType.UInt64);
        try {
            var result = await Connection.QuerySingleOrDefaultAsync<UserDiscordEntity?>(InsertUserDiscordAccountProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<UserDiscordEntity?>.Success(result);
        } catch (DbException ex) {
            return Result<UserDiscordEntity?>.Failure($"Failed to create user discord reference: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserDiscordEntity>>> GetUserDiscordReferences(long userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        try {
            var result = await Connection.QueryAsync<UserDiscordEntity>(SelectUserDiscordAccountsProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<UserDiscordEntity>>.Success(result);
        } catch (DbException ex) {
            return Result<IEnumerable<UserDiscordEntity>>.Failure($"Failed to get user discord references: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteUserDiscordReference(long userId, ulong discordSnowflake)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_DiscordSnowflake", discordSnowflake, DbType.UInt64);

        try
        {
            var affected = await Connection.ExecuteAsync(DeleteUserDiscordAccountProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(affected > 0);
        } catch (DbException ex)
        {
            return Result<bool>.Failure($"Failed to delete user discord reference: {ex.Message}");
        }
    }
}