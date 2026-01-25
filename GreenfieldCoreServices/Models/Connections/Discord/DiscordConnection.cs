using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Connections.Discord;

public record DiscordConnection : IModelConvertable<DiscordConnectionEntity, DiscordConnection>
{
    
    public required long DiscordConnectionId { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime RefreshBy { get; init; }
    public required DateTime? UpdatedOn { get; init; }
    public required DateTime CreatedOn { get; init; }
    
    public required ulong DiscordSnowflake { get; init; }
    public required string DiscordUsername { get; init; }
    
    public static DiscordConnection FromModel(DiscordConnectionEntity from)
    {
        return new DiscordConnection
        {
            DiscordConnectionId = from.DiscordConnectionId,
            RefreshToken = from.RefreshToken,
            RefreshBy = from.TokenExpiry,
            UpdatedOn = from.UpdatedOn,
            CreatedOn = from.CreatedOn,
            DiscordSnowflake = from.DiscordSnowflake,
            DiscordUsername = from.DiscordUsername
        };
    }
}

public record UserDiscordConnection : IModelConvertable<UserDiscordConnectionEntity, UserDiscordConnection>
{
    public required long UserDiscordConnectionId { get; init; }
    public required long UserId { get; init; }
    public required long DiscordConnectionId { get; init; }
    public required DateTime ConnectedOn { get; init; }

    public static UserDiscordConnection FromModel(UserDiscordConnectionEntity from)
    {
        return new UserDiscordConnection
        {
            UserDiscordConnectionId = from.UserDiscordConnectionId,
            UserId = from.UserId,
            DiscordConnectionId = from.DiscordConnectionId,
            ConnectedOn = from.UserDiscordConnectionCreatedOn
        };
    }
}

public record UserByDiscordConnection : IModelConvertable<UserByDiscordConnectionEntity, UserByDiscordConnection>
{
    public required long UserDiscordConnectionId { get; init; }
    public required long UserId { get; init; }
    public required string MinecraftUsername { get; init; }
    public required Guid MinecraftUuid { get; init; }
    public required DateTime ConnectedOn { get; init; }

    public static UserByDiscordConnection FromModel(UserByDiscordConnectionEntity from)
    {
        return new UserByDiscordConnection
        {
            UserDiscordConnectionId = from.UserDiscordConnectionId,
            UserId = from.UserId,
            MinecraftUsername = from.MinecraftUsername,
            MinecraftUuid = from.MinecraftUuid,
            ConnectedOn = from.UserDiscordConnectionCreatedOn
        };
    }
}