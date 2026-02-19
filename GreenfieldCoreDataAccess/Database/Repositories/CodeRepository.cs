using System.Data.Common;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Procedures;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class CodeRepository(IUnitOfWork uow, ILogger<ICodeRepository> logger) : BaseRepository(uow), ICodeRepository
{
    
    public async Task<Result<BuildCodeEntity?>> InsertCode(int listOrder, string buildCode)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.BuildCodes.InsertCode, (listOrder, buildCode), Transaction); 
            return Result<BuildCodeEntity?>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<BuildCodeEntity?>.Failure($"Failed to create build code: {ex.Message}");
        }
    }

    public async Task<Result<BuildCodeEntity?>> SelectCode(long buildCodeId)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.BuildCodes.SelectCode, buildCodeId, Transaction);
            return Result<BuildCodeEntity?>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<BuildCodeEntity?>.Failure($"Failed to get build code by ID: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<BuildCodeEntity>>> SelectCodes()
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.BuildCodes.SelectCodes, Transaction);
            return Result<IEnumerable<BuildCodeEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<BuildCodeEntity>>.Failure($"Failed to get build codes: {ex.Message}");
        }
    }

    public async Task<Result> DeleteCode(long buildCodeId)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.BuildCodes.DeleteCode, buildCodeId, Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No rows were deleted.");
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete build code: {ex.Message}");
        }
    }

    public async Task<Result> UpdateCode(long buildCodeId, int listOrder, string buildCode)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.BuildCodes.UpdateCode, (buildCodeId, listOrder, buildCode), Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No rows were updated.");
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to update build code: {ex.Message}");
        }
    }
}