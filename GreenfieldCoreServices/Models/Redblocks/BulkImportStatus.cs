namespace GreenfieldCoreServices.Models.Redblocks;

/// <summary>
/// The states of a bulk import job and its entries.
/// </summary>
public static class BulkImportStates
{
    public const string Queued = "Queued";
    public const string Running = "Running";
    public const string Completed = "Completed";
    public const string Failed = "Failed";

    public const string Importing = "Importing";
    public const string Imported = "Imported";
    public const string Skipped = "Skipped";

    public static bool IsFinished(string jobState) => jobState is Completed or Failed;
}

public class BulkImportStatus
{
    public required Guid ImportJobId { get; init; }

    /// <summary>
    /// Queued, Running, Completed or Failed.
    /// </summary>
    public required string State { get; init; }

    public required int Total { get; init; }

    /// <summary>
    /// The number of legacy redblocks that are done (imported, skipped or failed).
    /// </summary>
    public required long Processed { get; init; }

    public required long Failed { get; init; }

    /// <summary>
    /// Why the whole job failed, if it did.
    /// </summary>
    public string? ErrorMessage { get; init; }

    public required DateTime CreatedOn { get; init; }
    public DateTime? StartedOn { get; init; }
    public DateTime? CompletedOn { get; init; }

    /// <summary>
    /// The finished entries after the requested cursor, in order. While the job is running, this stops at the first
    /// entry that is still importing, so nothing is skipped by the next request.
    /// </summary>
    public required List<BulkImportStatusEntry> Entries { get; init; }

    /// <summary>
    /// Pass this as the cursor of the next request to get only the entries after these.
    /// </summary>
    public required long Cursor { get; init; }

    /// <summary>
    /// Whether there are more finished entries after these. Request again with the new cursor right away to get them.
    /// </summary>
    public required bool HasMoreEntries { get; init; }
}

public class BulkImportStatusEntry
{
    public required string LegacyKey { get; init; }

    /// <summary>
    /// Imported, Skipped or Failed. Importing only appears for jobs that failed while the entry was being imported.
    /// </summary>
    public required string State { get; init; }

    /// <summary>
    /// The PROJECT-NUMBER key of the created redblock, or null if none was created.
    /// </summary>
    public string? RedblockKey { get; init; }

    public required List<string> Errors { get; init; }
}
