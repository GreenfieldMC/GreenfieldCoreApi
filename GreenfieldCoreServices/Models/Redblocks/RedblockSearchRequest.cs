using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Redblocks;

public class RedblockSearchRequest
{
    
    /// <summary>
    /// The location to search around. If provided, the results will be ordered by distance from this location. If not provided, results will be ordered by index.
    /// </summary>
    public Location? Location { get; set; }
    
    /// <summary>
    /// The radius around the Location to search within. Location must be provided to search within a given radius.
    /// </summary>
    public long? Radius { get; set; }
    
    /// <summary>
    /// Only return redblocks that match the StatusFilter. If null, status is not used for filtering.
    /// </summary>
    public StatusFilter? StatusFilter { get; set; }
    
    /// <summary>
    /// Only return redblocks that match the DeletionFilter. If null, deletion status is not used for filtering.
    /// </summary>
    public DeletionFilter? DeletionFilter { get; set; }
    
    /// <summary>
    /// Only return redblocks that match the UserAssignmentFilter. If null, user assignments are not used for filtering.
    /// </summary>
    public UserAssignmentFilter? UserAssignmentFilter { get; set; }
    
    /// <summary>
    /// Only return redblocks that match the RoleAssignmentFilter. If null, role assignments are not used for filtering.
    /// </summary>
    public RoleAssignmentFilter? RoleAssignmentFilter { get; set; }
    
    /// <summary>
    /// Only return redblocks that match the MessageFilter. If null, message content is not used for filtering.
    /// </summary>
    public MessageFilter? MessageFilter { get; set; }

    /// <summary>
    /// Number of results to return per page.
    /// </summary>
    public int ResultsPerPage { get; set; } = 50;

    /// <summary>
    /// The current page of results to return.
    /// </summary>
    public long CurrentPage { get; set; } = 1;
}

/// <summary>
/// Filter a redblock by its status
/// </summary>
public class StatusFilter
{
    /// <summary>
    /// Statuses to filter by with this status filter.
    /// </summary>
    public required List<string> Statuses { get; set; } = [];
    
    /// <summary>
    /// How to match a redblock with a status. Supported options are "or" or "not"
    /// <br/>- "or" + [] = no filtering
    /// <br/>- "or" + [...] = the current status is one of the statuses
    /// <br/>- "not" + [] = the redblock has a status
    /// <br/>- "not" + [...] = the redblock has a status that is none of the statuses
    /// </summary>
    public required string MatchType { get; set; }
}

/// <summary>
/// Filter a redblock by its deletion status
/// </summary>
public class DeletionFilter
{
    /// <summary>
    /// Users to filter by with this deletion filter.
    /// </summary>
    public required List<long> Users { get; set; } = [];
    
    /// <summary>
    /// How to match a redblock with a deletion user. Supported options are "or", "not", "and"
    /// <br/>- "or" + [] = no filtering
    /// <br/>- "or" + [...] = deleted by one of the users
    /// <br/>- "not" + [] = deleted by anyone
    /// <br/>- "not" + [...] = deleted by anyone except the users
    /// <br/>- "and" + [] = not deleted
    /// <br/>- "and" + [...] = rejected, a redblock can only be deleted by one user
    /// </summary>
    public required string MatchType  { get; set; }
}

/// <summary>
/// Filter a redblock by its user assignments
/// </summary>
public class UserAssignmentFilter
{
    /// <summary>
    /// Users to filter by with this user assignment filter.
    /// </summary>
    public required List<long> Users { get; set; } = [];
    
    /// <summary>
    /// How to match a redblock with a user assignment user. Supported options are "or", "not", "and", "exact"
    /// <br/>- "or" + [] = no filtering
    /// <br/>- "or" + [...] = assigned to at least one of the users
    /// <br/>- "not" + [] = has at least one user assigned
    /// <br/>- "not" + [...] = assigned to none of the users, including redblocks with no users assigned
    /// <br/>- "and" + [] = has no users assigned
    /// <br/>- "and" + [...] = assigned to every one of the users, other users may also be assigned
    /// <br/>- "exact" + [] = has no users assigned
    /// <br/>- "exact" + [...] = assigned to every one of the users and no other users
    /// </summary>
    public required string MatchType  { get; set; }
}

/// <summary>
/// Filter a redblock by its role assignments
/// </summary>
public class RoleAssignmentFilter
{
    /// <summary>
    /// Roles to filter by with this role assignment filter.
    /// </summary>
    public required List<string> Roles { get; set; } = [];
    
    /// <summary>
    /// How to match a redblock with a role assignment role. Supported options are "or", "not", "and", "exact"
    /// <br/>- "or" + [] = no filtering
    /// <br/>- "or" + [...] = assigned at least one of the roles
    /// <br/>- "not" + [] = has at least one role assigned
    /// <br/>- "not" + [...] = assigned none of the roles, including redblocks with no roles assigned
    /// <br/>- "and" + [] = has no roles assigned
    /// <br/>- "and" + [...] = assigned every one of the roles, other roles may also be assigned
    /// <br/>- "exact" + [] = has no roles assigned
    /// <br/>- "exact" + [...] = assigned every one of the roles and no other roles
    /// </summary>
    public required string MatchType  { get; set; }
}

public class MessageFilter
{
    /// <summary>
    /// Message content to filter by with this message filter.
    /// </summary>
    public required string Content { get; set; }
    
    /// <summary>
    /// How to match a redblock with the message content. Supported options are:
    /// <br/>- "contains"
    /// <br/>- "exact"
    /// <br/>- "startsWith"
    /// <br/>- "endsWith"
    /// <br/>- "fuzzy"
    /// </summary>
    public required string MatchType { get; set; }
}