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
}