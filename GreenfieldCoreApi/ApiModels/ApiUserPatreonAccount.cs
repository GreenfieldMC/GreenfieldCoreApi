using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreApi.ApiModels;

public record ApiUserPatreonAccount(long UserPatreonId, User User, long PatreonId, decimal? Pledge, DateTime? UpdatedOn, DateTime CreatedOn);