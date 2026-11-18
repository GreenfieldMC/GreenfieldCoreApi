using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Redblocks;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IRedblockImportService
{
    /// <summary>
    /// Creates an import job for the legacy redblocks and queues it. The import runs in the background.
    /// </summary>
    /// <param name="request">Bulk redblock import request.</param>
    /// <returns>A result containing the id of the import job.</returns>
    Task<Result<Guid>> QueueBulkImport(BulkImportRedblocksRequest request);

    /// <summary>
    /// Gets the status of an import job, and its finished entries after the cursor.
    /// </summary>
    /// <param name="importJobId">The id of the import job.</param>
    /// <param name="cursor">The cursor from the previous status, or 0 to start from the first entry.</param>
    /// <returns>A result containing the status, or a 404 failure if the job does not exist.</returns>
    Task<Result<BulkImportStatus>> GetBulkImportStatus(Guid importJobId, long cursor);

    /// <summary>
    /// Runs a queued import job. Only called by the import workers.
    /// </summary>
    /// <param name="import">The queued import.</param>
    /// <param name="cancellationToken">Cancelled when the api is stopping.</param>
    Task RunBulkImport(QueuedRedblockImport import, CancellationToken cancellationToken);

    /// <summary>
    /// Fails every import job that was queued or running when the api last stopped, since their payloads are gone.
    /// Only called by the import workers on startup.
    /// </summary>
    Task FailUnfinishedImports();
}
