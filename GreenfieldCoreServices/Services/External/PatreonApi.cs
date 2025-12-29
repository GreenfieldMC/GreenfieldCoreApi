using System.Text.Json;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Patreon;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GreenfieldCoreServices.Services.External;

public class PatreonApi(IConfiguration configuration, HttpClient client, IPatreonService patreonService) : IPatreonApi
{
    private static string _cachedCampaignId = "";

    public async Task<Result<PatreonUserIdentityModel>> GetPatreonIdentity(string accessToken)
    {
        var uri = new Uri("v2/identity?fields[user]=full_name&fields[member]=currently_entitled_amount_cents&include=memberships,memberships.campaign", UriKind.Relative);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
            return Result<PatreonUserIdentityModel>.Failure($"Failed to retrieve Patreon identity. {response.ReasonPhrase}", response.StatusCode);
            
        var content = await response.Content.ReadAsStringAsync();
        var model = JsonSerializer.Deserialize<PatreonUserIdentityModel>(content);
        
        return model is null 
            ? Result<PatreonUserIdentityModel>.Failure("Failed to deserialize Patreon identity response.") 
            : Result<PatreonUserIdentityModel>.Success(model);
    }

    public async Task<Result<PatreonOAuthTokenResponse>> CreatePatreonAccessTokenAsync(string authorizationCode)
    {
        var uri = new Uri("token", UriKind.Relative);
        var parameters = new Dictionary<string, string>
        {
            { "code", authorizationCode },
            { "grant_type", "authorization_code" },
            { "client_id", configuration["Patreon:ClientId"]! },
            { "client_secret", configuration["Patreon:ClientSecret"]! },
            { "redirect_uri", configuration["Patreon:RedirectUri"]! }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = new FormUrlEncodedContent(parameters);
        
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return Result<PatreonOAuthTokenResponse>.Failure($"Failed to create Patreon access token. {response.ReasonPhrase}", response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var model = JsonSerializer.Deserialize<PatreonOAuthTokenResponse>(content);

        return model is null
            ? Result<PatreonOAuthTokenResponse>.Failure("Failed to deserialize Patreon access token response.")
            : Result<PatreonOAuthTokenResponse>.Success(model);
    }

    public async Task<Result<PatreonOAuthTokenResponse>> RefreshPatreonAccessTokenAsync(string refreshToken)
    {
        var uri = new Uri("token", UriKind.Relative);
        var parameters = new Dictionary<string, string>
        {
            { "refresh_token", refreshToken },
            { "grant_type", "refresh_token" },
            { "client_id", configuration["Patreon:ClientId"]! },
            { "client_secret", configuration["Patreon:ClientSecret"]! }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Content = new FormUrlEncodedContent(parameters);

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return Result<PatreonOAuthTokenResponse>.Failure($"Failed to refresh Patreon access token. {response.ReasonPhrase}", response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var model = JsonSerializer.Deserialize<PatreonOAuthTokenResponse>(content);

        return model is null
            ? Result<PatreonOAuthTokenResponse>.Failure("Failed to deserialize Patreon access token response.")
            : Result<PatreonOAuthTokenResponse>.Success(model);
    }

    public async Task<Result<UserPatreonAccount>> LinkPatreonAccountToUser(long userId, string code, string state)
    {
        var campaignIdTask = ResolveCampaignId();
        
        var tokenResult = await CreatePatreonAccessTokenAsync(code);
        if (!tokenResult.TryGetDataNonNull(out var tokenData))
            return Result<UserPatreonAccount>.Failure(tokenResult.ErrorMessage!, tokenResult.StatusCode);

        var identityResult = await GetPatreonIdentity(tokenData.AccessToken);
        if (!identityResult.TryGetDataNonNull(out var identityData))
            return Result<UserPatreonAccount>.Failure(identityResult.ErrorMessage!, identityResult.StatusCode);

        if (!long.TryParse(identityData.Data.Id, out var patreonId))
            return Result<UserPatreonAccount>.Failure("Failed to parse Patreon ID.");

        var campaignIdResult = await campaignIdTask;
        if (!campaignIdResult.TryGetDataNonNull(out var campaignId))
            return Result<UserPatreonAccount>.Failure(campaignIdResult.ErrorMessage!, campaignIdResult.StatusCode);
        
        var pledge = identityData.GetPledgedAmountOfCampaign(campaignId);
        var fullName = identityData.Data.Attributes?.FullName ?? "Unknown Patreon User";
        var linkResult = await patreonService.CreatePatreonAccountReference(userId, patreonId, tokenData.RefreshToken, tokenData.AccessToken, tokenData.TokenType, DateTime.Now.AddSeconds(tokenData.ExpiresIn), tokenData.Scope, fullName, pledge);
        return !linkResult.TryGetDataNonNull(out var userPatreonAccount) 
            ? Result<UserPatreonAccount>.Failure(linkResult.ErrorMessage!, linkResult.StatusCode) 
            : Result<UserPatreonAccount>.Success(userPatreonAccount);
    }

    public async Task<Result<string>> ResolveCampaignId()
    {
        if (!string.IsNullOrWhiteSpace(_cachedCampaignId))
            return Result<string>.Success(_cachedCampaignId);
        
        var creatorAccessToken = configuration["Patreon:CreatorAccessToken"] ?? throw new Exception("Patreon creator access token is not configured.");
        var campaignUrl = configuration["Patreon:CampaignUrl"] ?? throw new Exception("Patreon campaign URL is not configured.");
        var uri = new Uri("v2/campaigns?fields[campaign]=url", UriKind.Relative);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", creatorAccessToken);
        var response = client.SendAsync(request).Result;
        
        if (!response.IsSuccessStatusCode)
            return Result<string>.Failure($"Failed to retrieve Patreon campaign ID: {response.ReasonPhrase}", response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var campaignList = JsonSerializer.Deserialize<PatreonCampaignListModel>(content);
        if (campaignList?.Data is null)
            return Result<string>.Failure("Failed to deserialize Patreon campaign response.");
        
        var foundCampaign = campaignList.Data.FirstOrDefault(c => c.Attributes.Url.Equals(campaignUrl, StringComparison.OrdinalIgnoreCase));
        if (foundCampaign is null)
            return Result<string>.Failure("No matching Patreon campaign found for the configured URL.");
        
        _cachedCampaignId = foundCampaign.Id;
        return Result<string>.Success(foundCampaign.Id);
    }
}