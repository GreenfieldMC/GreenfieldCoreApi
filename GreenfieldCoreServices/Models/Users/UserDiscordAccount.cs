using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Users;

public record UserDiscordAccount(
    long UserDiscordId,
    long UserId,
    ulong DiscordSnowflake,
    string DiscordUsername,
    string RefreshToken,
    DateTime RefreshBy,
    DateTime? UpdatedOn,
    DateTime CreatedOn) : IModelConvertable<UserDiscordEntity, UserDiscordAccount>
{
    public static UserDiscordAccount FromModel(UserDiscordEntity from)
    {
        return new UserDiscordAccount(
            from.UserDiscordId,
            from.UserId,
            from.DiscordSnowflake,
            from.DiscordUsername,
            from.RefreshToken,
            from.TokenExpiry,
            from.UpdatedOn,
            from.CreatedOn
        );
    }
}
