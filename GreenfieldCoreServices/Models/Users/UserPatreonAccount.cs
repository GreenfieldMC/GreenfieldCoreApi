using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Users;

public record UserPatreonAccount(long UserPatreonId, long UserId, long PatreonId, decimal? Pledge, DateTime? UpdatedOn, DateTime CreatedOn) : IDbModelConvertable<UserPatreonEntity, UserPatreonAccount>
{
    public static UserPatreonAccount FromDbModel(UserPatreonEntity from)
    {
        return new UserPatreonAccount(
            from.UserPatreonId,
            from.UserId,
            from.PatreonId,
            from.Pledge,
            from.UpdatedOn,
            from.CreatedOn
        );
    }
}