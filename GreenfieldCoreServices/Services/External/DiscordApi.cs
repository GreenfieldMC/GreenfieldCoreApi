using System.Text.Json;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Discord;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GreenfieldCoreServices.Services.External;

public class DiscordApi(IConfiguration configuration, HttpClient client, IDiscordService discordService) : IDiscordApi
{
    public async Task<Result<DiscordIdentityResponse>> GetDiscordIdentity(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/api/v10/users/@me", UriKind.Relative));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return Result<DiscordIdentityResponse>.Failure($"Failed to retrieve Discord identity. {response.ReasonPhrase}", response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var model = JsonSerializer.Deserialize<DiscordIdentityResponse>(content);
        return model is null
            ? Result<DiscordIdentityResponse>.Failure("Failed to deserialize Discord identity response.")
            : Result<DiscordIdentityResponse>.Success(model);
    }

    public async Task<Result<DiscordOAuthTokenResponse>> CreateDiscordAccessTokenAsync(string authorizationCode)
    {
        var parameters = new Dictionary<string, string>
        {
            { "code", authorizationCode },
            { "grant_type", "authorization_code" },
            { "client_id", configuration["Discord:ClientId"]! },
            { "client_secret", configuration["Discord:ClientSecret"]! },
            { "redirect_uri", configuration["Discord:RedirectUri"]! }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/api/oauth2/token", UriKind.Relative))
        {
            Content = new FormUrlEncodedContent(parameters)
        };
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return Result<DiscordOAuthTokenResponse>.Failure($"Failed to create Discord access token. {response.ReasonPhrase}", response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var model = JsonSerializer.Deserialize<DiscordOAuthTokenResponse>(content);
        return model is null
            ? Result<DiscordOAuthTokenResponse>.Failure("Failed to deserialize Discord access token response.")
            : Result<DiscordOAuthTokenResponse>.Success(model);
    }

    public async Task<Result<DiscordOAuthTokenResponse>> RefreshDiscordAccessTokenAsync(string refreshToken)
    {
        var parameters = new Dictionary<string, string>
        {
            { "refresh_token", refreshToken },
            { "grant_type", "refresh_token" },
            { "client_id", configuration["Discord:ClientId"]! },
            { "client_secret", configuration["Discord:ClientSecret"]! }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/api/oauth2/token", UriKind.Relative))
        {
            Content = new FormUrlEncodedContent(parameters)
        };
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return Result<DiscordOAuthTokenResponse>.Failure($"Failed to refresh Discord access token. {response.ReasonPhrase}", response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var model = JsonSerializer.Deserialize<DiscordOAuthTokenResponse>(content);
        return model is null
            ? Result<DiscordOAuthTokenResponse>.Failure("Failed to deserialize Discord access token response.")
            : Result<DiscordOAuthTokenResponse>.Success(model);
    }

    public async Task<Result<UserDiscordAccount>> LinkDiscordAccountToUser(long userId, string code)
    {
        var tokenResult = await CreateDiscordAccessTokenAsync(code);
        if (!tokenResult.TryGetDataNonNull(out var tokenData))
            return Result<UserDiscordAccount>.Failure(tokenResult.ErrorMessage!, tokenResult.StatusCode);

        var identityResult = await GetDiscordIdentity(tokenData.AccessToken);
        if (!identityResult.TryGetDataNonNull(out var identity))
            return Result<UserDiscordAccount>.Failure(identityResult.ErrorMessage!, identityResult.StatusCode);

        if (!ulong.TryParse(identity.Id, out var discordSnowflake))
            return Result<UserDiscordAccount>.Failure("Failed to parse Discord user id.");

        var linkResult = await discordService.CreateDiscordAccountReference(
            userId,
            discordSnowflake,
            identity.GlobalName ?? identity.Username,
            tokenData.RefreshToken,
            tokenData.AccessToken,
            tokenData.TokenType,
            DateTime.Now.AddSeconds(tokenData.ExpiresIn),
            tokenData.Scope);
        return !linkResult.TryGetDataNonNull(out var discordAccount)
            ? Result<UserDiscordAccount>.Failure(linkResult.ErrorMessage!, linkResult.StatusCode)
            : Result<UserDiscordAccount>.Success(discordAccount);
    }
}
