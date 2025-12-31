namespace GreenfieldCoreServices.Models.Patreon;

public record PatreonConnectionState(Guid StateId, DateTime Timestamp, long UserId, string RedirectUrl);