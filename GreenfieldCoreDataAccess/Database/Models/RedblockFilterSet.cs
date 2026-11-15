namespace GreenfieldCoreDataAccess.Database.Models;

/// <summary>
/// One group of redblock search filters. Every filter that is set must match (AND).
/// A null match type means that filter is not used.
/// </summary>
public record RedblockFilterSet
{
    public List<string> Statuses { get; init; } = [];
    public string? StatusMatchType { get; init; }

    public List<long> DeletionUserIds { get; init; } = [];
    public string? DeletionMatchType { get; init; }

    public List<long> AssignedUserIds { get; init; } = [];
    public string? UserAssignmentMatchType { get; init; }

    public List<string> AssignedRoles { get; init; } = [];
    public string? RoleAssignmentMatchType { get; init; }

    public string MessageText { get; init; } = "";
    public string? MessageMatchType { get; init; }
}
