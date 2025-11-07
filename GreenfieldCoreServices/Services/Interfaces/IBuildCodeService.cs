using GreenfieldCoreServices.Models.BuildCodes;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IBuildCodeService
{
    /// <summary>
    /// Gets all build codes.
    /// </summary>
    /// <returns>A list of build codes.</returns>
    Task<IEnumerable<BuildCode>> GetAllBuildCodes();
    
    Task<BuildCode?> GetBuildCodeById(long buildCodeId);
    
    Task<BuildCode?> DeleteBuildCode(long buildCodeId);
    
    Task<BuildCode?> CreateBuildCode(int listOrder, string buildCode);
    
    Task<BuildCode?> UpdateBuildCode(long buildCodeId, int? listOrder = null, string? buildCode = null);
    
    
}