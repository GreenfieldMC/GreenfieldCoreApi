using GreenfieldCoreServices.Models.Connections.Discord;
using GreenfieldCoreServices.Models.Connections.Patreon;

namespace GreenfieldCoreServices.Services.Caching;

public class DiscordConnectionCacheService : BaseCacheService<long, DiscordConnection>;
public class UserDiscordConnectionCacheService : BaseCacheService<(long userId, long discordConnectionId), UserDiscordConnection>;

public class PatreonConnectionCacheService : BaseCacheService<long, PatreonConnection>;
public class UserPatreonConnectionCacheService : BaseCacheService<(long userId, long patreonConnectionId), UserPatreonConnection>;