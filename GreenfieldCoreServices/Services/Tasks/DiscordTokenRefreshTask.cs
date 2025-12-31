using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services.Tasks;

public class DiscordTokenRefreshTask(TaskStartSignalService startSignal, IServiceScopeFactory scopeFactory, ILogger<DiscordTokenRefreshTask> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await startSignal.WaitForStartAsync(stoppingToken);

        await RefreshDiscordTokensAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                await RefreshDiscordTokensAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the Discord token refresh task.");
            }
        }
    }

    private async Task RefreshDiscordTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var discordService = scope.ServiceProvider.GetRequiredService<IDiscordService>();
        var discordApi = scope.ServiceProvider.GetRequiredService<IDiscordApi>();

        logger.LogInformation("Starting Discord token refresh task at {Time}", DateTimeOffset.Now);
        var allAccountsResult = await discordService.GetAllDiscordAccounts();
        if (!allAccountsResult.TryGetDataNonNull(out var accounts))
        {
            logger.LogWarning("Failed to retrieve Discord accounts for token refresh. Error: {ErrorMessage}", allAccountsResult.ErrorMessage);
            return;
        }

        foreach (var account in accounts)
        {
            var expiresIn = account.RefreshBy - DateTime.Now;
            if (account.RefreshBy <= DateTime.Now)
            {
                logger.LogWarning("Discord token expired for user {UserId} discord {DiscordSnowflake}. Unlinking.", account.UserId, account.DiscordSnowflake);
                _ = discordService.UnlinkDiscordAccountReference(account.UserId, account.DiscordSnowflake)
                    .ContinueWith(async t =>
                    {
                        var result = await t;
                        if (!result.IsSuccessful)
                            logger.LogError("Failed to unlink expired Discord account {DiscordSnowflake} for user {UserId}. Error: {ErrorMessage}", account.DiscordSnowflake, account.UserId, result.ErrorMessage);
                    }, cancellationToken);
                continue;
            }

            if (expiresIn > TimeSpan.FromDays(2))
                continue;

            var refreshResult = await discordApi.RefreshDiscordAccessTokenAsync(account.RefreshToken);
            if (!refreshResult.TryGetDataNonNull(out var tokenResponse))
            {
                logger.LogError("Failed to refresh Discord token for user {UserId} discord {DiscordSnowflake}. Error: {ErrorMessage}", account.UserId, account.DiscordSnowflake, refreshResult.ErrorMessage);
                continue;
            }

            var latestUsername = account.DiscordUsername;
            var identityResult = await discordApi.GetDiscordIdentity(tokenResponse.AccessToken);
            if (identityResult.TryGetDataNonNull(out var identity))
                latestUsername = identity.GlobalName ?? identity.Username;
            else
                logger.LogWarning("Failed to fetch Discord identity for user {UserId} discord {DiscordSnowflake}. Username will not be updated. Error: {ErrorMessage}", account.UserId, account.DiscordSnowflake, identityResult.ErrorMessage);

            var updateTokensResult = await discordService.UpdateDiscordAccountTokens(
                account.UserId,
                account.DiscordSnowflake,
                tokenResponse.RefreshToken,
                tokenResponse.AccessToken,
                tokenResponse.TokenType,
                DateTime.Now.AddSeconds(tokenResponse.ExpiresIn),
                tokenResponse.Scope);
            if (!updateTokensResult.IsSuccessful)
            {
                logger.LogError("Failed to update Discord tokens in database for user {UserId} discord {DiscordSnowflake}. Error: {ErrorMessage}", account.UserId, account.DiscordSnowflake, updateTokensResult.ErrorMessage);
                continue;
            }

            if (identityResult.IsSuccessful && latestUsername != account.DiscordUsername)
            {
                var updateProfileResult = await discordService.UpdateDiscordAccountProfile(account.UserId, account.DiscordSnowflake, latestUsername);
                if (!updateProfileResult.IsSuccessful)
                    logger.LogWarning("Failed to update Discord profile for user {UserId} discord {DiscordSnowflake}. Error: {ErrorMessage}", account.UserId, account.DiscordSnowflake, updateProfileResult.ErrorMessage);
            }

            logger.LogInformation("Refreshed Discord token for user {UserId} discord {DiscordSnowflake}", account.UserId, account.DiscordSnowflake);
        }
    }
}
