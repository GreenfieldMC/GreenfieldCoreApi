using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Connections.Discord;

public record DiscordConnection : IDbModelConvertable<DiscordConnectionEntity, DiscordConnection>
{
    
    public required long DiscordConnectionId { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime RefreshBy { get; init; }
    public required DateTime? UpdatedOn { get; init; }
    public required DateTime CreatedOn { get; init; }
    
    public required ulong DiscordSnowflake { get; init; }
    public required string DiscordUsername { get; init; }
    
    public static DiscordConnection FromDbModel(DiscordConnectionEntity from)
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

public record UserDiscordConnection : IDbModelConvertable<UserDiscordConnectionEntity, UserDiscordConnection>
{
    public required long UserDiscordConnectionId { get; init; }
    public required long UserId { get; init; }
    public required long DiscordConnectionId { get; init; }
    public required DateTime ConnectedOn { get; init; }

    public static UserDiscordConnection FromDbModel(UserDiscordConnectionEntity from)
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