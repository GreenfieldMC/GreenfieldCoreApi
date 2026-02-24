using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Resources;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IResourcePackService
{
    /// <summary>
    /// Gets a resource pack zip for the given branch, using cache when possible.
    /// The returned zip has repository contents at the top level (GitHub's subfolder wrapper is stripped).
    /// </summary>
    /// <param name="branchName">The GitHub branch name to fetch.</param>
    /// <returns>A result containing the repackaged zip bytes and commit hash.</returns>
    Task<Result<ResourcePackResult>> GetResourcePack(string branchName);
}

