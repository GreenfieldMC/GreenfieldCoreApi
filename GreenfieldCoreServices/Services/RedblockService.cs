using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Extensions;
using GreenfieldCoreServices.Models.Redblocks;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services;

public class RedblockService(IUnitOfWork uow, ILogger<IRedblockService> logger, IUserService userService, ICacheService<string, RedblockProject> projectCache, ICacheService<string, Redblock> redblockCache) : IRedblockService
{
    #region Redblock Operations
    
    public async Task<Result<Redblock>> CreateRedblock(string projectKey, int x, int y, int z, string message, long createdBy, string initialStatus, List<long> assignedUsers, List<string> assignedRoles)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<Redblock>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        if (string.IsNullOrWhiteSpace(initialStatus))
            return Result<Redblock>.Failure("Initial status cannot be empty.");
        
        if (string.IsNullOrWhiteSpace(message))
            return Result<Redblock>.Failure("Message cannot be empty.");
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();

        var insertResult = await redblockRepo.InsertRedblock(projectEntity.ProjectId, message, x, y, z, createdBy);
        
        if (!insertResult.TryGetDataNonNull(out var redblockEntity))
            return Result<Redblock>.Failure(insertResult.ErrorMessage ?? "Failed to create redblock.", insertResult.StatusCode);
        
        var insertStatus = await redblockRepo.InsertStatus(projectEntity.ProjectId, redblockEntity.KeyNumber, initialStatus, createdBy);
        if (!insertStatus.TryGetDataNonNull(out var statusEntity))
            return Result<Redblock>.Failure(insertStatus.ErrorMessage ?? "Failed to set initial status on redblock.", insertStatus.StatusCode);

        var actualAssignedUsers = new List<RedblockUserAssignment>();
        var actualAssignedRoles = new List<RedblockRoleAssignment>();
        
        foreach (var userId in assignedUsers)
        {
            var insertUserResult = await redblockRepo.InsertUserAssignment(projectEntity.ProjectId, redblockEntity.KeyNumber, userId, createdBy);
            if (!insertUserResult.TryGetDataNonNull(out var assignedUser))
                logger.LogWarning("Failed to assign user {UserId} to redblock {RedblockId} in project {ProjectId}: {ErrorMessage}", userId, redblockEntity.RedblockId, projectEntity.ProjectId, insertUserResult.ErrorMessage);
            else
                actualAssignedUsers.Add(RedblockUserAssignment.FromModel(assignedUser));
        }

        foreach (var roleName in assignedRoles)
        {
            var insertRoleResult = await redblockRepo.InsertRoleAssignment(projectEntity.ProjectId, redblockEntity.KeyNumber, roleName, createdBy);
            if (!insertRoleResult.TryGetDataNonNull(out var assignedRole))
                logger.LogWarning("Failed to assign role {RoleName} to redblock {RedblockId} in project {ProjectId}: {ErrorMessage}", roleName, redblockEntity.RedblockId, projectEntity.ProjectId, insertRoleResult.ErrorMessage);
            else
                actualAssignedRoles.Add(RedblockRoleAssignment.FromModel(assignedRole));
        }

        uow.CompleteAndCommit();
        
        var mappedRedblock = Redblock.FromModel((redblockEntity, projectEntity, [RedblockStatus.FromModel(statusEntity)], actualAssignedUsers, actualAssignedRoles, []));
        
        redblockCache.SetValue(mappedRedblock.Key, mappedRedblock);
        
        return Result<Redblock>.Success(mappedRedblock);
    }

    public async Task<Result> DeleteRedblock(string projectKey, long keyNumber, long deletedBy)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<Redblock>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var deletionResult = await redblockRepo.SoftDeleteRedblock(projectEntity.ProjectId, keyNumber, deletedBy);
        if (!deletionResult.IsSuccessful)
            return Result.Failure(deletionResult.ErrorMessage ?? "Failed to delete redblock.", deletionResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        redblockCache.RemoveValue(projectEntity.ProjectKey + "-" + keyNumber);
        
        return Result.Success();
    }

    public async Task<Result> UpdateRedblock(string projectKey, long keyNumber, string newMessage, long updatedBy)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<Redblock>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var updateResult = await redblockRepo.UpdateRedblockMessage(projectEntity.ProjectId, keyNumber, newMessage, updatedBy);
        if (!updateResult.TryGetDataNonNull(out var redblockEntity))
            return Result.Failure(updateResult.ErrorMessage ?? "Failed to update redblock message.", updateResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        redblockCache.RemoveValue(projectEntity.ProjectKey + "-" + keyNumber);
        _ = await GetRedblockByKey(projectKey, keyNumber);
        
        return Result.Success();
    }

    public async Task<Result<Redblock>> GetRedblockByKey(string projectKey, long keyNumber)
    {
        if (redblockCache.TryGetValue(projectKey + "-" + keyNumber, out var cachedRedblock))
            return Result<Redblock>.Success(cachedRedblock);
        
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<Redblock>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        
        var redblockResult = await redblockRepo.SelectRedblockByKey(projectEntity.ProjectId, keyNumber);
        if (!redblockResult.TryGetDataNonNull(out var redblockEntity))
            return Result<Redblock>.Failure(redblockResult.ErrorMessage ?? "Failed to retrieve redblock.", redblockResult.StatusCode);
        
        var statusesResult = await redblockRepo.SelectRedblockStatuses(redblockEntity.RedblockId);
        if (!statusesResult.TryGetDataNonNull(out var statusEntities))
            return Result<Redblock>.Failure(statusesResult.ErrorMessage ?? "Failed to retrieve redblock statuses.", statusesResult.StatusCode);

        var userAssignmentsResult = await redblockRepo.SelectRedblockUserAssignments(redblockEntity.RedblockId);
        if (!userAssignmentsResult.TryGetDataNonNull(out var userAssignmentEntities))
            return Result<Redblock>.Failure(userAssignmentsResult.ErrorMessage ?? "Failed to retrieve redblock user assignments.", userAssignmentsResult.StatusCode);

        var roleAssignmentsResult = await redblockRepo.SelectRedblockRoleAssignments(redblockEntity.RedblockId);
        if (!roleAssignmentsResult.TryGetDataNonNull(out var roleAssignmentEntities))
            return Result<Redblock>.Failure(roleAssignmentsResult.ErrorMessage ?? "Failed to retrieve redblock role assignments.", roleAssignmentsResult.StatusCode);

        var entitiesResult = await redblockRepo.SelectRedblockEntities(redblockEntity.RedblockId);
        if (!entitiesResult.TryGetDataNonNull(out var foundEntities))
            return Result<Redblock>.Failure(entitiesResult.ErrorMessage ?? "Failed to retrieve redblock entities.", entitiesResult.StatusCode);

        var mappedRedblock = Redblock.FromModel((redblockEntity, 
            projectEntity, 
            statusEntities.Select(RedblockStatus.FromModel).ToList(),
            userAssignmentEntities.Select(RedblockUserAssignment.FromModel).ToList(),
            roleAssignmentEntities.Select(RedblockRoleAssignment.FromModel).ToList(),
            foundEntities.ToList()));
        
        redblockCache.SetValue(mappedRedblock.Key, mappedRedblock);
        
        return Result<Redblock>.Success(mappedRedblock);
    }

    public async Task<Result<RedblockSearchResult>> SearchRedblocks(string projectKey, RedblockSearchRequest searchFilter)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockSearchResult>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        if (searchFilter.CurrentPage < 1)
            return Result<RedblockSearchResult>.Failure("Current page must be greater or equal to 1.");
        
        if (searchFilter.ResultsPerPage < 1)
            return Result<RedblockSearchResult>.Failure("Results per page must be greater than or equal to 1");
        
        if (searchFilter.Location is not null && searchFilter.Radius is not null && searchFilter.Radius < 0)
            return Result<RedblockSearchResult>.Failure("Radius filter cannot be negative.");
        
        var filterGroups = searchFilter.FilterGroups ?? [];
        if (filterGroups.Count > RedblockSearchRequest.MaxFilterGroups)
            return Result<RedblockSearchResult>.Failure($"A search can have at most {RedblockSearchRequest.MaxFilterGroups} filter groups.");

        for (var i = 0; i < filterGroups.Count; i++)
        {
            var groupError = ValidateFilterGroup(filterGroups[i]);
            if (groupError is not null)
                return Result<RedblockSearchResult>.Failure($"filterGroups[{i}]: {groupError}");
        }

        var redblockRepo = uow.Repository<IRedblockRepository>();

        var searchResult = await redblockRepo.SelectRedblocksByProject(projectEntity.ProjectId,
            searchFilter.Location, searchFilter.Radius,
            filterGroups.Select(ToFilterSet).ToList(),
            searchFilter.ResultsPerPage, searchFilter.CurrentPage);

        if (!searchResult.TryGetDataNonNull(out var actualSearchResult))
            return Result<RedblockSearchResult>.Failure(searchResult.ErrorMessage ?? "Failed to search redblocks.", searchResult.StatusCode);
        
        var mappedResults = actualSearchResult.Redblocks.Select(e => SearchedRedblockResult.FromModel((e, projectKey))).ToList();
        return Result<RedblockSearchResult>.Success(new RedblockSearchResult
        {
            Results = mappedResults,
            TotalResults = actualSearchResult.TotalResults
        });
    }

    /// <summary>
    /// Checks one filter group of a search request.
    /// </summary>
    /// <returns>An error message, or null if the group is valid.</returns>
    private static string? ValidateFilterGroup(FilterGroup group)
    {
        if (group.StatusFilter is null && group.DeletionFilter is null && group.UserAssignmentFilter is null
            && group.RoleAssignmentFilter is null && group.MessageFilter is null)
            return "A filter group must set at least one filter. To search without filters, send no filter groups.";

        if (group.StatusFilter is not null && !group.StatusFilter.MatchType.IsAnyOf("or", "not"))
            return "Status filter must have a match type of either 'or' or 'not'.";

        if (group.DeletionFilter is not null && !group.DeletionFilter.MatchType.IsAnyOf("or", "not", "and"))
            return "Deletion filter must have a match type of either 'or' or 'not' or 'and'.";

        if (group.DeletionFilter is not null && group.DeletionFilter.MatchType.IsAnyOf("and") && group.DeletionFilter.Users.Count > 0)
            return "Deletion filter cannot use the 'and' match type with users, a redblock can only be deleted by one user. Use 'and' with no users to find redblocks that are not deleted.";

        if (group.UserAssignmentFilter is not null && !group.UserAssignmentFilter.MatchType.IsAnyOf("or", "not", "and", "exact"))
            return "User assignment filter must have a match type of either 'or', 'not', 'and' or 'exact'.";

        if (group.RoleAssignmentFilter is not null && !group.RoleAssignmentFilter.MatchType.IsAnyOf("or", "not", "and", "exact"))
            return "Role assignment filter must have a match type of either 'or', 'not', 'and' or 'exact'.";

        if (group.MessageFilter is not null && !group.MessageFilter.MatchType.IsAnyOf("contains", "startsWith", "endsWith", "exact"))
            return "Message filter must have a match type of either 'contains', 'startsWith', 'endsWith', or 'exact'.";

        return null;
    }

    private static RedblockFilterSet ToFilterSet(FilterGroup group) => new()
    {
        Statuses = group.StatusFilter?.Statuses ?? [],
        StatusMatchType = group.StatusFilter?.MatchType,
        DeletionUserIds = group.DeletionFilter?.Users ?? [],
        DeletionMatchType = group.DeletionFilter?.MatchType,
        AssignedUserIds = group.UserAssignmentFilter?.Users ?? [],
        UserAssignmentMatchType = group.UserAssignmentFilter?.MatchType,
        AssignedRoles = group.RoleAssignmentFilter?.Roles ?? [],
        RoleAssignmentMatchType = group.RoleAssignmentFilter?.MatchType,
        MessageText = group.MessageFilter?.Content ?? "",
        MessageMatchType = group.MessageFilter?.MatchType
    };

    public async Task<Result<RedblockStatus>> AddRedblockStatus(string projectKey, long keyNumber, string status, long createdBy)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockStatus>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var insertStatusResult = await redblockRepo.InsertStatus(projectEntity.ProjectId, keyNumber, status, createdBy);
        if (!insertStatusResult.TryGetDataNonNull(out var statusEntity))
            return Result<RedblockStatus>.Failure(insertStatusResult.ErrorMessage ?? "Failed to add status to redblock.", insertStatusResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.Statuses.Add(RedblockStatus.FromModel(statusEntity));
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result<RedblockStatus>.Success(RedblockStatus.FromModel(statusEntity));
    }

    public async Task<Result<RedblockUserAssignment>> AddRedblockUserAssignment(string projectKey, long keyNumber, long assignedTo, long createdBy)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockUserAssignment>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var insertUserAssignmentResult = await redblockRepo.InsertUserAssignment(projectEntity.ProjectId, keyNumber, assignedTo, createdBy);
        if (!insertUserAssignmentResult.TryGetDataNonNull(out var userAssignmentEntity))
            return Result<RedblockUserAssignment>.Failure(insertUserAssignmentResult.ErrorMessage ?? "Failed to assign user to redblock.", insertUserAssignmentResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.UserAssignments.Add(RedblockUserAssignment.FromModel(userAssignmentEntity));
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result<RedblockUserAssignment>.Success(RedblockUserAssignment.FromModel(userAssignmentEntity));
    }

    public async Task<Result> RemoveRedblockUserAssignment(string projectKey, long keyNumber, long assignedTo)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockStatus>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var deletionResult = await redblockRepo.DeleteUserAssignment(projectEntity.ProjectId, keyNumber, assignedTo);
        if (!deletionResult.IsSuccessful)
            return Result.Failure(deletionResult.ErrorMessage ?? "Failed to remove user assignment from redblock.", deletionResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.UserAssignments.RemoveAll(ua => ua.AssignedTo == assignedTo);
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result.Success();
    }

    public async Task<Result<RedblockRoleAssignment>> AddRedblockRoleAssignment(string projectKey, long keyNumber, string roleName, long createdBy)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockRoleAssignment>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var insertRoleAssignmentResult = await redblockRepo.InsertRoleAssignment(projectEntity.ProjectId, keyNumber, roleName, createdBy);
        if (!insertRoleAssignmentResult.TryGetDataNonNull(out var roleAssignmentEntity))
            return Result<RedblockRoleAssignment>.Failure(insertRoleAssignmentResult.ErrorMessage ?? "Failed to assign role to redblock.", insertRoleAssignmentResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.RoleAssignments.Add(RedblockRoleAssignment.FromModel(roleAssignmentEntity));
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result<RedblockRoleAssignment>.Success(RedblockRoleAssignment.FromModel(roleAssignmentEntity));
    }

    public async Task<Result> RemoveRedblockRoleAssignment(string projectKey, long keyNumber, string roleName)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockStatus>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var deletionResult = await redblockRepo.DeleteRoleAssignment(projectEntity.ProjectId, keyNumber, roleName);
        if (!deletionResult.IsSuccessful)
            return Result.Failure(deletionResult.ErrorMessage ?? "Failed to remove role assignment from redblock.", deletionResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.RoleAssignments.RemoveAll(ra => ra.RoleName.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result.Success();
    }

    public async Task<Result<List<Guid>>> ReplaceRedblockEntities(string projectKey, long keyNumber, List<Guid> entities)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<List<Guid>>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        var distinctEntities = entities.Distinct().ToList();

        uow.BeginTransaction();

        var deleteResult = await redblockRepo.DeleteRedblockEntities(projectEntity.ProjectId, keyNumber);
        if (!deleteResult.IsSuccessful)
            return Result<List<Guid>>.Failure(deleteResult.ErrorMessage ?? "Failed to clear redblock entities.", deleteResult.StatusCode);

        foreach (var entityGuid in distinctEntities)
        {
            var insertResult = await redblockRepo.InsertRedblockEntity(projectEntity.ProjectId, keyNumber, entityGuid);
            if (!insertResult.IsSuccessful)
                return Result<List<Guid>>.Failure(insertResult.ErrorMessage ?? "Failed to replace redblock entities.", insertResult.StatusCode);
        }

        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.Entities = distinctEntities;
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result<List<Guid>>.Success(distinctEntities);
    }

    public async Task<Result> ClearRedblockEntities(string projectKey, long keyNumber)
    {
        var projectResult = await GetProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockStatus>.Failure(projectResult.ErrorMessage ?? "Project not found for given key.", projectResult.StatusCode);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();

        var redblockResult = await redblockRepo.SelectRedblockByKey(projectEntity.ProjectId, keyNumber);
        if (!redblockResult.IsSuccessful)
            return Result.Failure(redblockResult.ErrorMessage ?? "Redblock not found.", redblockResult.StatusCode);

        var deleteResult = await redblockRepo.DeleteRedblockEntities(projectEntity.ProjectId, keyNumber);
        if (!deleteResult.IsSuccessful)
            return Result.Failure(deleteResult.ErrorMessage ?? "Failed to clear redblock entities.", deleteResult.StatusCode);

        uow.CompleteAndCommit();
        
        if (redblockCache.TryGetValue(projectEntity.ProjectKey + "-" + keyNumber, out var cachedRedblock))
        {
            cachedRedblock.Entities = [];
            redblockCache.SetValue(projectEntity.ProjectKey + "-" + keyNumber, cachedRedblock);
        }
        
        return Result.Success();
    }

    #endregion
    
    #region Project Operations
    
    public async Task<Result<List<RedblockProject>>> GetProjects()
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();

        var projectsResult = await redblockRepo.SelectProjects();
        if (!projectsResult.TryGetDataNonNull(out var projectEntities))
            return Result<List<RedblockProject>>.Failure(projectsResult.ErrorMessage ?? "Failed to retrieve redblock projects.", projectsResult.StatusCode);

        var mappedProjects = projectEntities.Select(RedblockProject.FromModel).ToList();
        return Result<List<RedblockProject>>.Success(mappedProjects);
    }

    public async Task<Result<RedblockProject>> CreateProject(string projectName, string projectKey)
    {
        if (projectKey.Length > 6)
            return Result<RedblockProject>.Failure(projectKey + " is too long.");
        
        if (projectKey.Length < 2)
            return Result<RedblockProject>.Failure(projectKey + " is too short.");
        
        if (!projectKey.All(char.IsLetter))
            return Result<RedblockProject>.Failure(projectKey + " must be only letters.");
        
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var insertResult = await redblockRepo.InsertProject(projectName, projectKey.ToUpper());
        if (!insertResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockProject>.Failure(insertResult.ErrorMessage ?? "Failed to create redblock project.", insertResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        projectCache.SetValue(projectEntity.ProjectKey, RedblockProject.FromModel(projectEntity));
        
        return Result<RedblockProject>.Success(RedblockProject.FromModel(projectEntity));
    }

    public async Task<Result<RedblockProject>> GetProjectById(long projectId)
    {
        if (projectCache.TryGetValue(p => p.ProjectId == projectId, out var cachedProject))
            return Result<RedblockProject>.Success(cachedProject);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();

        var projectResult = await redblockRepo.SelectProjectById(projectId);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockProject>.Failure(projectResult.ErrorMessage ?? "Failed to retrieve redblock project.", projectResult.StatusCode);

        var mappedProject = RedblockProject.FromModel(projectEntity);
        
        projectCache.SetValue(projectEntity.ProjectKey, mappedProject);
        
        return Result<RedblockProject>.Success(mappedProject);
    }

    public async Task<Result<RedblockProject>> GetProjectByKey(string projectKey)
    {
        if (projectCache.TryGetValue(projectKey, out var cachedProject))
            return Result<RedblockProject>.Success(cachedProject);
        
        var redblockRepo = uow.Repository<IRedblockRepository>();

        var projectResult = await redblockRepo.SelectProjectByKey(projectKey);
        if (!projectResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockProject>.Failure(projectResult.ErrorMessage ?? "Failed to retrieve redblock project.", projectResult.StatusCode);

        var mappedProject = RedblockProject.FromModel(projectEntity);
        
        projectCache.SetValue(projectEntity.ProjectKey, mappedProject);
        
        return Result<RedblockProject>.Success(mappedProject);
    }

    public async Task<Result<RedblockProject>> UpdateProject(long projectId, string projectName)
    {
        var redblockRepo = uow.Repository<IRedblockRepository>();
        uow.BeginTransaction();
        
        var updateResult = await redblockRepo.UpdateProject(projectId, projectName);
        if (!updateResult.TryGetDataNonNull(out var projectEntity))
            return Result<RedblockProject>.Failure(updateResult.ErrorMessage ?? "Failed to update redblock project.", updateResult.StatusCode);
        
        uow.CompleteAndCommit();
        
        projectCache.SetValue(projectEntity.ProjectKey, RedblockProject.FromModel(projectEntity));
        
        return Result<RedblockProject>.Success(RedblockProject.FromModel(projectEntity));
    }
    
    #endregion
}