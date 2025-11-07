using System.Data.Common;
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
    
    public async Task<DbResult<BuildCodeEntity?>> CreateBuildCode(int listOrder, string buildCode)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_ListOrder", listOrder, System.Data.DbType.Int32);
        parameters.Add("p_BuildCode", buildCode, System.Data.DbType.String, size: 4096);
        try {
            var result = await Connection.QuerySingleOrDefaultAsync<BuildCodeEntity?>(
                InsertBuildCodeProc,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure,
                transaction: Transaction);
            return DbResult<BuildCodeEntity?>.Success(result);
        } catch (DbException ex) {
            return DbResult<BuildCodeEntity?>.Failure($"Failed to create build code: {ex.Message}");
        }
    }

    public async Task<DbResult<BuildCodeEntity?>> GetBuildCode(long buildCodeId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_BuildCodeId", buildCodeId, System.Data.DbType.Int64);
        try {
            var result = await Connection.QuerySingleOrDefaultAsync<BuildCodeEntity?>(
                SelectBuildCodeProc,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure,
                transaction: Transaction);
            return DbResult<BuildCodeEntity?>.Success(result);
        } catch (DbException ex) {
            return DbResult<BuildCodeEntity?>.Failure($"Failed to get build code: {ex.Message}");
        }
    }

    public async Task<DbResult<IEnumerable<BuildCodeEntity>>> GetAllBuildCodes()
    {
        try {
            var result = await Connection.QueryAsync<BuildCodeEntity>(
                SelectBuildCodesProc,
                commandType: System.Data.CommandType.StoredProcedure,
                transaction: Transaction);
            return DbResult<IEnumerable<BuildCodeEntity>>.Success(result);
        } catch (DbException ex) {
            return DbResult<IEnumerable<BuildCodeEntity>>.Failure($"Failed to get all build codes: {ex.Message}");
        }
    }

    public async Task<DbResult<bool>> DeleteBuildCode(long buildCodeId)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_BuildCodeId", buildCodeId, System.Data.DbType.Int64);
        try {
            var affected = await Connection.ExecuteAsync(
                DeleteBuildCodeProc,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure,
                transaction: Transaction);
            return DbResult<bool>.Success(affected > 0);
        } catch (DbException ex) {
            return DbResult<bool>.Failure($"Failed to delete build code: {ex.Message}");
        }
    }

    public async Task<DbResult<bool>> UpdateBuildCode(long buildCodeId, int listOrder, string buildCode)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_BuildCodeId", buildCodeId, System.Data.DbType.Int64);
        parameters.Add("p_ListOrder", listOrder, System.Data.DbType.Int32);
        parameters.Add("p_BuildCode", buildCode, System.Data.DbType.String, size: 4096);
        try {
            var affected = await Connection.ExecuteAsync(
                UpdateBuildCodeProc,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure,
                transaction: Transaction);
            return DbResult<bool>.Success(affected > 0);
        } catch (DbException ex) {
            return DbResult<bool>.Failure($"Failed to update build code: {ex.Message}");
        }
    }
}