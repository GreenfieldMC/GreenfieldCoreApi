using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface ICodeRepository
{
    /// <summary>
    /// Create a new build code entry
    /// </summary>
    /// <param name="listOrder">Lowest = show first in the list of codes</param>
    /// <param name="buildCode">The build code</param>
    /// <returns>Result containing the created build code, or null if no insert was performed</returns>
    Task<Result<BuildCodeEntity?>> InsertCode(int listOrder, string buildCode);
    
    /// <summary>
    /// Get a build code by its ID
    /// </summary>
    /// <param name="buildCodeId">The build code ID</param>
    /// <returns>Result containing the build code entity, or null if not found</returns>
    Task<Result<BuildCodeEntity?>> SelectCode(long buildCodeId);
    
    /// <summary>
    /// Get all build codes
    /// </summary>
    /// <returns>Result containing all build codes</returns>
    Task<Result<IEnumerable<BuildCodeEntity>>> SelectCodes();

    /// <summary>
    /// Mark a build code as deleted
    /// </summary>
    /// <param name="buildCodeId"></param>
    /// <returns>Result true if deleted, false otherwise</returns>
    Task<Result> DeleteCode(long buildCodeId);

    /// <summary>
    /// Update a build code's details
    /// </summary>
    /// <param name="buildCodeId">The build code ID to update</param>
    /// <param name="listOrder">The new list order</param>
    /// <param name="buildCode">The new build code</param>
    /// <returns>Result true if updated, false otherwise</returns>
    Task<Result> UpdateCode(long buildCodeId, int listOrder, string buildCode);
}