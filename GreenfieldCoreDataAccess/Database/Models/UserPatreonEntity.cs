namespace GreenfieldCoreDataAccess.Database.Models;

public record UserPatreonEntity(long UserPatreonId, long UserId, string RefreshToken, string AccessToken, string TokenType, DateTime TokenExpiry, string Scope, long PatreonId, decimal? Pledge, DateTime UpdatedOn, DateTime CreatedOn);