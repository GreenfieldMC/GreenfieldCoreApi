using System.Data;
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
    
    public Task<UserEntity?> GetUserByUserId(long userId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_UserId", userId, DbType.Int64);
        
        return Connection.QuerySingleOrDefaultAsync<UserEntity?>(SelectUserByUserIdProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }

    public Task<UserEntity?> GetUserByUuid(Guid minecraftUuid)
    {
        var parameters = new DynamicParameters();
        parameters.Add("u_MinecraftUuid", minecraftUuid, DbType.Guid);
        
        return Connection.QuerySingleOrDefaultAsync<UserEntity?>(SelectUserByUuidProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
    }

    public Task<UserEntity?> CreateUser(Guid minecraftUuid, string minecraftUsername)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_MinecraftUuid", minecraftUuid, DbType.Guid);
        parameters.Add("p_MinecraftUsername", minecraftUsername, DbType.String, size: 16);

        // Returns the created user when an insert occurred; null otherwise
        return Connection.QuerySingleOrDefaultAsync<UserEntity?>(
            InsertUserProc,
            parameters,
            commandType: CommandType.StoredProcedure,
            transaction: Transaction);
    }

    public async Task<bool> UpdateUsername(Guid minecraftUuid, string newMinecraftUsername)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_MinecraftUuid", minecraftUuid, DbType.Guid);
        parameters.Add("p_NewUsername", newMinecraftUsername, DbType.String, size: 16);
        
        var rows = await Connection.ExecuteAsync(UpdateUsernameProc, parameters, commandType: CommandType.StoredProcedure, transaction: Transaction);
        return rows > 0;
    }
}