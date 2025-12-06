using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreServices.Services.Caching;

public class UserCacheService : BaseCacheService<long, User>;
public class UserDiscordCacheService : BaseCacheService<long, List<ulong>>;