using System.Net;
using System.Text.Json;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Redblocks;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services;

public class RedblockImportService(IUnitOfWork uow, ILogger<IRedblockImportService> logger, IUserService userService, IRedblockService redblockService, IServiceScopeFactory scopeFactory, RedblockImportQueue importQueue) : IRedblockImportService
{
    /// <summary>
    /// The most follow-ups a single job keeps queued before it waits for one to finish.
    /// </summary>
    private const int MaxQueuedFollowupTasks = 128;

    /// <summary>
    /// The most entries returned by one status request.
    /// </summary>
    private const int MaxStatusEntries = 500;

    public async Task<Result<Guid>> QueueBulkImport(BulkImportRedblocksRequest request)
    {
        // wait until the jobs left over from the last run are failed, so this one is not failed with them
        await importQueue.Ready;

        var importJobId = Guid.NewGuid();
        var insertResult = await uow.Repository<IRedblockImportRepository>().InsertImportJob(importJobId, request.Redblocks.Count);
        if (!insertResult.IsSuccessful)
            return Result<Guid>.Failure(insertResult.ErrorMessage ?? "Failed to create the import job.", insertResult.StatusCode);

        await importQueue.Enqueue(new QueuedRedblockImport(importJobId, request));
        logger.LogInformation("Queued redblock import {ImportJobId} with {Count} redblocks", importJobId, request.Redblocks.Count);
        return Result<Guid>.Success(importJobId, HttpStatusCode.Accepted);
    }

    public async Task<Result<BulkImportStatus>> GetBulkImportStatus(Guid importJobId, long cursor)
    {
        var importRepo = uow.Repository<IRedblockImportRepository>();

        var jobResult = await importRepo.SelectImportJob(importJobId);
        if (!jobResult.TryGetDataNonNull(out var job))
            return Result<BulkImportStatus>.Failure(jobResult.ErrorMessage ?? "Import job not found.", jobResult.StatusCode);

        var entriesResult = await importRepo.SelectImportEntries(importJobId, cursor, MaxStatusEntries);
        if (!entriesResult.TryGetDataNonNull(out var entryEntities))
            return Result<BulkImportStatus>.Failure(entriesResult.ErrorMessage ?? "Failed to load the import entries.", entriesResult.StatusCode);

        var fetched = entryEntities.ToList();
        // a job inserts its entries one at a time, so stopping at the first one still importing means the cursor never
        // passes an entry that finishes later. Once the job is finished, nothing will change, so return them all
        var finished = BulkImportStates.IsFinished(job.State)
            ? fetched
            : fetched.TakeWhile(entry => entry.State != BulkImportStates.Importing).ToList();

        return Result<BulkImportStatus>.Success(new BulkImportStatus
        {
            ImportJobId = job.ImportJobId,
            State = job.State,
            Total = job.Total,
            Processed = job.Processed,
            Failed = job.Failed,
            ErrorMessage = job.ErrorMessage,
            CreatedOn = job.CreatedOn,
            StartedOn = job.StartedOn,
            CompletedOn = job.CompletedOn,
            Entries = finished.Select(entry => new BulkImportStatusEntry
            {
                LegacyKey = entry.LegacyKey,
                State = entry.State,
                RedblockKey = entry.RedblockKey,
                Errors = entry.Errors is null ? [] : JsonSerializer.Deserialize<List<string>>(entry.Errors) ?? []
            }).ToList(),
            Cursor = finished.Count == 0 ? cursor : finished[^1].ImportEntryId,
            HasMoreEntries = finished.Count == MaxStatusEntries
        });
    }

    public async Task FailUnfinishedImports()
    {
        var failResult = await uow.Repository<IRedblockImportRepository>().FailUnfinishedImportJobs("The api stopped before the import finished.");
        if (!failResult.IsSuccessful)
            logger.LogError("Failed to fail the unfinished redblock imports: {ErrorMessage}", failResult.ErrorMessage);
        else if (failResult.Data > 0)
            logger.LogWarning("Failed {Count} redblock imports that were unfinished when the api stopped", failResult.Data);
    }

    public async Task RunBulkImport(QueuedRedblockImport import, CancellationToken cancellationToken)
    {
        var (importJobId, request) = import;
        var importRepo = uow.Repository<IRedblockImportRepository>();

        await importRepo.UpdateImportJobState(importJobId, BulkImportStates.Running, null);
        logger.LogInformation("Started redblock import {ImportJobId}", importJobId);

        try
        {
            var failure = await ImportRedblocks(importJobId, request, cancellationToken);
            await importRepo.UpdateImportJobState(importJobId, failure is null ? BulkImportStates.Completed : BulkImportStates.Failed, failure);
            if (failure is null) logger.LogInformation("Completed redblock import {ImportJobId}", importJobId);
            else logger.LogError("Redblock import {ImportJobId} failed: {ErrorMessage}", importJobId, failure);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Redblock import {ImportJobId} failed unexpectedly", importJobId);
            await importRepo.UpdateImportJobState(importJobId, BulkImportStates.Failed, Truncate($"Unexpected failure: {ex.Message}", 1024));
        }
    }

    /// <summary>
    /// Imports the redblocks of a job, recording an entry for each one.
    /// </summary>
    /// <returns>Null if the job finished, or why the whole job failed.</returns>
    private async Task<string?> ImportRedblocks(Guid importJobId, BulkImportRedblocksRequest request, CancellationToken cancellationToken)
    {
        var importRepo = uow.Repository<IRedblockImportRepository>();

        var projectMap = new Dictionary<string, RedblockProject>();
        foreach (var worldProject in request.WorldProjects)
        {
            var projectResult = await GetOrCreateProject(worldProject.Value);
            if (!projectResult.TryGetDataNonNull(out var project))
                return $"Failed to create project for world {worldProject.Key}: {projectResult.ErrorMessage}";

            projectMap[worldProject.Key] = project;
        }

        var systemUserResult = await userService.GetSystemUser();
        if (!systemUserResult.TryGetDataNonNull(out var systemUser))
            return systemUserResult.ErrorMessage ?? "System user does not exist.";

        var repo = uow.Repository<IRedblockRepository>();
        var pendingFollowupTasks = new List<Task>();

        foreach (var legacyRedblock in request.Redblocks)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var errors = new List<string>();

            if (!projectMap.TryGetValue(legacyRedblock.Value.Location.World, out var associatedProject))
            {
                errors.Add($"Redblock {legacyRedblock.Key} references world {legacyRedblock.Value.Location.World} which does not have an associated project in the import payload.");
                await InsertFinishedEntry(importRepo, importJobId, legacyRedblock.Key, null, BulkImportStates.Failed, errors);
                continue;
            }

            // claim the legacy key, so it is only ever imported into the project once
            var claimResult = await importRepo.InsertImportEntry(importJobId, legacyRedblock.Key, associatedProject.ProjectId, BulkImportStates.Importing, null);
            if (!claimResult.TryGetDataNonNull(out var claim))
            {
                errors.Add($"Failed to record the import of legacy redblock {legacyRedblock.Key}: {claimResult.ErrorMessage}");
                await InsertFinishedEntry(importRepo, importJobId, legacyRedblock.Key, associatedProject.ProjectId, BulkImportStates.Failed, errors);
                continue;
            }

            if (claim.State == BulkImportStates.Skipped)
                continue;

            User? foundAssignedUser = null;
            if (legacyRedblock.Value.AssignedTo.HasValue)
            {
                var assignedToUser = legacyRedblock.Value.AssignedTo.Value;
                var assignedToUserResult = await userService.GetUserByUuid(assignedToUser);
                if (!assignedToUserResult.TryGetDataNonNull(out foundAssignedUser))
                {
                    errors.Add($"Redblock {legacyRedblock.Key} is assigned to user {assignedToUser} who could not be found in the system.");
                }
            }

            var createdByUser = legacyRedblock.Value.CreatedBy;
            var foundCreatedByUserResult = await userService.GetUserByUuid(createdByUser);
            if (!foundCreatedByUserResult.TryGetDataNonNull(out var foundCreatedByUser))
            {
                errors.Add($"Redblock {legacyRedblock.Key} was created by user {createdByUser} who could not be found in the system.");
                foundCreatedByUser = systemUser;
            }

            var statuses = new List<(string Status, long CreatedByUserId)> { ("Incomplete", foundCreatedByUser.UserId) };

            if (legacyRedblock.Value.CompletedBy.HasValue)
            {
                var pendingByUser = legacyRedblock.Value.CompletedBy.Value;
                var pendingByUserResult = await userService.GetUserByUuid(pendingByUser);
                if (!pendingByUserResult.TryGetDataNonNull(out var foundPendingByUser))
                {
                    errors.Add($"Redblock {legacyRedblock.Key} is pending completion by user {pendingByUser} who could not be found in the system.");
                    foundPendingByUser = systemUser;
                }

                statuses.Add(("Pending", foundPendingByUser.UserId));
            }

            if (legacyRedblock.Value.ApprovedBy.HasValue)
            {
                var approvedByUser = legacyRedblock.Value.ApprovedBy.Value;
                var approvedByUserResult = await userService.GetUserByUuid(approvedByUser);
                if (!approvedByUserResult.TryGetDataNonNull(out var foundApprovedByUser))
                {
                    errors.Add($"Redblock {legacyRedblock.Key} is approved by user {approvedByUser} who could not be found in the system.");
                    foundApprovedByUser = systemUser;
                }

                statuses.Add(("Approved", foundApprovedByUser.UserId));
            }

            var role = legacyRedblock.Value.MinRank;
            var isDeleted = legacyRedblock.Value.Status.Equals("Deleted", StringComparison.OrdinalIgnoreCase);
            var entities = legacyRedblock.Value.Armorstands ?? [];

            BulkImportFollowupWorkItem followupWorkItem;
            try
            {
                uow.BeginTransaction();

                var redblockInsertResult = await repo.InsertRedblock(
                    associatedProject.ProjectId,
                    legacyRedblock.Value.Content,
                    (int)legacyRedblock.Value.Location.X,
                    (int)legacyRedblock.Value.Location.Y,
                    (int)legacyRedblock.Value.Location.Z,
                    foundCreatedByUser.UserId);

                if (!redblockInsertResult.TryGetDataNonNull(out var redblockEntity))
                {
                    errors.Add($"Failed to create redblock for legacy redblock {legacyRedblock.Key}: {redblockInsertResult.ErrorMessage}");
                    uow.Rollback();
                    await importRepo.UpdateImportEntry(claim.ImportEntryId, BulkImportStates.Failed, null, ToJson(errors));
                    continue;
                }

                uow.CompleteAndCommit();

                followupWorkItem = new BulkImportFollowupWorkItem
                {
                    ImportEntryId = claim.ImportEntryId,
                    LegacyRedblockKey = legacyRedblock.Key,
                    ProjectId = associatedProject.ProjectId,
                    RedblockId = redblockEntity.RedblockId,
                    RedblockKeyNumber = redblockEntity.KeyNumber,
                    CreatedByUserId = foundCreatedByUser.UserId,
                    AssignedUserId = foundAssignedUser?.UserId,
                    AssignedUsername = foundAssignedUser?.Username,
                    Role = role,
                    Entities = entities,
                    Statuses = statuses,
                    IsDeleted = isDeleted,
                    SystemUserId = systemUser.UserId,
                    Errors = errors
                };
            }
            catch (Exception ex)
            {
                if (uow.HasActiveTransaction)
                    uow.Rollback();

                errors.Add($"Unexpected failure while creating base redblock for legacy redblock {legacyRedblock.Key}: {ex.Message}");
                await importRepo.UpdateImportEntry(claim.ImportEntryId, BulkImportStates.Failed, null, ToJson(errors));
                continue;
            }

            pendingFollowupTasks.Add(ProcessFollowupWithThrottle(followupWorkItem));
            if (pendingFollowupTasks.Count < MaxQueuedFollowupTasks)
                continue;

            var completedTask = await Task.WhenAny(pendingFollowupTasks);
            pendingFollowupTasks.Remove(completedTask);
            await completedTask;
        }

        await Task.WhenAll(pendingFollowupTasks);
        return null;
    }

    /// <summary>
    /// Gets the project with the key, or creates it. If another job creates it at the same time, the creation here
    /// fails on the unique key, so the project is looked up again.
    /// </summary>
    private async Task<Result<RedblockProject>> GetOrCreateProject(BulkImportProject importProject)
    {
        var projectResult = await redblockService.GetProjectByKey(importProject.ProjectKey);
        if (projectResult.IsSuccessful) return projectResult;

        var creationResult = await redblockService.CreateProject(importProject.ProjectName, importProject.ProjectKey);
        if (creationResult.IsSuccessful) return creationResult;

        if (uow.HasActiveTransaction) uow.Rollback();
        var retryResult = await redblockService.GetProjectByKey(importProject.ProjectKey);
        return retryResult.IsSuccessful ? retryResult : creationResult;
    }

    private async Task ProcessFollowupWithThrottle(BulkImportFollowupWorkItem workItem)
    {
        await importQueue.FollowupLimiter.WaitAsync();
        try
        {
            await ProcessRedblockFollowupsAsync(workItem);
        }
        finally
        {
            importQueue.FollowupLimiter.Release();
        }
    }

    private async Task ProcessRedblockFollowupsAsync(BulkImportFollowupWorkItem workItem)
    {
        using var scope = scopeFactory.CreateScope();
        var scopedUow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var scopedRepo = scopedUow.Repository<IRedblockRepository>();
        var migrationIssues = workItem.Errors;

        try
        {
            logger.LogInformation("Processing follow-up work for legacy redblock {LegacyRedblockKey} with new redblock ID {RedblockId}", workItem.LegacyRedblockKey, workItem.RedblockId);
            scopedUow.BeginTransaction();

            if (workItem.Role is not null)
            {
                var roleInsertResult = await scopedRepo.InsertRoleAssignment(workItem.ProjectId, workItem.RedblockKeyNumber, workItem.Role, workItem.CreatedByUserId);
                if (!roleInsertResult.IsSuccessful)
                    migrationIssues.Add($"Failed to assign role '{workItem.Role}' to redblock {workItem.RedblockId} for legacy redblock {workItem.LegacyRedblockKey}: {roleInsertResult.ErrorMessage}");
            }

            if (workItem.AssignedUserId.HasValue)
            {
                var userAssignmentInsertResult = await scopedRepo.InsertUserAssignment(workItem.ProjectId, workItem.RedblockKeyNumber, workItem.AssignedUserId.Value, workItem.CreatedByUserId);
                if (!userAssignmentInsertResult.IsSuccessful)
                    migrationIssues.Add($"Failed to assign user '{workItem.AssignedUsername ?? workItem.AssignedUserId.Value.ToString()}' to redblock {workItem.RedblockId} for legacy redblock {workItem.LegacyRedblockKey}: {userAssignmentInsertResult.ErrorMessage}");
            }

            foreach (var entity in workItem.Entities)
            {
                var entityInsertResult = await scopedRepo.InsertRedblockEntity(workItem.ProjectId, workItem.RedblockKeyNumber, entity);
                if (!entityInsertResult.IsSuccessful)
                    migrationIssues.Add($"Failed to add entity {entity} to redblock {workItem.RedblockId} for legacy redblock {workItem.LegacyRedblockKey}: {entityInsertResult.ErrorMessage}");
            }

            foreach (var status in workItem.Statuses)
            {
                await Task.Delay(1000);
                var statusInsertResult = await scopedRepo.InsertStatus(workItem.ProjectId, workItem.RedblockKeyNumber, status.Status, status.CreatedByUserId);
                if (!statusInsertResult.IsSuccessful)
                    migrationIssues.Add($"Failed to add status '{status.Status}' to redblock {workItem.RedblockId} for legacy redblock {workItem.LegacyRedblockKey}: {statusInsertResult.ErrorMessage}");
            }

            if (workItem.IsDeleted)
            {
                var deletionResult = await scopedRepo.SoftDeleteRedblock(workItem.ProjectId, workItem.RedblockKeyNumber, workItem.SystemUserId);
                if (!deletionResult.IsSuccessful)
                    migrationIssues.Add($"Failed to delete redblock {workItem.RedblockId} for legacy redblock {workItem.LegacyRedblockKey}: {deletionResult.ErrorMessage}");
            }

            scopedUow.CompleteAndCommit();
            logger.LogInformation("Completed follow-up work for legacy redblock {LegacyRedblockKey} with new redblock ID {RedblockId}", workItem.LegacyRedblockKey, workItem.RedblockId);
        }
        catch (Exception ex)
        {
            if (scopedUow.HasActiveTransaction)
                scopedUow.Rollback();

            migrationIssues.Add($"Unexpected failure while running follow-up inserts for legacy redblock {workItem.LegacyRedblockKey}: {ex.Message}");
        }

        // the redblock exists whether or not every follow-up worked, so the entry is imported, with any errors
        var entryResult = await scopedUow.Repository<IRedblockImportRepository>().UpdateImportEntry(workItem.ImportEntryId, BulkImportStates.Imported, workItem.RedblockId, ToJson(migrationIssues));
        if (!entryResult.IsSuccessful)
            logger.LogError("Failed to record the import of legacy redblock {LegacyRedblockKey}: {ErrorMessage}", workItem.LegacyRedblockKey, entryResult.ErrorMessage);
    }

    private async Task InsertFinishedEntry(IRedblockImportRepository importRepo, Guid importJobId, string legacyKey, long? projectId, string state, List<string> errors)
    {
        var result = await importRepo.InsertImportEntry(importJobId, legacyKey, projectId, state, ToJson(errors));
        if (!result.IsSuccessful)
            logger.LogError("Failed to record the import of legacy redblock {LegacyRedblockKey}: {ErrorMessage}", legacyKey, result.ErrorMessage);
    }

    private static string? ToJson(List<string> errors) => errors.Count == 0 ? null : JsonSerializer.Serialize(errors);

    private static string Truncate(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];
}
