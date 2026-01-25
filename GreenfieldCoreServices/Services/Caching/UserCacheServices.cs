using GreenfieldCoreServices.Models.Users;

namespace GreenfieldCoreServices.Services.Caching;

public class UserCacheService : BaseCacheService<long, User>;
public class UserPatreonCacheService : BaseCacheService<long, UserPatreonAccount>;
public class UserDiscordCacheService : BaseCacheService<long, UserDiscordAccount>;
