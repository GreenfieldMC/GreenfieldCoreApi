namespace GreenfieldCoreDataAccess.Database.Models;

public record BaseConnectionEntity
{
    public required string RefreshToken { get; init; }
    public required string AccessToken { get; init; }
    public required string TokenType { get; init; }
    public required DateTime TokenExpiry { get; init; }
    public required string Scope { get; init; }
    public required DateTime UpdatedOn { get; init; }
    public required DateTime CreatedOn { get; init; }
}

#region Discord Connection Entities

public record DiscordConnectionEntity : BaseConnectionEntity
{
    public required long DiscordConnectionId { get; init; }
    public required ulong DiscordSnowflake { get; init; }
    public required string DiscordUsername { get; init; }
}

public record UserDiscordConnectionEntity : DiscordConnectionEntity
{
    public required long UserDiscordConnectionId { get; init; }
    public required DateTime UserDiscordConnectionCreatedOn { get; init; }
    public required long UserId { get; init; }
}

public record UserByDiscordConnectionEntity : UserEntity
{
    public required long UserDiscordConnectionId { get; init; }
    public required DateTime UserDiscordConnectionCreatedOn { get; init; }
}

#endregion

#region Patreon Connection Entities

public record PatreonConnectionEntity : BaseConnectionEntity 
{
    public required long PatreonConnectionId { get; init; }
    public required long PatreonId { get; init; }
    public required string FullName { get; init; }
    public required decimal? Pledge { get; init; }
}

public record UserPatreonConnectionEntity : PatreonConnectionEntity
{
    public required long UserPatreonConnectionId { get; init; }
    public required DateTime UserPatreonConnectionCreatedOn { get; init; }
    public required long UserId { get; init; }
}

public record UserByPatreonConnectionEntity : UserEntity
{
    public required long UserPatreonConnectionId { get; init; }
    public required DateTime UserPatreonConnectionCreatedOn { get; init; }
}

#endregion
