namespace GreenfieldCoreDataAccess.Database.Models;

public record UserDiscordEntity(long UserDiscordId, long UserId, ulong DiscordSnowflake, DateTime CreatedOn);