namespace GreenfieldCoreApi.ApiModels;

public record ApiAddApplicationStatusModel(string Status, string? StatusMessage = null);
public class ApiApplicationSubmissionModel
{

    public required long UserId { get; set; }
    public required int Age { get; set; }
    public string? Nationality { get; set; } = null;

    public required List<ApiApplicationImageModel> Images { get; set; } = [];
    public string? AdditionalBuildingInformation { get; set; } = null;
    
    public required string WhyJoinGreenfield { get; set; }
    public string? AdditionalComments { get; set; } = null;

}

public class ApiApplicationImageModel
{
    public required string ImageLink { get; set; }
    public required string ImageType { get; set; }
}