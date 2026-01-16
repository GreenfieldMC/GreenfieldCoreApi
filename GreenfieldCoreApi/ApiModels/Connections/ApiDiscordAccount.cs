using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreApi.ApiModels.Connections;

/// <summary>
/// Represents a Discord account connected to a user.
/// </summary>
public record ApiDiscordAccount : ApiDiscordConnection
{
    public required long UserDiscordConnectionId { get; init; }
    public required User User { get; init; }
    public required DateTime ConnectedOn { get; init; }
}