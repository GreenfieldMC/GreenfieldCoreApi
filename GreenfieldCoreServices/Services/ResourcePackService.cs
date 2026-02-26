using System.IO.Compression;
using System.Net;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Resources;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services;

public class ResourcePackService(IGitHubApi gitHubApi, ILogger<ResourcePackService> logger, ICacheService<string, ResourcePackCacheEntry> cache) : IResourcePackService
{
    private const int MaxCachedBranches = 4;

    public async Task<Result<ResourcePackResult>> GetResourcePack(string branchName)
    {
        // Get the latest commit hash for the branch
        var commitHashResult = await gitHubApi.GetLatestCommitHash(branchName);
        if (!commitHashResult.TryGetDataNonNull(out var commitHash))
        {
            logger.LogError("Failed to get latest commit hash for branch {Branch}: {Error}", branchName, commitHashResult.ErrorMessage);
            return Result<ResourcePackResult>.Failure(commitHashResult.ErrorMessage ?? "Failed to get latest commit hash.", commitHashResult.StatusCode);
        }

        // Check cache - if we have a matching entry, return it
        if (cache.TryGetValue(branchName, out var cachedEntry) && cachedEntry.CommitHash == commitHash)
        {
            logger.LogInformation("Serving cached resource pack for branch {Branch} at commit {CommitHash}", branchName, commitHash[..7]);
            return Result<ResourcePackResult>.Success(new ResourcePackResult(cachedEntry.ZipBytes, cachedEntry.CommitHash));
        }

        // Download the zip from GitHub
        var zipResult = await gitHubApi.DownloadBranchZip(branchName);
        if (!zipResult.TryGetDataNonNull(out var zipBytes))
        {
            logger.LogError("Failed to download zip for branch {Branch}: {Error}", branchName, zipResult.ErrorMessage);
            return Result<ResourcePackResult>.Failure(zipResult.ErrorMessage ?? "Failed to download branch zip.", zipResult.StatusCode);
        }

        // Repackage the zip with contents at the top level
        byte[] repackagedZip;
        try
        {
            repackagedZip = RepackageZip(zipBytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to repackage zip for branch {Branch}", branchName);
            return Result<ResourcePackResult>.Failure($"Failed to repackage zip: {ex.Message}", HttpStatusCode.InternalServerError);
        }

        // Evict oldest entry if cache is at capacity
        if (cache.GetCount() >= MaxCachedBranches && !cache.TryGetValue(branchName, out _))
        {
            var oldest = cache.GetValues().OrderBy(e => e.CachedAt).First();
            cache.RemoveValues(e => e.Branch == oldest.Branch && e.CachedAt == oldest.CachedAt);
            logger.LogInformation("Evicted cached resource pack for branch {Branch} to make room", oldest.Branch);
        }

        // Cache the new entry
        var newEntry = new ResourcePackCacheEntry(branchName, commitHash, repackagedZip, DateTime.UtcNow);
        cache.SetValue(branchName, newEntry);
        logger.LogInformation("Cached resource pack for branch {Branch} at commit {CommitHash}", branchName, commitHash[..7]);

        return Result<ResourcePackResult>.Success(new ResourcePackResult(repackagedZip, commitHash));
    }

    /// <summary>
    /// Takes a GitHub archive zip (which wraps everything in a repo-branch/ subfolder)
    /// and rewrites it so all contents are at the top level of the zip.
    /// </summary>
    private static byte[] RepackageZip(byte[] originalZipBytes)
    {
        using var originalStream = new MemoryStream(originalZipBytes);
        using var originalArchive = new ZipArchive(originalStream, ZipArchiveMode.Read);

        using var newStream = new MemoryStream();
        using (var newArchive = new ZipArchive(newStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            // Find the common root folder prefix (e.g., "repo-branch/")
            var prefix = FindRootPrefix(originalArchive);

            foreach (var entry in originalArchive.Entries)
            {
                // Skip the root directory entry itself
                if (entry.FullName == prefix)
                    continue;

                // Strip the prefix from the entry path
                var newPath = entry.FullName;
                if (!string.IsNullOrEmpty(prefix) && newPath.StartsWith(prefix))
                    newPath = newPath[prefix.Length..];

                // Skip if stripping the prefix results in an empty name (root dir)
                if (string.IsNullOrEmpty(newPath))
                    continue;

                var newEntry = newArchive.CreateEntry(newPath, CompressionLevel.Optimal);
                newEntry.LastWriteTime = entry.LastWriteTime;

                // Directories have zero-length content
                if (entry.Length > 0)
                {
                    using var sourceStream = entry.Open();
                    using var destStream = newEntry.Open();
                    sourceStream.CopyTo(destStream);
                }
            }
        }

        return newStream.ToArray();
    }

    /// <summary>
    /// Finds the common root folder prefix that GitHub adds to archive zips.
    /// For example, "repo-main/" for a repo downloaded at the main branch.
    /// </summary>
    private static string FindRootPrefix(ZipArchive archive)
    {
        var firstEntry = archive.Entries.FirstOrDefault();
        if (firstEntry == null)
            return string.Empty;

        var firstSlash = firstEntry.FullName.IndexOf('/');
        return firstSlash >= 0 ? firstEntry.FullName[..(firstSlash + 1)] : string.Empty;
    }
}

