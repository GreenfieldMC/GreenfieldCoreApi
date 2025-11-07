using Dapper;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class BuildCodeRepository(IUnitOfWork uow) : BaseRepository(uow), IBuildCodeRepository
{
    
    private const string DeleteBuildCodeProc = "usp_DeleteBuildCode";
    private const string InsertBuildCodeProc = "usp_InsertBuildCode";
    private const string SelectBuildCodeProc = "usp_SelectBuildCode";
    private const string SelectBuildCodesProc = "usp_SelectBuildCodes";
    private const string UpdateBuildCodeProc = "usp_UpdateBuildCode";
    
    public Task<BuildCodeEntity?> CreateBuildCode(int listOrder, string buildCode)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ListOrder", listOrder, System.Data.DbType.Int32);
        parameters.Add("p_BuildCode", buildCode, System.Data.DbType.String, size: 4096);
        
        return Connection.QuerySingleOrDefaultAsync<BuildCodeEntity?>(
            InsertBuildCodeProc,
            parameters,
            commandType: System.Data.CommandType.StoredProcedure,
            transaction: Transaction);
    }

    public Task<BuildCodeEntity?> GetBuildCode(long buildCodeId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_BuildCodeId", buildCodeId, System.Data.DbType.Int64);
        
        return Connection.QuerySingleOrDefaultAsync<BuildCodeEntity?>(
            SelectBuildCodeProc,
            parameters,
            commandType: System.Data.CommandType.StoredProcedure,
            transaction: Transaction);
    }

    public Task<IEnumerable<BuildCodeEntity>> GetAllBuildCodes()
    {
        return Connection.QueryAsync<BuildCodeEntity>(
            SelectBuildCodesProc,
            commandType: System.Data.CommandType.StoredProcedure,
            transaction: Transaction);
    }

    public async Task<bool> DeleteBuildCode(long buildCodeId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_BuildCodeId", buildCodeId, System.Data.DbType.Int64);
        
        return await Connection.ExecuteAsync(
            DeleteBuildCodeProc,
            parameters,
            commandType: System.Data.CommandType.StoredProcedure,
            transaction: Transaction) > 0;
    }

    public async Task<bool> UpdateBuildCode(long buildCodeId, int listOrder, string buildCode)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_BuildCodeId", buildCodeId, System.Data.DbType.Int64);
        parameters.Add("p_ListOrder", listOrder, System.Data.DbType.Int32);
        parameters.Add("p_BuildCode", buildCode, System.Data.DbType.String, size: 4096);
        
        return await Connection.ExecuteAsync(
            UpdateBuildCodeProc,
            parameters,
            commandType: System.Data.CommandType.StoredProcedure,
            transaction: Transaction) > 0;
    }
}