namespace GreenfieldCoreApi.ApiModels.Connections;

/// <summary>
/// Represents a Discord account connected to a user.
/// </summary>
/// <param name="UserDiscordConnectionId">The ID of the user-discord connection record.</param>
/// <param name="DiscordConnectionId">The ID of the Discord connection record.</param>
/// <param name="DiscordSnowflake">The Discord snowflake ID of the user.</param>
/// <param name="DiscordUsername">The username of the Discord account.</param>
/// <param name="ConnectedOn">The date the user and the Discord connection were linked.</param>
/// <param name="UpdatedOn">The date the connection was last updated.</param>
/// <param name="CreatedOn">The date the connection was created.</param>
public record ApiDiscordAccount(long UserDiscordConnectionId, long DiscordConnectionId, ulong DiscordSnowflake, string DiscordUsername, DateTime ConnectedOn, DateTime? UpdatedOn, DateTime CreatedOn);