namespace GreenfieldCoreServices.Models.Patreon;

public record PatreonConnectionState(Guid StateId, DateTime Timestamp, long UserId, string RedirectUrl);
public record PatreonDisconnectState(Guid StateId, DateTime Timestamp, long UserId, long PatreonConnectionId, string RedirectUrl);