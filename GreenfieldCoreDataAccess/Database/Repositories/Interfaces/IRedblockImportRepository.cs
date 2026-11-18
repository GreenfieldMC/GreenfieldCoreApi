using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

/// <summary>
/// Repository methods for tracking bulk imports of legacy redblocks.
/// </summary>
public interface IRedblockImportRepository
{
    /// <summary>
    /// Create a new import job in the Queued state.
    /// </summary>
    /// <param name="importJobId">The id of the job.</param>
    /// <param name="total">The number of legacy redblocks in the import.</param>
    /// <returns>Result indicating whether the job was created.</returns>
    Task<Result> InsertImportJob(Guid importJobId, int total);

    /// <summary>
    /// Set the state of an import job.
    /// </summary>
    /// <param name="importJobId">The id of the job.</param>
    /// <param name="state">The new state: Queued, Running, Completed or Failed.</param>
    /// <param name="errorMessage">Why the whole job failed, or null.</param>
    /// <returns>Result indicating whether the job was updated.</returns>
    Task<Result> UpdateImportJobState(Guid importJobId, string state, string? errorMessage);

    /// <summary>
    /// Mark every queued or running import job as failed.
    /// </summary>
    /// <param name="errorMessage">The error message to set on the failed jobs.</param>
    /// <returns>Result containing the number of jobs that were failed.</returns>
    Task<Result<long>> FailUnfinishedImportJobs(string errorMessage);

    /// <summary>
    /// Get an import job with its processed and failed entry counts.
    /// </summary>
    /// <param name="importJobId">The id of the job.</param>
    /// <returns>Result containing the <see cref="RedblockImportJobEntity"/>, or a failed Result if the job was not found.</returns>
    Task<Result<RedblockImportJobEntity>> SelectImportJob(Guid importJobId);

    /// <summary>
    /// Insert an import entry. Inserting an Importing entry claims the legacy key for the project; if it is already
    /// claimed, a Skipped entry is inserted instead.
    /// </summary>
    /// <param name="importJobId">The id of the job.</param>
    /// <param name="legacyKey">The key of the legacy redblock.</param>
    /// <param name="projectId">The project the redblock is imported into, or null if it has none.</param>
    /// <param name="state">Importing, Skipped or Failed.</param>
    /// <param name="errors">The errors as a JSON array, or null.</param>
    /// <returns>Result containing the id and actual state of the inserted entry.</returns>
    Task<Result<RedblockImportEntryInsertEntity>> InsertImportEntry(Guid importJobId, string legacyKey, long? projectId, string state, string? errors);

    /// <summary>
    /// Finish an Importing entry.
    /// </summary>
    /// <param name="importEntryId">The id of the entry.</param>
    /// <param name="state">Imported or Failed.</param>
    /// <param name="redblockId">The created redblock, or null if none was created.</param>
    /// <param name="errors">The errors as a JSON array, or null.</param>
    /// <returns>Result indicating whether the entry was updated.</returns>
    Task<Result> UpdateImportEntry(long importEntryId, string state, long? redblockId, string? errors);

    /// <summary>
    /// Get up to a limit of a job's entries after the given entry id, in the order they were inserted.
    /// </summary>
    /// <param name="importJobId">The id of the job.</param>
    /// <param name="afterImportEntryId">Only entries after this id are returned.</param>
    /// <param name="limit">The maximum number of entries to return.</param>
    /// <returns>Result containing the entries.</returns>
    Task<Result<IEnumerable<RedblockImportEntryEntity>>> SelectImportEntries(Guid importJobId, long afterImportEntryId, int limit);
}
