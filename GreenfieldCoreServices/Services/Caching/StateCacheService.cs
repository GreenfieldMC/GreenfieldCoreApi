using GreenfieldCoreServices.Models.Discord;
using GreenfieldCoreServices.Models.Patreon;

namespace GreenfieldCoreServices.Services.Caching;

public class PatreonConnectionStateCache : BaseCacheService<long, PatreonConnectionState>;
public class DiscordConnectionStateCache : BaseCacheService<long, DiscordConnectionState>;