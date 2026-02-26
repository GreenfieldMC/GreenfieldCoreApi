using System.Text.Json.Serialization;

namespace GreenfieldCoreServices.Models.Discord;

public class DiscordIdentityResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("global_name")]
    public string? GlobalName { get; set; }
}

