namespace GreenfieldCoreApi.ApiModels;

public class BuilderApplicationSubmitModel
{

    public required ulong DiscordId { get; set; }
    public required string MinecraftUsername { get; set; }
    public required Guid MinecraftUuid { get; set; }

    public required int Age { get; set; }
    public string? Nationality { get; set; } = null;

    public required List<string> HouseBuildLinks { get; set; } = [];
    public required List<string> OtherBuildLinks { get; set; } = [];
    public string? AdditionalBuildingInformation { get; set; } = null;
    
    public required string WhyJoinGreenfield { get; set; }
    public string? AdditionalComments { get; set; } = null;

}