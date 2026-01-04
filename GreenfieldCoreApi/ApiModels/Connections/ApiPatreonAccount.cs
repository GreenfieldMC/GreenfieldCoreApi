namespace GreenfieldCoreApi.ApiModels.Connections;

/// <summary>
/// Represents a Patreon account connection.
/// </summary>
/// <param name="UserPatreonConnectionId">The ID of the user-patreon connection record.</param>
/// <param name="PatreonConnectionId">The patreon connection ID.</param>
/// <param name="FullName">The full name of the user.</param>
/// <param name="Pledge">The pledge amount.</param>
/// <param name="ConnectedOn">The date user and the patreon connection were linked.</param>
/// <param name="UpdatedOn">The date the connection was last updated.</param>
/// <param name="CreatedOn">The date the connection was created.</param>
public record ApiPatreonAccount(long UserPatreonConnectionId, long PatreonConnectionId, string FullName, decimal? Pledge, DateTime ConnectedOn, DateTime? UpdatedOn, DateTime CreatedOn);