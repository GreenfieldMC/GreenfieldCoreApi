using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.BuildCodes;

public class BuildCode : IDbModelConvertable<BuildCodeEntity, BuildCode>
{
    public required long CodeId { get; set; }
    public required int ListOrder { get; set; }
    public required string Code { get; set; }
    public required DateTime CreatedOn { get; set; }
    
    public static BuildCode FromDbModel(BuildCodeEntity from)
    {
        return new BuildCode
        {
            CodeId = from.CodeId,
            ListOrder = from.ListOrder,
            Code = from.BuildCode,
            CreatedOn = from.CreatedOn,
        };
    }
}