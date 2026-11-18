using System.Threading.Channels;
using GreenfieldCoreServices.Models.Redblocks;

namespace GreenfieldCoreServices.Services;

/// <summary>
/// Holds the queued bulk imports until a <see cref="Tasks.RedblockImportTask"/> worker picks them up. The payloads only
/// live in memory, which is why unfinished jobs are failed when the api starts.
/// </summary>
public class RedblockImportQueue
{
    /// <summary>
    /// The most follow-up inserts (statuses, assignments, entities) running at once, across every import job.
    /// </summary>
    private const int MaxParallelFollowups = 16;

    private readonly Channel<QueuedRedblockImport> _channel = Channel.CreateUnbounded<QueuedRedblockImport>();
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Limits the follow-up inserts of all running import jobs together, so more jobs do not mean more database load.
    /// </summary>
    public SemaphoreSlim FollowupLimiter { get; } = new(MaxParallelFollowups);

    /// <summary>
    /// Completes once the jobs left over from the last run have been failed. New jobs wait for this, so they are not
    /// failed along with them.
    /// </summary>
    public Task Ready => _ready.Task;

    public void MarkReady() => _ready.TrySetResult();

    public ValueTask Enqueue(QueuedRedblockImport import) => _channel.Writer.WriteAsync(import);

    public IAsyncEnumerable<QueuedRedblockImport> ReadAllAsync(CancellationToken cancellationToken) => _channel.Reader.ReadAllAsync(cancellationToken);
}

public record QueuedRedblockImport(Guid ImportJobId, BulkImportRedblocksRequest Request);
