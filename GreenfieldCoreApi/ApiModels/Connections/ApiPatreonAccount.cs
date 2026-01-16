using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreApi.ApiModels.Connections;

/// <summary>
/// Represents a Patreon account connection.
/// </summary>
public record ApiPatreonAccount : ApiPatreonConnection
{
    public required long UserPatreonConnectionId { get; init; }
    public required User User { get; init; }
    public required DateTime ConnectedOn { get; init; }
}