using System.Data.Common;
using System.Net;
using Dapper;
using GreenfieldCoreDataAccess.Database.Helpers;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Procedures;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class RedblockRepository(IUnitOfWork uow, ILogger<IRedblockRepository> logger) : BaseRepository(uow), IRedblockRepository
{
    public async Task<Result<RedblockProjectEntity>> InsertProject(string projectName, string projectKey)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertProject, (projectName, projectKey), Transaction);
            return result is null
                ? Result<RedblockProjectEntity>.Failure("Failed to insert redblock project: No project returned from database.", HttpStatusCode.InternalServerError)
                : Result<RedblockProjectEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockProjectEntity>.Failure($"Failed to insert redblock project: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockProjectEntity>> UpdateProject(long projectId, string projectName)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.UpdateProject, (projectId, projectName), Transaction);
            return result is null
                ? Result<RedblockProjectEntity>.Failure("Redblock project not found.", HttpStatusCode.NotFound)
                : Result<RedblockProjectEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockProjectEntity>.Failure($"Failed to update redblock project: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RedblockProjectEntity>>> SelectProjects()
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.SelectProjects, Transaction);
            return Result<IEnumerable<RedblockProjectEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<RedblockProjectEntity>>.Failure($"Failed to select redblock projects: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockProjectEntity>> SelectProjectById(long projectId)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SelectProjectById, projectId, Transaction);
            return result is null
                ? Result<RedblockProjectEntity>.Failure("Redblock project not found.", HttpStatusCode.NotFound)
                : Result<RedblockProjectEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockProjectEntity>.Failure($"Failed to select redblock project: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockProjectEntity>> SelectProjectByKey(string projectKey)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SelectProjectByKey, projectKey, Transaction);
            return result is null
                ? Result<RedblockProjectEntity>.Failure("Redblock project not found.", HttpStatusCode.NotFound)
                : Result<RedblockProjectEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockProjectEntity>.Failure($"Failed to select redblock project: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockEntity>> InsertRedblock(long projectId, string message, int x, int y, int z, long createdBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertRedblock, (projectId, message, x, y, z, createdBy), Transaction);
            return result is null
                ? Result<RedblockEntity>.Failure("Failed to insert redblock: No redblock returned from database.", HttpStatusCode.InternalServerError)
                : Result<RedblockEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockEntity>.Failure($"Failed to insert redblock: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockWithLatestStatusEntity>> SelectRedblockByKey(long projectId, long keyNumber)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SelectRedblockByKey, (projectId, keyNumber), Transaction);
            return result is null
                ? Result<RedblockWithLatestStatusEntity>.Failure("Redblock not found.", HttpStatusCode.NotFound)
                : Result<RedblockWithLatestStatusEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockWithLatestStatusEntity>.Failure($"Failed to select redblock: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<(IEnumerable<RedblockWithLatestStatusEntity> Redblocks, long TotalResults)>>
        SelectRedblocksByProject(long projectId,
            Location? location,
            long? radius,
            List<RedblockFilterSet> filterGroups,
            int resultsPerPage, long currentPage)
    {
        try
        {
            var statementBuilder = StatementBuilder
                .SelectFrom("`Redblocks.Redblocks` rb")
                .Columns("""
                         rb.RedblockId
                         ,rb.ProjectId 
                         ,rb.KeyNumber
                         ,rb.Message
                         ,rb.CurrentStatus AS Status
                         ,rb.X
                         ,rb.Y
                         ,rb.Z
                         ,rb.CreatedBy
                         ,rb.CreatedOn
                         ,rb.UpdatedBy
                         ,rb.UpdatedOn
                         ,rb.DeletedBy
                         ,rb.DeletedOn
                         """)
                .WithParameter("@ProjectId", projectId)
                .Where("rb.ProjectId = @ProjectId")
                .WithLimit(resultsPerPage)
                .WithOffset(resultsPerPage * (currentPage - 1));

            var filterGroupsStatementPart = BuildFilterGroupsStatementPart(filterGroups);
            if (filterGroupsStatementPart != null) statementBuilder.WithPart(filterGroupsStatementPart);

            var spatialStatementPart = BuildSpatialStatementParts(location, radius);
            if (spatialStatementPart != null) statementBuilder.WithPart(spatialStatementPart);

            var countStatement = statementBuilder.BuildCount();
            Console.WriteLine(countStatement.query);
            var totalResults = await Connection.ExecuteScalarAsync<long>(countStatement.query, countStatement.parameters, Transaction);
            
            var statement = statementBuilder.Build();
            
            var redblocks = (await Connection.QueryAsync<RedblockWithLatestStatusEntity>(statement.query, statement.parameters, Transaction)).ToList();
            
            return Result<(IEnumerable<RedblockWithLatestStatusEntity> Redblocks, long TotalResults)>.Success((redblocks, totalResults));
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<(IEnumerable<RedblockWithLatestStatusEntity> Redblocks, long TotalResults)>.Failure($"Failed to select redblocks: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    #region Query String Builders

    private static StatementPart? BuildSpatialStatementParts(Location? location, long? radius)
    {
        if (location == null) return null;
        var statementPart = new StatementPartBuilder()
            .WithParameter("@LocationX", location.Value.X)
            .WithParameter("@LocationY", location.Value.Y)
            .WithParameter("@LocationZ", location.Value.Z)
            .Columns("POW(rb.X - @LocationX, 2) + POW(rb.Y - @LocationY, 2) + POW(rb.Z - @LocationZ, 2) AS DistanceSquared")
            .OrderBy("DistanceSquared ASC");

        if (radius is null) return statementPart.Build();

        // the bounding box lets the (ProjectId, X, Y, Z) index skip far away redblocks before any distances are computed,
        // the having clause then trims the box down to the actual sphere
        statementPart.WithParameter("@Radius", radius)
            .WithParameter("@RadiusSquared", radius.Value * radius.Value)
            .Where("""
                   AND rb.X BETWEEN @LocationX - @Radius AND @LocationX + @Radius
                   AND rb.Y BETWEEN @LocationY - @Radius AND @LocationY + @Radius
                   AND rb.Z BETWEEN @LocationZ - @Radius AND @LocationZ + @Radius
                   """)
            .Having("DistanceSquared <= @RadiusSquared");

        return statementPart.Build();
    }
    
    /// <summary>
    /// Combines the filter groups into one WHERE part: AND ((group 0) OR (group 1) OR ...), where every filter inside a group must match.
    /// Each group's parameters are prefixed with @g{index}_ so groups using the same kind of filter don't overwrite each other's parameters.
    /// </summary>
    private static StatementPart? BuildFilterGroupsStatementPart(List<RedblockFilterSet> filterGroups)
    {
        if (filterGroups.Count == 0) return null;
        var builder = new StatementPartBuilder();
        var groupConditions = new List<string>();

        for (var i = 0; i < filterGroups.Count; i++)
        {
            var group = filterGroups[i];
            var prefix = $"@g{i}_";
            var conditions = new[]
            {
                BuildStatusCondition(group.Statuses, group.StatusMatchType, prefix),
                BuildDeletionCondition(group.DeletionUserIds, group.DeletionMatchType, prefix),
                BuildUserAssignmentCondition(group.AssignedUserIds, group.UserAssignmentMatchType, prefix),
                BuildRoleAssignmentCondition(group.AssignedRoles, group.RoleAssignmentMatchType, prefix),
                BuildMessageCondition(group.MessageText, group.MessageMatchType, prefix)
            }.OfType<StatementPart>().ToList();

            // a group whose filters don't restrict anything (e.g. only "or" + []) matches every redblock, so the whole OR does too
            if (conditions.Count == 0) return null;

            groupConditions.Add($"({string.Join(" AND ", conditions.Select(c => c.WherePart))})");
            foreach (var parameter in conditions.SelectMany(c => c.Parameters))
                builder.WithParameter(parameter.Key, parameter.Value);
        }

        return builder.Where($"AND ({string.Join(" OR ", groupConditions)})").Build();
    }

    // The Build*Condition methods return a part whose WherePart is a bare condition (no leading AND), or null when the filter
    // doesn't restrict anything. They never use joins, so any condition can sit on either side of an OR.

    private static StatementPart? BuildStatusCondition(List<string> statuses, string? matchType, string prefix)
    {
        if (string.IsNullOrWhiteSpace(matchType)) return null;
        var builder = new StatementPartBuilder();

        foreach (var status in statuses)
            builder.WithIndexedParameter($"{prefix}Status", status);

        var inClause = builder.Build().JoinParameterKeys();

        // a redblock has at most one current status, so a non-empty list only ever matches redblocks that have a status

        // or + [] = return regardless of status
        // or + [...] = return redblocks whose current status is any of the specified statuses
        if (matchType.Equals("or", StringComparison.OrdinalIgnoreCase))
            return statuses.Count == 0
                ? null
                : builder.Where($"rb.CurrentStatus IN ({inClause})").Build();

        // not + [] = return only redblocks that have a status
        // not + [...] = return redblocks that have a status other than the specified statuses
        if (matchType.Equals("not", StringComparison.OrdinalIgnoreCase))
            return statuses.Count == 0
                ? builder.Where("rb.CurrentStatus IS NOT NULL").Build()
                : builder.Where($"rb.CurrentStatus NOT IN ({inClause})").Build();

        return null;
    }

    private static StatementPart? BuildDeletionCondition(List<long> deletionUserIds, string? matchType, string prefix)
    {
        if (string.IsNullOrWhiteSpace(matchType)) return null;
        var builder = new StatementPartBuilder();

        foreach (var deletionUserId in deletionUserIds)
            builder.WithIndexedParameter($"{prefix}DeletionUserId", deletionUserId);

        var inClause = builder.Build().JoinParameterKeys();

        // a redblock has at most one deleter, so a non-empty list only ever matches deleted redblocks.
        // redblocks that are not deleted can only be selected with an empty list ("and" + [])

        // or + [] = return regardless of deletion
        // or + [...] = return redblocks deleted by any of the specified users
        if (matchType.Equals("or", StringComparison.OrdinalIgnoreCase))
            return deletionUserIds.Count == 0
                ? null
                : builder.Where($"rb.DeletedBy IN ({inClause})").Build();

        // not + [] = return only deleted redblocks
        // not + [...] = return deleted redblocks that were not deleted by any of the specified users
        if (matchType.Equals("not", StringComparison.OrdinalIgnoreCase))
            return deletionUserIds.Count == 0
                ? builder.Where("rb.DeletedBy IS NOT NULL").Build()
                : builder.Where($"rb.DeletedBy NOT IN ({inClause})").Build();

        // and + [] = return only redblocks that are not deleted
        // and + [...] = rejected by the service, a redblock cannot be deleted by multiple users
        if (matchType.Equals("and", StringComparison.OrdinalIgnoreCase))
            return deletionUserIds.Count == 0
                ? builder.Where("rb.DeletedBy IS NULL").Build()
                : null;

        return null;
    }

    private static StatementPart? BuildUserAssignmentCondition(List<long> userIds, string? matchType, string prefix)
    {
        if (string.IsNullOrWhiteSpace(matchType)) return null;
        var builder = new StatementPartBuilder();

        foreach (var userId in userIds)
            builder.WithIndexedParameter($"{prefix}UserId", userId);

        var inClause = builder.Build().JoinParameterKeys();
        const string noUsers = "NOT EXISTS (SELECT 1 FROM `Redblocks.UserAssignments` rua WHERE rua.RedblockId = rb.RedblockId)";
        var hasAllUsers = $"rb.RedblockId IN (SELECT rua.RedblockId FROM `Redblocks.UserAssignments` rua WHERE rua.AssignedTo IN ({inClause}) GROUP BY rua.RedblockId HAVING COUNT(DISTINCT rua.AssignedTo) = {userIds.Count})";

        // or + [] = return regardless of assignment status
        // or + [...] = return redblocks with at least one of the specified user assignments
        if (matchType.Equals("or", StringComparison.OrdinalIgnoreCase))
            return userIds.Count == 0
                ? null
                : builder.Where($"EXISTS (SELECT 1 FROM `Redblocks.UserAssignments` rua WHERE rua.RedblockId = rb.RedblockId AND rua.AssignedTo IN ({inClause}))").Build();

        // not + [] = return only redblocks with user assignments ("not empty")
        // not + [...] = return redblocks with no user assignments or user assignments that do not include any of the specified users
        if (matchType.Equals("not", StringComparison.OrdinalIgnoreCase))
            return userIds.Count == 0
                ? builder.Where("EXISTS (SELECT 1 FROM `Redblocks.UserAssignments` rua WHERE rua.RedblockId = rb.RedblockId)").Build()
                : builder.Where($"NOT EXISTS (SELECT 1 FROM `Redblocks.UserAssignments` rua WHERE rua.RedblockId = rb.RedblockId AND rua.AssignedTo IN ({inClause}))").Build();

        // and + [] = return redblocks with no users assigned ("empty")
        // and + [...] = return redblocks that have all the specified users assigned
        if (matchType.Equals("and", StringComparison.OrdinalIgnoreCase))
            return userIds.Count == 0
                ? builder.Where(noUsers).Build()
                : builder.Where(hasAllUsers).Build();

        // exact + [] = return redblocks with no users assigned ("empty")
        // exact + [...] = return redblocks that have all the specified users assigned and no other users
        if (matchType.Equals("exact", StringComparison.OrdinalIgnoreCase))
            return userIds.Count == 0
                ? builder.Where(noUsers).Build()
                : builder.Where($"({hasAllUsers} AND NOT EXISTS (SELECT 1 FROM `Redblocks.UserAssignments` rua_other WHERE rua_other.RedblockId = rb.RedblockId AND rua_other.AssignedTo NOT IN ({inClause})))").Build();

        return null;
    }

    private static StatementPart? BuildRoleAssignmentCondition(List<string> roles, string? matchType, string prefix)
    {
        if (string.IsNullOrWhiteSpace(matchType)) return null;
        var builder = new StatementPartBuilder();

        foreach (var role in roles)
            builder.WithIndexedParameter($"{prefix}Role", role);

        var inClause = builder.Build().JoinParameterKeys();
        const string noRoles = "NOT EXISTS (SELECT 1 FROM `Redblocks.RoleAssignments` rra WHERE rra.RedblockId = rb.RedblockId)";
        var hasAllRoles = $"rb.RedblockId IN (SELECT rra.RedblockId FROM `Redblocks.RoleAssignments` rra WHERE rra.RoleName IN ({inClause}) GROUP BY rra.RedblockId HAVING COUNT(DISTINCT rra.RoleName) = {roles.Count})";

        // or + [] = return regardless of role assignment status
        // or + [...] = return redblocks with at least one of the specified role assignments
        if (matchType.Equals("or", StringComparison.OrdinalIgnoreCase))
            return roles.Count == 0
                ? null
                : builder.Where($"EXISTS (SELECT 1 FROM `Redblocks.RoleAssignments` rra WHERE rra.RedblockId = rb.RedblockId AND rra.RoleName IN ({inClause}))").Build();

        // not + [] = return only redblocks with role assignments ("not empty")
        // not + [...] = return redblocks with no role assignments or role assignments that do not include any of the specified roles
        if (matchType.Equals("not", StringComparison.OrdinalIgnoreCase))
            return roles.Count == 0
                ? builder.Where("EXISTS (SELECT 1 FROM `Redblocks.RoleAssignments` rra WHERE rra.RedblockId = rb.RedblockId)").Build()
                : builder.Where($"NOT EXISTS (SELECT 1 FROM `Redblocks.RoleAssignments` rra WHERE rra.RedblockId = rb.RedblockId AND rra.RoleName IN ({inClause}))").Build();

        // and + [] = return redblocks with no roles assigned ("empty")
        // and + [...] = return redblocks that have all the specified roles assigned
        if (matchType.Equals("and", StringComparison.OrdinalIgnoreCase))
            return roles.Count == 0
                ? builder.Where(noRoles).Build()
                : builder.Where(hasAllRoles).Build();

        // exact + [] = return redblocks with no roles assigned ("empty")
        // exact + [...] = return redblocks that have all the specified roles assigned and no other roles
        if (matchType.Equals("exact", StringComparison.OrdinalIgnoreCase))
            return roles.Count == 0
                ? builder.Where(noRoles).Build()
                : builder.Where($"({hasAllRoles} AND NOT EXISTS (SELECT 1 FROM `Redblocks.RoleAssignments` rra_other WHERE rra_other.RedblockId = rb.RedblockId AND rra_other.RoleName NOT IN ({inClause})))").Build();

        return null;
    }

    private static StatementPart? BuildMessageCondition(string searchText, string? matchType, string prefix)
    {
        if (string.IsNullOrWhiteSpace(matchType)) return null;
        var parameter = $"{prefix}SearchText";
        var builder = new StatementPartBuilder().WithParameter(parameter, searchText);

        if (matchType.Equals("contains", StringComparison.OrdinalIgnoreCase))
            return builder.Where($"rb.Message LIKE CONCAT('%', {parameter}, '%')").Build();

        if (matchType.Equals("startsWith", StringComparison.OrdinalIgnoreCase))
            return builder.Where($"rb.Message LIKE CONCAT({parameter}, '%')").Build();

        if (matchType.Equals("endsWith", StringComparison.OrdinalIgnoreCase))
            return builder.Where($"rb.Message LIKE CONCAT('%', {parameter})").Build();

        if (matchType.Equals("exact", StringComparison.OrdinalIgnoreCase))
            return builder.Where($"rb.Message = {parameter}").Build();

        return null;
    }
    
    #endregion
    
    public async Task<Result<RedblockWithLatestStatusEntity>> SelectRedblockById(long redblockId)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SelectRedblockById, redblockId, Transaction);
            return result is null
                ? Result<RedblockWithLatestStatusEntity>.Failure("Redblock not found.", HttpStatusCode.NotFound)
                : Result<RedblockWithLatestStatusEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockWithLatestStatusEntity>.Failure($"Failed to select redblock by ID: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
    

    public async Task<Result<IEnumerable<RedblockStatusEntity>>> SelectRedblockStatuses(long redblockId)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.SelectRedblockStatuses, redblockId, Transaction);
            return Result<IEnumerable<RedblockStatusEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<RedblockStatusEntity>>.Failure($"Failed to select redblock statuses: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RedblockUserAssignmentEntity>>> SelectRedblockUserAssignments(long redblockId)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.SelectRedblockUserAssignments, redblockId, Transaction);
            return Result<IEnumerable<RedblockUserAssignmentEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<RedblockUserAssignmentEntity>>.Failure($"Failed to select redblock user assignments: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<RedblockRoleAssignmentEntity>>> SelectRedblockRoleAssignments(long redblockId)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.SelectRedblockRoleAssignments, redblockId, Transaction);
            return Result<IEnumerable<RedblockRoleAssignmentEntity>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<RedblockRoleAssignmentEntity>>.Failure($"Failed to select redblock role assignments: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<Guid>>> SelectRedblockEntities(long redblockId)
    {
        try
        {
            var result = await Connection.QueryProcedure(StoredProcs.Redblocks.SelectRedblockEntities, redblockId, Transaction);
            return Result<IEnumerable<Guid>>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<IEnumerable<Guid>>.Failure($"Failed to select redblock entities: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockWithLatestStatusEntity>> UpdateRedblockMessage(long projectId, long keyNumber, string message, long updatedBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.UpdateRedblockMessage, (projectId, keyNumber, message, updatedBy), Transaction);
            return result is null
                ? Result<RedblockWithLatestStatusEntity>.Failure("Redblock not found.", HttpStatusCode.NotFound)
                : Result<RedblockWithLatestStatusEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockWithLatestStatusEntity>.Failure($"Failed to update redblock message: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockWithLatestStatusEntity>> SoftDeleteRedblock(long projectId, long keyNumber, long deletedBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.SoftDeleteRedblock, (projectId, keyNumber, deletedBy), Transaction);
            return result is null
                ? Result<RedblockWithLatestStatusEntity>.Failure("Redblock not found.", HttpStatusCode.NotFound)
                : Result<RedblockWithLatestStatusEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockWithLatestStatusEntity>.Failure($"Failed to soft delete redblock: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockStatusEntity>> InsertStatus(long projectId, long keyNumber, string status, long createdBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertStatus, (projectId, keyNumber, status, createdBy), Transaction);
            return result is null
                ? Result<RedblockStatusEntity>.Failure("Failed to insert redblock status: No status returned from database.", HttpStatusCode.InternalServerError)
                : Result<RedblockStatusEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockStatusEntity>.Failure($"Failed to insert redblock status: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockUserAssignmentEntity>> InsertUserAssignment(long projectId, long keyNumber, long assignedTo, long createdBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertUserAssignment, (projectId, keyNumber, assignedTo, createdBy), Transaction);
            return result is null
                ? Result<RedblockUserAssignmentEntity>.Failure("Failed to insert redblock user assignment: No assignment returned from database.", HttpStatusCode.InternalServerError)
                : Result<RedblockUserAssignmentEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockUserAssignmentEntity>.Failure($"Failed to insert redblock user assignment: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result> DeleteUserAssignment(long projectId, long keyNumber, long assignedTo)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Redblocks.DeleteUserAssignment, (projectId, keyNumber, assignedTo), Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No user assignment was deleted.", HttpStatusCode.NotFound);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete redblock user assignment: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<RedblockRoleAssignmentEntity>> InsertRoleAssignment(long projectId, long keyNumber, string roleName, long createdBy)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertRoleAssignment, (projectId, keyNumber, roleName, createdBy), Transaction);
            return result is null
                ? Result<RedblockRoleAssignmentEntity>.Failure("Failed to insert redblock role assignment: No assignment returned from database.", HttpStatusCode.InternalServerError)
                : Result<RedblockRoleAssignmentEntity>.Success(result);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result<RedblockRoleAssignmentEntity>.Failure($"Failed to insert redblock role assignment: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result> DeleteRoleAssignment(long projectId, long keyNumber, string roleName)
    {
        try
        {
            var rows = await Connection.ExecuteProcedure(StoredProcs.Redblocks.DeleteRoleAssignment, (projectId, keyNumber, roleName), Transaction);
            return rows > 0
                ? Result.Success()
                : Result.Failure("No role assignment was deleted.", HttpStatusCode.NotFound);
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete redblock role assignment: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result> InsertRedblockEntity(long projectId, long keyNumber, Guid entityGuid)
    {
        try
        {
            var result = await Connection.QuerySingleProcedure(StoredProcs.Redblocks.InsertRedblockEntity, (projectId, keyNumber, entityGuid), Transaction);
            return result is null
                ? Result.Failure("Failed to insert redblock entity mapping: No mapping returned from database.", HttpStatusCode.InternalServerError)
                : Result.Success();
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to insert redblock entity mapping: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result> DeleteRedblockEntities(long projectId, long keyNumber)
    {
        try
        {
            await Connection.ExecuteProcedure(StoredProcs.Redblocks.DeleteRedblockEntities, (projectId, keyNumber), Transaction);
            return Result.Success();
        }
        catch (DbException ex)
        {
            logger.LogDebug("{ErrorMessage}", ex.Message);
            return Result.Failure($"Failed to delete redblock entities: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
