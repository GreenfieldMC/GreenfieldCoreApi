using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Resources;

namespace GreenfieldCoreServices.Services.External.Interfaces;

public interface IGitHubApi
{
    /// <summary>
    /// Gets the list of branches for the configured repository from the GitHub API.
    /// </summary>
    /// <returns>A list of branch names and their latest commit SHAs.</returns>
    Task<Result<List<GitHubBranch>>> GetBranches();

    /// <summary>
    /// Gets the latest commit hash for the given branch from the GitHub API.
    /// </summary>
    /// <param name="branchName">The branch name to look up.</param>
    /// <returns>The full SHA-1 commit hash string.</returns>
    Task<Result<string>> GetLatestCommitHash(string branchName);

    /// <summary>
    /// Downloads the zip archive of the repository at the given branch from GitHub.
    /// </summary>
    /// <param name="branchName">The branch name to download.</param>
    /// <returns>The raw zip file bytes.</returns>
    Task<Result<byte[]>> DownloadBranchZip(string branchName);
}

