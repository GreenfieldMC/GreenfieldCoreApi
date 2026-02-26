using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.BuildApps;

public record BuildAppStatus(string Status, string? StatusMessage, DateTime CreatedOn) : IModelConvertable<ApplicationStatusEntity, BuildAppStatus>
{
    public static BuildAppStatus FromModel(ApplicationStatusEntity from)
    {
        return new BuildAppStatus(from.Status, from.StatusMessage, from.CreatedOn);
    }
}