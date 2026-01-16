using GreenfieldCoreServices.Models.Discord;
using GreenfieldCoreServices.Models.Patreon;

namespace GreenfieldCoreServices.Services.Caching;

public class PatreonConnectionStateCache : BaseCacheService<long, PatreonConnectionState>;
public class PatreonDisconnectStateCache : BaseCacheService<(long userId, long patreonConnectionId), PatreonDisconnectState>;
public class DiscordConnectionStateCache : BaseCacheService<long, DiscordConnectionState>;
public class DiscordDisconnectStateCache : BaseCacheService<(long userId, long discordConnectionId), DiscordDisconnectState>;