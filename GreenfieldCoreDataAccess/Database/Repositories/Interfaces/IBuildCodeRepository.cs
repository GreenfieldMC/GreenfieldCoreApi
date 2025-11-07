using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IBuildCodeRepository
{
    /// <summary>
    /// Create a new build code entry
    /// </summary>
    /// <param name="listOrder">Lowest = show first in the list of codes</param>
    /// <param name="buildCode">The build code</param>
    /// <returns>DbResult containing the created build code, or null if no insert was performed</returns>
    Task<DbResult<BuildCodeEntity?>> CreateBuildCode(int listOrder, string buildCode);
    
    /// <summary>
    /// Get a build code by its ID
    /// </summary>
    /// <param name="buildCodeId">The build code ID</param>
    /// <returns>DbResult containing the build code entity, or null if not found</returns>
    Task<DbResult<BuildCodeEntity?>> GetBuildCode(long buildCodeId);
    
    /// <summary>
    /// Get all build codes
    /// </summary>
    /// <returns>DbResult containing all build codes</returns>
    Task<DbResult<IEnumerable<BuildCodeEntity>>> GetAllBuildCodes();
    
    /// <summary>
    /// Mark a build code as deleted
    /// </summary>
    /// <param name="buildCodeId"></param>
    /// <returns>DbResult true if deleted, false otherwise</returns>
    Task<DbResult<bool>> DeleteBuildCode(long buildCodeId);

    /// <summary>
    /// Update a build code's details
    /// </summary>
    /// <param name="buildCodeId">The build code ID to update</param>
    /// <param name="listOrder">The new list order</param>
    /// <param name="buildCode">The new build code</param>
    /// <returns>DbResult true if updated, false otherwise</returns>
    Task<DbResult<bool>> UpdateBuildCode(long buildCodeId, int listOrder, string buildCode);
}