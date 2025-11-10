using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.BuildCodes;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IBuildCodeService
{
    /// <summary>
    /// Gets all build codes.
    /// </summary>
    /// <param name="showDeleted"></param>
    /// <returns>A list of all build codes.</returns>
    Task<Result<IEnumerable<BuildCode>>> GetAllBuildCodes(bool showDeleted = false);

    /// <summary>
    /// Gets a build code by its unique identifier.
    /// </summary>
    /// <param name="buildCodeId">The unique identifier of the build code.</param>
    /// <returns>The build code if found; otherwise, a failed result.</returns>
    Task<Result<BuildCode>> GetBuildCodeById(long buildCodeId);

    /// <summary>
    /// Deletes a build code by its unique identifier.
    /// </summary>
    /// <param name="buildCodeId">The unique identifier of the build code to delete.</param>
    /// <returns>The deleted build code if successful; otherwise, a failed result.</returns>
    Task<Result<BuildCode>> DeleteBuildCode(long buildCodeId);

    /// <summary>
    /// Creates a new build code entry.
    /// </summary>
    /// <param name="listOrder">The order in which the build code appears in the list (lowest shows first).</param>
    /// <param name="buildCode">The build code string.</param>
    /// <returns>The created build code if successful; otherwise, a failed result.</returns>
    Task<Result<BuildCode>> CreateBuildCode(int listOrder, string buildCode);

    /// <summary>
    /// Updates an existing build code's details.
    /// </summary>
    /// <param name="buildCodeId">The unique identifier of the build code to update.</param>
    /// <param name="listOrder">The new order for the build code (optional).</param>
    /// <param name="buildCode">The new build code string (optional).</param>
    /// <returns>The updated build code if successful; otherwise, a failed result.</returns>
    Task<Result<BuildCode>> UpdateBuildCode(long buildCodeId, int? listOrder = null, string? buildCode = null);
}