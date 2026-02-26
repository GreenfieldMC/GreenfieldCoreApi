using System.Net;
using System.Text.Json;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Resources;
using GreenfieldCoreServices.Services.External.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services.External;

public class GitHubApi(ILogger<IGitHubApi> logger, IConfiguration configuration, HttpClient client) : IGitHubApi
{
    private string Owner => configuration["GitHub:Owner"] ?? throw new InvalidOperationException("GitHub:Owner is not configured.");
    private string Repo => configuration["GitHub:Repo"] ?? throw new InvalidOperationException("GitHub:Repo is not configured.");
    private string? Token => configuration["GitHub:Token"];

    private void ApplyHeaders(HttpRequestMessage request, string? accept = null)
    {
        request.Headers.Add("User-Agent", "GreenfieldCoreApi");
        if (accept is not null)
            request.Headers.Add("Accept", accept);
        if (Token is not null)
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("token", Token);
    }

    public async Task<Result<List<GitHubBranch>>> GetBranches()
    {
        var uri = new Uri($"/repos/{Owner}/{Repo}/branches", UriKind.Relative);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        ApplyHeaders(request, "application/vnd.github+json");

        try
        {
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get branches. StatusCode: {StatusCode}, Reason: {Reason}",
                    response.StatusCode, response.ReasonPhrase);
                return Result<List<GitHubBranch>>.Failure($"Failed to get branches. {response.ReasonPhrase}", response.StatusCode);
            }

            var content = await response.Content.ReadAsStringAsync();
            var branches = JsonSerializer.Deserialize<List<GitHubBranch>>(content);
            return branches is null
                ? Result<List<GitHubBranch>>.Failure("Failed to deserialize branches response.")
                : Result<List<GitHubBranch>>.Success(branches);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while fetching branches");
            return Result<List<GitHubBranch>>.Failure($"Exception occurred while fetching branches: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<string>> GetLatestCommitHash(string branchName)
    {
        var uri = new Uri($"/repos/{Owner}/{Repo}/commits/{branchName}", UriKind.Relative);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        ApplyHeaders(request, "application/vnd.github.sha");

        try
        {
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to get latest commit hash for branch {Branch}. StatusCode: {StatusCode}, Reason: {Reason}",
                    branchName, response.StatusCode, response.ReasonPhrase);
                return Result<string>.Failure($"Failed to get latest commit hash for branch '{branchName}'. {response.ReasonPhrase}", response.StatusCode);
            }

            var commitHash = (await response.Content.ReadAsStringAsync()).Trim();
            return Result<string>.Success(commitHash);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while fetching commit hash for branch {Branch}", branchName);
            return Result<string>.Failure($"Exception occurred while fetching commit hash: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<byte[]>> DownloadBranchZip(string branchName)
    {
        var url = $"https://github.com/{Owner}/{Repo}/archive/refs/heads/{branchName}.zip";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyHeaders(request);

        try
        {
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Failed to download zip for branch {Branch}. StatusCode: {StatusCode}, Reason: {Reason}",
                    branchName, response.StatusCode, response.ReasonPhrase);
                return Result<byte[]>.Failure($"Failed to download zip for branch '{branchName}'. {response.ReasonPhrase}", response.StatusCode);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return Result<byte[]>.Success(bytes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while downloading zip for branch {Branch}", branchName);
            return Result<byte[]>.Failure($"Exception occurred while downloading zip: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
