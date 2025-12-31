namespace GreenfieldCoreDataAccess.Database.Models;

public record UserDiscordEntity(
    long UserDiscordId,
    long UserId,
    ulong DiscordSnowflake,
    string DiscordUsername,
    string RefreshToken,
    string AccessToken,
    string TokenType,
    DateTime TokenExpiry,
    string Scope,
    DateTime? UpdatedOn,
    DateTime CreatedOn);
