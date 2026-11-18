using System.Data.Common;
using System.Net;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Procedures;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class RedblockImportRepository(IUnitOfWork uow, ILogger<IRedblockImportRepository> logger) : BaseRepository(uow), IRedblockImportRepository
{
    public async Task<Result> InsertImportJob(Guid importJobId, int total)
    {
        try
        {
            await Connection.ExecuteProcedure(StoredProcs.Redblocks.Imports.InsertImportJob, (importJobId, total), Transaction);
            return Result.Success();
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to insert import job: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result> UpdateImportJobState(Guid importJobId, string state, string? errorMessage)
    {
        try
        {
            await Connection.ExecuteProcedure(StoredProcs.Redblocks.Imports.UpdateImportJobState, (importJobId, state, errorMessage), Transaction);
            return Result.Success();
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to update import job state: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<long>> FailUnfinishedImportJobs(string errorMessage)
    {
        try
        {
            var failed = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.Imports.FailUnfinishedImportJobs, errorMessage, Transaction);
            return Result<long>.Success(failed);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<long>.Failure($"Failed to fail unfinished import jobs: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockImportJobEntity>> SelectImportJob(Guid importJobId)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.Imports.SelectImportJob, importJobId, Transaction);
            return result is null
                ? Result<RedblockImportJobEntity>.Failure("Import job not found.", HttpStatusCode.NotFound)
                : Result<RedblockImportJobEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockImportJobEntity>.Failure($"Failed to select import job: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockImportEntryInsertEntity>> InsertImportEntry(Guid importJobId, string legacyKey, long? projectId, string state, string? errors)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.Imports.InsertImportEntry, (importJobId, legacyKey, projectId, state, errors), Transaction);
            return result is null
                ? Result<RedblockImportEntryInsertEntity>.Failure("Failed to insert import entry: No entry returned from database.", HttpStatusCode.InternalServerError)
                : Result<RedblockImportEntryInsertEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockImportEntryInsertEntity>.Failure($"Failed to insert import entry: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result> UpdateImportEntry(long importEntryId, string state, long? redblockId, string? errors)
    {
        try
        {
            await Connection.ExecuteProcedure(StoredProcs.Redblocks.Imports.UpdateImportEntry, (importEntryId, state, redblockId, errors), Transaction);
            return Result.Success();
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to update import entry: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RedblockImportEntryEntity>>> SelectImportEntries(Guid importJobId, long afterImportEntryId, int limit)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.Imports.SelectImportEntries, (importJobId, afterImportEntryId, limit), Transaction);
            return Result<IEnumerable<RedblockImportEntryEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<RedblockImportEntryEntity>>.Failure($"Failed to select import entries: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
