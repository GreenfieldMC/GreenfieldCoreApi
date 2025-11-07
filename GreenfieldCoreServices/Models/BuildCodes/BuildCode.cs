namespace GreenfieldCoreServices.Models.BuildCodes;

public class BuildCode
{
    public required long BuildCodeId { get; set; }
    public required int ListOrder { get; set; }
    public required string Code { get; set; }
    public required DateTime CreatedOn { get; set; }
    public required bool Deleted { get; set; }
}