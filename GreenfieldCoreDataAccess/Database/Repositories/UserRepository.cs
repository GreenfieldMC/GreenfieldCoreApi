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
    private const string SelectUsersByDiscordSnowflakeProc = "usp_SelectUsersByDiscordSnowflake";
    
    private const string InsertUserPatreonAccountProc = "usp_InsertUserPatreonAccount";
    private const string SelectUserPatreonAccountsProc = "usp_SelectUserPatreonAccounts";
    private const string UpdateUserPatreonTokensProc = "usp_UpdateUserPatreonTokens";
    private const string DeleteUserPatreonAccountProc = "usp_DeleteUserPatreonAccount";
    private const string UpdateUserPatreonPledgeProc = "usp_UpdateUserPatreonPledge";
    private const string SelectUserPatreonAccountProc = "usp_SelectUserPatreonAccount";
    
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

    public async Task<Result<IEnumerable<UserEntity>>> GetUsersByDiscordSnowflake(ulong discordSnowflake)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_DiscordSnowflake", discordSnowflake, DbType.UInt64);
        try {
            var users = await Connection.QueryAsync<UserEntity>(SelectUsersByDiscordSnowflakeProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<UserEntity>>.Success(users);
        } catch (DbException ex) {
            return Result<IEnumerable<UserEntity>>.Failure($"Failed to get users by Discord snowflake: {ex.Message}");
        }
    }

    public async Task<Result<UserPatreonEntity>> CreateUserPatreonReference(long userId, long patreonId, string refreshToken, string accessToken, string tokenType,
        DateTime tokenExpiry, string scope, decimal? pledge)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_PatreonId", patreonId, DbType.Int64);
        parameters.Add("p_RefreshToken", refreshToken, DbType.String);
        parameters.Add("p_AccessToken", accessToken, DbType.String);
        parameters.Add("p_TokenType", tokenType, DbType.String);
        parameters.Add("p_TokenExpiry", tokenExpiry, DbType.DateTime);
        parameters.Add("p_Scope", scope, DbType.String);
        parameters.Add("p_Pledge", pledge, DbType.Decimal);
        try {
            var result = await Connection.QuerySingleOrDefaultAsync<UserPatreonEntity?>(InsertUserPatreonAccountProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return result is null
                ? Result<UserPatreonEntity>.Failure("Failed to create user patreon reference.")
                : Result<UserPatreonEntity>.Success(result);
        } catch (DbException ex) {
            return Result<UserPatreonEntity>.Failure($"Failed to create user patreon reference: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserPatreonEntity>>> GetUserPatreonReferences(long userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        try {
            var result = await Connection.QueryAsync<UserPatreonEntity>(SelectUserPatreonAccountsProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<UserPatreonEntity>>.Success(result);
        } catch (DbException ex) {
            return Result<IEnumerable<UserPatreonEntity>>.Failure($"Failed to get user patreon references: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateUserPatreonTokens(long userId, long patreonId, string refreshToken, string accessToken, string tokenType,
        DateTime tokenExpiry, string scope)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_PatreonId", patreonId, DbType.Int64);
        parameters.Add("p_RefreshToken", refreshToken, DbType.String);
        parameters.Add("p_AccessToken", accessToken, DbType.String);
        parameters.Add("p_TokenType", tokenType, DbType.String);
        parameters.Add("p_TokenExpiry", tokenExpiry, DbType.DateTime);
        parameters.Add("p_Scope", scope, DbType.String);
        try {
            var affected = await Connection.ExecuteAsync(UpdateUserPatreonTokensProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(affected > 0);
        } catch (DbException ex) {
            return Result<bool>.Failure($"Failed to update user patreon tokens: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteUserPatreonReference(long userId, long patreonId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_PatreonId", patreonId, DbType.Int64);
        try {
            var affected = await Connection.ExecuteAsync(DeleteUserPatreonAccountProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(affected > 0);
        } catch (DbException ex) {
            return Result<bool>.Failure($"Failed to delete user patreon reference: {ex.Message}");
        }
    }
    
    public async Task<Result<UserPatreonEntity>> UpdateUserPatreonPledge(long userId, long patreonId, decimal? pledge)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_PatreonId", patreonId, DbType.Int64);
        parameters.Add("p_Pledge", pledge, DbType.Decimal);
        try {
            var result = await Connection.QuerySingleOrDefaultAsync<UserPatreonEntity?>(UpdateUserPatreonPledgeProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return result is null
                ? Result<UserPatreonEntity>.Failure("Failed to update user patreon pledge.")
                : Result<UserPatreonEntity>.Success(result);
        } catch (DbException ex) {
            return Result<UserPatreonEntity>.Failure($"Failed to update user patreon pledge: {ex.Message}");
        }
    }

    public async Task<Result<UserPatreonEntity>> GetUserPatreonAccount(long userId, long patreonId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_PatreonId", patreonId, DbType.Int64);
        try {
            var result = await Connection.QuerySingleOrDefaultAsync<UserPatreonEntity?>(SelectUserPatreonAccountProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return result is null
                ? Result<UserPatreonEntity>.Failure("Failed to get user patreon account.")
                : Result<UserPatreonEntity>.Success(result);
        } catch (DbException ex) {
            return Result<UserPatreonEntity>.Failure($"Failed to get user patreon account: {ex.Message}");
        }
    }
}