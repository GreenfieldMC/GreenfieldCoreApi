namespace GreenfieldCoreServices.Models.BuildApps;

public class BuilderApplication
{
    public required long ApplicationId;
    public required List<BuildAppStatus> BuildAppStatuses;
    public required long UserId;
    public required int Age;
    public string? Nationality;
    public required List<BuildAppImage> HouseBuilds;
    public required List<BuildAppImage> OtherBuilds;
    public string? AdditionalBuildingInformation;
    public required string WhyJoinGreenfield;
    public string? AdditionalComments;
    public required DateTime CreatedOn;
}