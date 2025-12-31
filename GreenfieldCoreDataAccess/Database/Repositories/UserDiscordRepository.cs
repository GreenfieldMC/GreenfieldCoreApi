using System.Data;
using System.Data.Common;
using Dapper;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class UserDiscordRepository(IUnitOfWork uow) : BaseRepository(uow), IUserDiscordRepository
{
    private const string InsertUserDiscordAccountProc = "usp_InsertUserDiscordAccount";
    private const string SelectUserDiscordAccountsProc = "usp_SelectUserDiscordAccounts";
    private const string SelectUserDiscordAccountProc = "usp_SelectUserDiscordAccount";
    private const string UpdateUserDiscordTokensProc = "usp_UpdateUserDiscordTokens";
    private const string UpdateUserDiscordProfileProc = "usp_UpdateUserDiscordProfile";
    private const string DeleteUserDiscordAccountProc = "usp_DeleteUserDiscordAccount";
    private const string SelectAllDiscordAccountsProc = "usp_SelectAllDiscordAccounts";
    private const string SelectDiscordAccountsBySnowflakeProc = "usp_SelectDiscordAccounts";

    public async Task<Result<UserDiscordEntity>> InsertUserDiscordReference(long userId, ulong discordSnowflake, string? discordUsername, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_DiscordSnowflake", discordSnowflake, DbType.UInt64);
        parameters.Add("p_DiscordUsername", discordUsername, DbType.String);
        parameters.Add("p_RefreshToken", refreshToken, DbType.String);
        parameters.Add("p_AccessToken", accessToken, DbType.String);
        parameters.Add("p_TokenType", tokenType, DbType.String);
        parameters.Add("p_TokenExpiry", tokenExpiry, DbType.DateTime);
        parameters.Add("p_Scope", scope, DbType.String);
        try
        {
            var result = await Connection.QuerySingleOrDefaultAsync<UserDiscordEntity?>(InsertUserDiscordAccountProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return result is null
                ? Result<UserDiscordEntity>.Failure("Failed to create user discord reference.")
                : Result<UserDiscordEntity>.Success(result);
        }
        catch (DbException ex)
        {
            return Result<UserDiscordEntity>.Failure($"Failed to create user discord reference: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserDiscordEntity>>> SelectUserDiscordReferences(long userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        try
        {
            var result = await Connection.QueryAsync<UserDiscordEntity>(SelectUserDiscordAccountsProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<UserDiscordEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            return Result<IEnumerable<UserDiscordEntity>>.Failure($"Failed to get user discord references: {ex.Message}");
        }
    }

    public async Task<Result<UserDiscordEntity>> SelectUserDiscordAccount(long userId, ulong discordSnowflake)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_DiscordSnowflake", discordSnowflake, DbType.UInt64);
        try
        {
            var result = await Connection.QuerySingleOrDefaultAsync<UserDiscordEntity?>(SelectUserDiscordAccountProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return result is null
                ? Result<UserDiscordEntity>.Failure("Failed to get user discord account.")
                : Result<UserDiscordEntity>.Success(result);
        }
        catch (DbException ex)
        {
            return Result<UserDiscordEntity>.Failure($"Failed to get user discord account: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateUserDiscordTokens(long userId, ulong discordSnowflake, string refreshToken, string accessToken, string tokenType, DateTime tokenExpiry, string scope)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_DiscordSnowflake", discordSnowflake, DbType.UInt64);
        parameters.Add("p_RefreshToken", refreshToken, DbType.String);
        parameters.Add("p_AccessToken", accessToken, DbType.String);
        parameters.Add("p_TokenType", tokenType, DbType.String);
        parameters.Add("p_TokenExpiry", tokenExpiry, DbType.DateTime);
        parameters.Add("p_Scope", scope, DbType.String);
        try
        {
            var affected = await Connection.ExecuteAsync(UpdateUserDiscordTokensProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(affected > 0);
        }
        catch (DbException ex)
        {
            return Result<bool>.Failure($"Failed to update user discord tokens: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateUserDiscordProfile(long userId, ulong discordSnowflake, string? discordUsername)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_DiscordSnowflake", discordSnowflake, DbType.UInt64);
        parameters.Add("p_DiscordUsername", discordUsername, DbType.String);
        try
        {
            var affected = await Connection.ExecuteAsync(UpdateUserDiscordProfileProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<bool>.Success(affected > 0);
        }
        catch (DbException ex)
        {
            return Result<bool>.Failure($"Failed to update user discord profile: {ex.Message}");
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
        }
        catch (DbException ex)
        {
            return Result<bool>.Failure($"Failed to delete user discord reference: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserDiscordEntity>>> SelectAllDiscordAccounts()
    {
        try
        {
            var result = await Connection.QueryAsync<UserDiscordEntity>(SelectAllDiscordAccountsProc, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<UserDiscordEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            return Result<IEnumerable<UserDiscordEntity>>.Failure($"Failed to get all discord accounts: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserDiscordEntity>>> SelectDiscordAccountsBySnowflake(ulong discordSnowflake)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_DiscordSnowflake", discordSnowflake, DbType.UInt64);
        try
        {
            var result = await Connection.QueryAsync<UserDiscordEntity>(SelectDiscordAccountsBySnowflakeProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<UserDiscordEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            return Result<IEnumerable<UserDiscordEntity>>.Failure($"Failed to get discord accounts by snowflake: {ex.Message}");
        }
    }
}
