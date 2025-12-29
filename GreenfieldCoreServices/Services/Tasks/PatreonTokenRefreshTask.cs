using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services.Tasks;

public class PatreonTokenRefreshTask(TaskStartSignalService startSignal, IServiceScopeFactory scopeFactory, ILogger<PatreonTokenRefreshTask> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await startSignal.WaitForStartAsync(stoppingToken);
        
        await RefreshPatreonTokensAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                await RefreshPatreonTokensAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled, exit gracefully
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the Patreon token refresh task.");
            }
        }
        
    }
    
    private async Task RefreshPatreonTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var patreonService = scope.ServiceProvider.GetRequiredService<IPatreonService>();
        var patreonApi = scope.ServiceProvider.GetRequiredService<IPatreonApi>();
        
        logger.LogInformation("Starting Patreon token refresh task at {Time}", DateTimeOffset.Now);
        var allAccountsResult = await patreonService.GetAllPatreonAccounts();

        if (!allAccountsResult.TryGetDataNonNull(out var patreonAccounts))
        {
            logger.LogWarning("Failed to retrieve Patreon accounts for token refresh. Error: {ErrorMessage}", allAccountsResult.ErrorMessage);
            return;
        }
        
        foreach (var userPatreonAccount in patreonAccounts)
        {
            var isExpired = userPatreonAccount.RefreshBy <= DateTime.Now;
            if (isExpired)
            {
                logger.LogWarning("Patreon user {PatreonId} token for user {UserId} has expired. Unlinking the account.", userPatreonAccount.PatreonId, userPatreonAccount.UserId);
                _ = patreonService
                    .UnlinkPatreonAccountReference(userPatreonAccount.UserId, userPatreonAccount.PatreonId)
                    .ContinueWith(async (task, _) =>
                    {
                        var result = await task;
                        if (!result.IsSuccessful) 
                            logger.LogError("Failed to unlink expired Patreon account {PatreonId} for user {UserId}. Error: {ErrorMessage}", userPatreonAccount.PatreonId, userPatreonAccount.UserId, result.ErrorMessage);

                        logger.LogInformation("Successfully unlinked expired Patreon account {PatreonId} for user {UserId}", userPatreonAccount.PatreonId, userPatreonAccount.UserId);
                    }, null, cancellationToken);
                continue;
            }
            
            var timeUntilRefresh = userPatreonAccount.RefreshBy - DateTime.Now;
            if (timeUntilRefresh > TimeSpan.FromDays(2))
                continue;
            
            var refreshResult = await patreonApi.RefreshPatreonAccessTokenAsync(userPatreonAccount.RefreshToken);
            if (!refreshResult.TryGetDataNonNull(out var tokenResponse))
            {
                logger.LogError("Failed to refresh Patreon token for user {UserId} PatreonId {PatreonId}. Error: {ErrorMessage}", 
                    userPatreonAccount.UserId, userPatreonAccount.PatreonId, refreshResult.ErrorMessage);
                continue;
            }

            var tokenUpdateResult = await patreonService.UpdatePatreonAccountTokens(userPatreonAccount.UserId, userPatreonAccount.PatreonId,
                tokenResponse.RefreshToken, tokenResponse.AccessToken, tokenResponse.TokenType,
                DateTime.Now.AddSeconds(tokenResponse.ExpiresIn), tokenResponse.Scope);
            if (!tokenUpdateResult.TryGetDataNonNull(out var patreonData))
            {
                logger.LogError("Failed to update Patreon tokens in database for user {UserId} PatreonId {PatreonId}. Error: {ErrorMessage}", 
                    userPatreonAccount.UserId, userPatreonAccount.PatreonId, tokenUpdateResult.ErrorMessage);
                continue;
            }

            var identityResponse = await patreonApi.GetPatreonIdentity(tokenResponse.AccessToken);
            if (!identityResponse.TryGetDataNonNull(out var patreonIdentity))
            {
                logger.LogError("Failed to fetch Patreon identity for user {UserId} PatreonId {PatreonId}. Error: {ErrorMessage}", 
                    userPatreonAccount.UserId, userPatreonAccount.PatreonId, identityResponse.ErrorMessage);
                continue;
            }

            var campaignIdResult = await patreonApi.ResolveCampaignId();
            if (!campaignIdResult.TryGetDataNonNull(out var campaignId))
            {
                logger.LogError("Failed to resolve Patreon campaign ID for user {UserId} PatreonId {PatreonId}. Error: {ErrorMessage}",
                    userPatreonAccount.UserId, userPatreonAccount.PatreonId, campaignIdResult.ErrorMessage);
                continue;
            }

            var fullName = patreonIdentity.Data.Attributes?.FullName ?? "Unknown Patreon User";
            var pledgedAmount = patreonIdentity.GetPledgedAmountOfCampaign(campaignId);
            _ = patreonService.UpdatePatreonAccountInfo(patreonData.UserId, patreonData.PatreonId, fullName, pledgedAmount);
            logger.LogInformation("Successfully refreshed Patreon token for user {UserId} PatreonId {PatreonId}. New pledged amount: {PledgedAmount}",
                userPatreonAccount.UserId, userPatreonAccount.PatreonId, pledgedAmount);
        }
            
    }
    
}