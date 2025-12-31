using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreApi.ApiModels;

public record ApiUserDiscordAccount(long UserDiscordId, User User, ulong DiscordSnowflake, string DiscordUsername, DateTime? UpdatedOn, DateTime CreatedOn);