using System.Data;
using System.Data.Common;
using Dapper;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class UserPatreonRepository(IUnitOfWork uow) : BaseRepository(uow), IUserPatreonRepository
{
    
    private const string InsertUserPatreonAccountProc = "usp_InsertUserPatreonAccount";
    private const string SelectUserPatreonAccountsProc = "usp_SelectUserPatreonAccounts";
    private const string UpdateUserPatreonTokensProc = "usp_UpdateUserPatreonTokens";
    private const string DeleteUserPatreonAccountProc = "usp_DeleteUserPatreonAccount";
    private const string UpdateUserPatreonPledgeProc = "usp_UpdateUserPatreonPledge";
    private const string SelectUserPatreonAccountProc = "usp_SelectUserPatreonAccount";
    private const string SelectAllPatreonAccountsProc = "usp_SelectAllPatreonAccounts";
    private const string SelectPatreonAccountsByPatreonIdProc = "usp_SelectPatreonAccountsByPatreonId";
    
    public async Task<Result<UserPatreonEntity>> InsertUserPatreonReference(long userId, long patreonId, string refreshToken, string accessToken, string tokenType,
        DateTime tokenExpiry, string scope, string fullName, decimal? pledge)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_PatreonId", patreonId, DbType.Int64);
        parameters.Add("p_RefreshToken", refreshToken, DbType.String);
        parameters.Add("p_AccessToken", accessToken, DbType.String);
        parameters.Add("p_TokenType", tokenType, DbType.String);
        parameters.Add("p_TokenExpiry", tokenExpiry, DbType.DateTime);
        parameters.Add("p_Scope", scope, DbType.String);
        parameters.Add("p_FullName", fullName, DbType.String);
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

    public async Task<Result<IEnumerable<UserPatreonEntity>>> SelectUserPatreonReferences(long userId)
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

    public async Task<Result<UserPatreonEntity>> UpdateUserPatreonInfo(long userId, long patreonId, string fullName, decimal? pledge)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        parameters.Add("p_PatreonId", patreonId, DbType.Int64);
        parameters.Add("p_FullName", fullName, DbType.String);
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

    public async Task<Result<UserPatreonEntity>> SelectUserPatreonAccount(long userId, long patreonId)
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

    public async Task<Result<IEnumerable<UserPatreonEntity>>> SelectAllPatreonAccounts()
    {
        try {
            var result = await Connection.QueryAsync<UserPatreonEntity>(SelectAllPatreonAccountsProc, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<UserPatreonEntity>>.Success(result);
        } catch (DbException ex) {
            return Result<IEnumerable<UserPatreonEntity>>.Failure($"Failed to get all patreon accounts: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<UserPatreonEntity>>> SelectUserPatreonAccountByPatreonId(long patreonId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_PatreonId", patreonId, DbType.Int64);
        try 
        {
            var result = await Connection.QueryAsync<UserPatreonEntity>(SelectPatreonAccountsByPatreonIdProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
            return Result<IEnumerable<UserPatreonEntity>>.Success(result);
        } 
        catch (DbException ex) 
        {
            return Result<IEnumerable<UserPatreonEntity>>.Failure($"Failed to get user patreon account by patreon id: {ex.Message}");
        }
    }
}