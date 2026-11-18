namespace GreenfieldCoreDataAccess.Database.Models;

public record RedblockImportJobEntity(
    Guid ImportJobId,
    string State,
    int Total,
    string? ErrorMessage,
    DateTime CreatedOn,
    DateTime? StartedOn,
    DateTime? CompletedOn,
    long Processed,
    long Failed);

/// <summary>
/// The id and state of an inserted import entry. When claiming a legacy key, the state is Skipped if it was already claimed.
/// </summary>
public record RedblockImportEntryInsertEntity(
    long ImportEntryId,
    string State);

public record RedblockImportEntryEntity(
    long ImportEntryId,
    Guid ImportJobId,
    string LegacyKey,
    long? ProjectId,
    string State,
    long? RedblockId,
    string? Errors,
    DateTime ProcessedOn,
    string? RedblockKey);
