using System.Text.Json.Serialization;

namespace GreenfieldCoreServices.Models.Patreon;

public record PatreonCampaignListModel
{
    [JsonPropertyName("data")]
    public required List<PatreonCampaignModel> Data { get; init; }
}

public record PatreonCampaignModel
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    
    [JsonPropertyName("attributes")]
    public PatreonCampaignAttributesModel? Attributes { get; init; }
}

public record PatreonCampaignAttributesModel
{
    [JsonPropertyName("url")]
    public required string? Url { get; set; }
}