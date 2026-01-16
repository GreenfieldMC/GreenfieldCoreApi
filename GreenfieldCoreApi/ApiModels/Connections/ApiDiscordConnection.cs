using GreenfieldCoreServices.Models;
using GreenfieldCoreServices.Models.Connections.Discord;
using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreApi.ApiModels.Connections;

public record ApiDiscordConnection : IModelConvertable<DiscordConnection, ApiDiscordConnection>
{
    public required long DiscordConnectionId { get; init; }
    public required ulong DiscordSnowflake { get; init; }
    public required string DiscordUsername { get; init; }
    public required DateTime? UpdatedOn { get; init; }
    public required DateTime CreatedOn { get; init; }

    public static ApiDiscordConnection FromModel(DiscordConnection from)
    {
        return new ApiDiscordConnection
        {
            DiscordConnectionId = from.DiscordConnectionId,
            DiscordSnowflake = from.DiscordSnowflake,
            DiscordUsername = from.DiscordUsername,
            UpdatedOn = from.UpdatedOn,
            CreatedOn = from.CreatedOn
        };
    }
}

public record ApiDiscordConnectionWithUsers : ApiDiscordConnection
{
    public required IEnumerable<User> Users { get; init; }
}
    