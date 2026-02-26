using System.Text.Json.Serialization;

namespace GreenfieldCoreServices.Models.Patreon;

public record PatreonUserIdentityModel
{
    /// <summary>
    /// Contains the data about the user
    /// </summary>
    [JsonPropertyName("data")]
    public required PatreonUserDataModel Data { get; set; }
    
    /// <summary>
    /// The included data in the response, such as memberships and campaigns
    /// </summary>
    [JsonPropertyName("included")]
    public required List<PatreonMembershipModel> Included { get; set; }

    /// <summary>
    /// Gets the pledged amount in cents for the given campaign ID
    /// </summary>
    /// <param name="campaignId"></param>
    /// <returns></returns>
    public decimal GetPledgedAmountOfCampaign(string campaignId)
    {
        var pledgedMembership = Included.FirstOrDefault(i => i.Type == "member" && i.Relationships!.Campaign.Data.Id.Equals(campaignId, StringComparison.OrdinalIgnoreCase));
        return pledgedMembership?.Attributes?.AmountCents ?? 0;
    }
}
    
public record PatreonUserDataModel
{
    /// <summary>
    /// The unique ID of the user
    /// </summary>
    [JsonPropertyName("id")]
    public required string? Id { get; set; }
    
    /// <summary>
    /// The attributes of the user
    /// </summary>
    [JsonPropertyName("attributes")]
    public required PatreonUserAttributesModel? Attributes { get; set; }
}

public record PatreonMembershipModel
{
    /// <summary>
    /// The id of this included membership
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    /// <summary>
    /// The type of this included membership
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; set; }
    
    /// <summary>
    /// The attributes of this included membership
    /// </summary>
    [JsonPropertyName("attributes")]
    public PatreonMembershipAttributesModel? Attributes { get; set; }
    
    /// <summary>
    /// The relationships of this included membership
    /// </summary>
    [JsonPropertyName("relationships")]
    public PatreonMembershipRelationshipsModel? Relationships { get; set; }
    
}

public record PatreonMembershipAttributesModel
{
    /// <summary>
    /// The amount in cents that the user is currently entitled to for this membership
    /// </summary>
    [JsonPropertyName("currently_entitled_amount_cents")]
    public decimal AmountCents { get; set; }
}

public record PatreonMembershipRelationshipsModel
{
    /// <summary>
    /// The campaign associated with this membership
    /// </summary>
    [JsonPropertyName("campaign")]
    public required PatreonMemberCampaignModel Campaign { get; set; }
}

public record PatreonMemberCampaignModel
{
    /// <summary>
    /// The data about the campaign
    /// </summary>
    [JsonPropertyName("data")]
    public required PatreonCampaignModel Data { get; set; }
}

public record PatreonUserAttributesModel
{
    /// <summary>
    /// The full name of the user
    /// </summary>
    [JsonPropertyName("full_name")]
    public required string? FullName { get; set; }
}