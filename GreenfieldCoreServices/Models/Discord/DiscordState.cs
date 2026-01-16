namespace GreenfieldCoreServices.Models.Discord;

public record DiscordConnectionState(Guid StateId, DateTime Timestamp, long UserId, string RedirectUrl);
public record DiscordDisconnectState(Guid StateId, DateTime Timestamp, long UserId, long DiscordConnectionId, string RedirectUrl);