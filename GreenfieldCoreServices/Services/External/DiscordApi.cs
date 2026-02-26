using System.Net;
using System.Text.Json;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.Connections.Discord;
using GreenfieldCoreServices.Models.Discord;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services.External;

public class DiscordApi(ILogger<IDiscordApi> logger, IConfiguration configuration, HttpClient client, IDiscordService discordService) : IDiscordApi
{
    public async Task<Result<DiscordIdentityResponse>> GetDiscordIdentity(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri("/api/v10/users/@me", UriKind.Relative));
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to retrieve Discord identity. StatusCode: {StatusCode}, ReasonPhrase: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            return Result<DiscordIdentityResponse>.Failure($"Failed to retrieve Discord identity. {response.ReasonPhrase}", response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            var model = JsonSerializer.Deserialize<DiscordIdentityResponse>(content);
            return model is null
                ? Result<DiscordIdentityResponse>.Failure("Failed to deserialize Discord identity response.")
                : Result<DiscordIdentityResponse>.Success(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while deserializing Discord identity response.");
            return Result<DiscordIdentityResponse>.Failure($"Exception occurred while deserializing Discord identity response: {ex.Message}", HttpStatusCode.InternalServerError);
        }
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
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/api/oauth2/token", UriKind.Relative));
        request.Content = new FormUrlEncodedContent(parameters);
        
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to create Discord access token. StatusCode: {StatusCode}, ReasonPhrase: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            return Result<DiscordOAuthTokenResponse>.Failure($"Failed to create Discord access token. {response.ReasonPhrase}", response.StatusCode);   
        }

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            var model = JsonSerializer.Deserialize<DiscordOAuthTokenResponse>(content);
            return model is null
                ? Result<DiscordOAuthTokenResponse>.Failure("Failed to deserialize Discord access token response.")
                : Result<DiscordOAuthTokenResponse>.Success(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while deserializing Discord access token response.");
            return Result<DiscordOAuthTokenResponse>.Failure($"Exception occurred while deserializing Discord access token response: {ex.Message}", HttpStatusCode.InternalServerError);
        }
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
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("/api/oauth2/token", UriKind.Relative));
        request.Content = new FormUrlEncodedContent(parameters);
        
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to refresh Discord access token. StatusCode: {StatusCode}, ReasonPhrase: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            return Result<DiscordOAuthTokenResponse>.Failure($"Failed to refresh Discord access token. {response.ReasonPhrase}", response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            var model = JsonSerializer.Deserialize<DiscordOAuthTokenResponse>(content);
            return model is null
                ? Result<DiscordOAuthTokenResponse>.Failure("Failed to deserialize Discord access token response.")
                : Result<DiscordOAuthTokenResponse>.Success(model);   
        } 
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while deserializing Discord access token response.");
            return Result<DiscordOAuthTokenResponse>.Failure($"Exception occurred while deserializing Discord access token response: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<UserDiscordConnection>> LinkDiscordAccountToUser(long userId, string code)
    {
        var tokenResult = await CreateDiscordAccessTokenAsync(code);
        if (!tokenResult.TryGetDataNonNull(out var tokenData))
            return Result<UserDiscordConnection>.Failure(tokenResult.ErrorMessage!, tokenResult.StatusCode);

        var identityResult = await GetDiscordIdentity(tokenData.AccessToken);
        if (!identityResult.TryGetDataNonNull(out var identity))
            return Result<UserDiscordConnection>.Failure(identityResult.ErrorMessage!, identityResult.StatusCode);

        if (!ulong.TryParse(identity.Id, out var discordSnowflake))
            return Result<UserDiscordConnection>.Failure("Failed to parse Discord user id.");
        
        var existingConnection = await discordService.GetDiscordConnectionBySnowflake(discordSnowflake);
        if (existingConnection.TryGetDataNonNull(out var discordConnection))
        {
            var updateExistingResult = await discordService.UpdateDiscordConnectionTokens(discordConnection.DiscordConnectionId, tokenData.RefreshToken, tokenData.AccessToken, tokenData.TokenType, DateTime.Now.AddSeconds(tokenData.ExpiresIn), tokenData.Scope);
            if (!updateExistingResult.IsSuccessful)
            {
                logger.LogError("Failed to update existing Discord connection tokens for DiscordConnectionId {DiscordConnectionId}: {ErrorMessage}", discordConnection.DiscordConnectionId, updateExistingResult.ErrorMessage);
                return Result<UserDiscordConnection>.Failure(updateExistingResult.ErrorMessage!, updateExistingResult.StatusCode);
            }
            
            var existingLinkResult = await discordService.LinkUserToDiscordConnection(userId, discordConnection.DiscordConnectionId);
            return !existingLinkResult.TryGetDataNonNull(out var existingUser)
                ? Result<UserDiscordConnection>.Failure(existingLinkResult.ErrorMessage!, existingLinkResult.StatusCode)
                : Result<UserDiscordConnection>.Success(existingUser);
        }

        var createConnection = await discordService.CreateDiscordConnection(tokenData.RefreshToken, tokenData.AccessToken, tokenData.TokenType, DateTime.Now.AddSeconds(tokenData.ExpiresIn), tokenData.Scope, discordSnowflake, identity.GlobalName ?? identity.Username);
        if (!createConnection.TryGetDataNonNull(out var connection))
            return Result<UserDiscordConnection>.Failure(createConnection.ErrorMessage!, createConnection.StatusCode);

        var linkResult = await discordService.LinkUserToDiscordConnection(userId, connection.DiscordConnectionId);
        
        return !linkResult.TryGetDataNonNull(out var userConnection)
            ? Result<UserDiscordConnection>.Failure(linkResult.ErrorMessage!, linkResult.StatusCode)
            : Result<UserDiscordConnection>.Success(userConnection);
    }

    public async Task<Result<DiscordConnection>> RefreshDiscordConnectionData(long discordConnectionId)
    {
        var connectionResult = await discordService.GetDiscordConnection(discordConnectionId);
        if (!connectionResult.TryGetDataNonNull(out var connection))
            return Result<DiscordConnection>.Failure(connectionResult.ErrorMessage!, connectionResult.StatusCode);

        if (connection.RefreshBy <= DateTime.UtcNow)
            return Result<DiscordConnection>.Failure(
                "The Discord access token for this connection has expired. The user must re-link their Discord account.",
                HttpStatusCode.FailedDependency);

        var identityResult = await GetDiscordIdentity(connection.AccessToken);
        if (!identityResult.TryGetDataNonNull(out var identity))
            return Result<DiscordConnection>.Failure(identityResult.ErrorMessage!, identityResult.StatusCode);

        var discordUsername = identity.GlobalName ?? identity.Username;
        return await discordService.UpdateDiscordConnectionProfile(discordConnectionId, discordUsername);
    }
}
