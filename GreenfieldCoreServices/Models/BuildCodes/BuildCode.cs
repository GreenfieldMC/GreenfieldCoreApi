using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.BuildCodes;

public class BuildCode : IDbModelConvertable<BuildCodeEntity, BuildCode>
{
    public required long BuildCodeId { get; set; }
    public required int ListOrder { get; set; }
    public required string Code { get; set; }
    public required DateTime CreatedOn { get; set; }
    public required bool Deleted { get; set; }
    
    public static BuildCode FromDbModel(BuildCodeEntity from)
    {
        return new BuildCode
        {
            BuildCodeId = from.BuildCodeId,
            ListOrder = from.ListOrder,
            Code = from.BuildCode,
            CreatedOn = from.CreatedOn,
            Deleted = from.Deleted
        };
    }
}