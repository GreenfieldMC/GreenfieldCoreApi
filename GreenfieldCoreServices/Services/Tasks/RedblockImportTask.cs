using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services.Tasks;

/// <summary>
/// Runs the queued redblock imports. Several workers each take the next queued import, so imports run side by side.
/// </summary>
public class RedblockImportTask(TaskStartSignalService startSignal, IServiceScopeFactory scopeFactory, RedblockImportQueue importQueue, ILogger<RedblockImportTask> logger) : BackgroundService
{
    private const int WorkerCount = 4;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await startSignal.WaitForStartAsync(stoppingToken);

        // the payloads of jobs from the last run are gone, so fail them before accepting new ones
        using (var scope = scopeFactory.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<IRedblockImportService>().FailUnfinishedImports();
        }
        importQueue.MarkReady();

        var workers = Enumerable.Range(1, WorkerCount).Select(worker => RunWorker(worker, stoppingToken));
        await Task.WhenAll(workers);
    }

    private async Task RunWorker(int worker, CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var import in importQueue.ReadAllAsync(stoppingToken))
            {
                logger.LogInformation("Import worker {Worker} picked up redblock import {ImportJobId}", worker, import.ImportJobId);
                // each import gets its own scope, so its unit of work is not shared with other imports
                using var scope = scopeFactory.CreateScope();
                await scope.ServiceProvider.GetRequiredService<IRedblockImportService>().RunBulkImport(import, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // the api is stopping. Unfinished jobs are failed on the next startup
        }
    }
}
