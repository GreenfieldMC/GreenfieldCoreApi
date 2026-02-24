using System.Text.RegularExpressions;
using Asp.Versioning;
using GreenfieldCoreApi.Extensions;
using GreenfieldCoreServices.Models.Resources;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public partial class ResourceController(
    IConfiguration configuration,
    IResourcePackService resourcePackService,
    IGitHubApi gitHubApi,
    ICacheService<Guid, DownloadToken> downloadTokenCache) : ControllerBase
{
    private static readonly TimeSpan TokenExpiry = TimeSpan.FromMinutes(10);

    [GeneratedRegex(@"^[a-zA-Z0-9._\-/]+$")]
    private static partial Regex BranchNameRegex();

    private static string LegalFileName(string fileName) => Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c, '_'));
    
    /// <summary>
    /// Lists the available branches for the resource pack repository.
    /// </summary>
    /// <returns>A list of branch names and their latest commit SHAs.</returns>
    [Authorize(Roles = "Resources,Resources.Pack,Resources.Pack.Branches")]
    [HttpGet("resourcepack/branches")]
    [ProducesResponseType(typeof(List<GitHubBranch>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBranches()
    {
        var result = await gitHubApi.GetBranches();
        if (!result.TryGetDataNonNull(out var branches))
            return Problem(statusCode: result.GetStatusCodeInt(), detail: result.ErrorMessage);

        return Ok(branches);
    }

    /// <summary>
    /// Generates a one-time download link for a resource pack from the specified branch.
    /// The link expires after 10 minutes.
    /// </summary>
    /// <param name="branch">The GitHub branch name to generate a download link for.</param>
    /// <returns>An object containing the one-time download URL.</returns>
    [Authorize(Roles = "Resources,Resources.Pack,Resources.Pack.Download")]
    [HttpPost("resourcepack/download-request")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult RequestDownload([FromQuery] string branch)
    {
        if (string.IsNullOrWhiteSpace(branch) || !BranchNameRegex().IsMatch(branch))
            return Problem(statusCode: StatusCodes.Status400BadRequest,
                detail: "Invalid branch name. Branch names may only contain alphanumeric characters, dots, hyphens, underscores, and forward slashes.");

        var token = new DownloadToken(Guid.NewGuid(), branch, DateTime.UtcNow);
        downloadTokenCache.SetValue(token.TokenId, token);

        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var downloadUrl = Url.Action(nameof(GetResourcePack), "Resource",
            new { version, branchName = branch, token = token.TokenId },
            Request.Scheme, Request.Host.ToString());

        return Ok(new { downloadUrl, token = token.TokenId, expiresInMinutes = TokenExpiry.TotalMinutes });
    }

    /// <summary>
    /// Downloads a Minecraft resource pack from a specific branch.
    /// Requires a valid one-time download token obtained from the download-request endpoint.
    /// The resource pack is fetched from GitHub, repackaged so contents are at the zip root, and cached.
    /// </summary>
    /// <param name="branchName">The GitHub branch name to download the resource pack from.</param>
    /// <param name="token">The one-time download token.</param>
    /// <returns>A zip file containing the resource pack.</returns>
    [AllowAnonymous]
    [HttpGet("resourcepack/download/{*branchName}")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetResourcePack([FromRoute] string branchName, [FromQuery] Guid token)
    {
        var actualBranch = Uri.UnescapeDataString(branchName);
        if (string.IsNullOrWhiteSpace(actualBranch) || !BranchNameRegex().IsMatch(actualBranch))
            return ResourceHelpers.Redirect(RedirectType.Error, null, "Invalid branch name. Branch names may only contain alphanumeric characters, dots, hyphens, underscores, and forward slashes.");

        // Validate the one-time download token
        if (!downloadTokenCache.TryGetValue(token, out var downloadToken))
            return ResourceHelpers.Redirect(RedirectType.Error, null, "Invalid or expired download token. Please request a new download link.");

        // Consume the token immediately so it cannot be reused
        downloadTokenCache.RemoveValue(token);

        // Verify the token hasn't expired
        if (downloadToken.CreatedAt + TokenExpiry < DateTime.UtcNow)
            return ResourceHelpers.Redirect(RedirectType.Error, null, "Download token has expired. Please request a new download link.");

        // Verify the token was issued for this branch
        if (!string.Equals(downloadToken.BranchName, actualBranch, StringComparison.OrdinalIgnoreCase))
            return ResourceHelpers.Redirect(RedirectType.Error, null, "Download token does not match the requested branch.");

        var result = await resourcePackService.GetResourcePack(actualBranch);
        if (!result.TryGetDataNonNull(out var resourcePack))
            return ResourceHelpers.Redirect(RedirectType.Error, null, result.ErrorMessage ?? "Unknown error. Report this.");

        var packName = configuration["GitHub:ResourcePackName"] ?? "resourcepack";
        var shortHash = resourcePack.CommitHash.Length >= 7 ? resourcePack.CommitHash[..7] : resourcePack.CommitHash;
        var fileName = $"{packName}-{actualBranch}-{shortHash}.zip";

        return File(resourcePack.ZipBytes, "application/zip", LegalFileName(fileName));
    }
}
