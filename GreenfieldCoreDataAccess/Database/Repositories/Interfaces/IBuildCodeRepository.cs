using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IBuildCodeRepository
{
    /// <summary>
    /// Create a new build code entry
    /// </summary>
    /// <param name="listOrder">Lowest = show first in the list of codes</param>
    /// <param name="buildCode">The build code</param>
    /// <returns>The created build code, or null if no insert was performed</returns>
    Task<BuildCodeEntity?> CreateBuildCode(int listOrder, string buildCode);
    
    /// <summary>
    /// Get a build code by its ID
    /// </summary>
    /// <param name="buildCodeId">The build code ID</param>
    /// <returns>The build code entity, or null if not found</returns>
    Task<BuildCodeEntity?> GetBuildCode(long buildCodeId);
    
    /// <summary>
    /// Get all build codes
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<BuildCodeEntity>> GetAllBuildCodes();
    
    /// <summary>
    /// Mark a build code as deleted
    /// </summary>
    /// <param name="buildCodeId"></param>
    /// <returns></returns>
    Task<bool> DeleteBuildCode(long buildCodeId);

    /// <summary>
    /// Update a build code's details
    /// </summary>
    /// <param name="buildCodeId">The build code ID to update</param>
    /// <param name="listOrder">The new list order</param>
    /// <param name="buildCode">The new build code</param>
    /// <returns></returns>
    Task<bool> UpdateBuildCode(long buildCodeId, int listOrder, string buildCode);
}