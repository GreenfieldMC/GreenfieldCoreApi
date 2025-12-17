namespace GreenfieldCoreServices.Models.BuildApps;

public class ApplicationLatestStatus
{
    public required long ApplicationId { get; set; }
    public BuildAppStatus? LatestStatus { get; set; }
}
