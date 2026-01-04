namespace GreenfieldCoreDataAccess.Database.Models;

public record UserEntity
{
    public required long UserId { get; init; }
    public required Guid MinecraftUuid { get; init; }
    public required string MinecraftUsername { get; init; }
    public required DateTime CreatedOn { get; init; }
}