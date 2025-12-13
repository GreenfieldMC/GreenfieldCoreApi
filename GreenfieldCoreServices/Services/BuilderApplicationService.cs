using System.Net;
using System.Linq;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.BuildApps;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services;

public class BuilderApplicationService(IUnitOfWork uow, IUserService userService, ILogger<BuilderApplicationService> logger, ICacheService<long, BuilderApplication> buildAppCache) : IBuilderApplicationService
{
    public async Task<Result<long>> SubmitApplication(
        ulong discordSnowflake,
        string minecraftUsername,
        Guid minecraftUuid,
        int age,
        string? nationality,
        List<string> houseBuildLinks,
        List<string> otherBuildLinks,
        string? additionalBuildingInformation,
        string whyJoinGreenfield,
        string? additionalComments)
    {
        var builderRepo = uow.Repository<IBuilderApplicationRepository>();

        var userResult = await EnsureUser(minecraftUuid, minecraftUsername);
        if (!userResult.IsSuccessful)
            return Result<long>.Failure(userResult.ErrorMessage ?? "Failed to resolve user.", userResult.StatusCode);

        var user = userResult.GetNonNullOrThrow();

        var discordLinkResult = await EnsureDiscordLink(user.UserId, discordSnowflake);
        if (!discordLinkResult.IsSuccessful)
            return Result<long>.Failure(discordLinkResult.ErrorMessage ?? "Failed to link Discord account.", discordLinkResult.StatusCode);

        var applicationsResult = await builderRepo.GetApplicationsByUser(user.UserId);
        if (!applicationsResult.IsSuccessful)
            return Result<long>.Failure(applicationsResult.ErrorMessage ?? "Failed to retrieve existing applications.", applicationsResult.StatusCode);

        var applications = applicationsResult.GetNonNullOrThrow("GetApplicationsByUser returned null unexpectedly.");
        foreach (var application in applications)
        {
            var statusResult = await builderRepo.GetStatusesByApplication(application.ApplicationId);
            if (!statusResult.IsSuccessful)
                return Result<long>.Failure(statusResult.ErrorMessage ?? "Failed to retrieve application statuses.", statusResult.StatusCode);

            var statuses = statusResult.GetNonNullOrThrow("GetStatusesByApplication returned null unexpectedly.").ToList();
            if (statuses.Any(s => string.Equals(s.Status, "UnderReview", StringComparison.OrdinalIgnoreCase)))
                return Result<long>.Failure("An application is already under review for this user.", HttpStatusCode.Conflict);

            if (statuses.Any(s => string.Equals(s.Status, "Approved", StringComparison.OrdinalIgnoreCase)))
                return Result<long>.Failure("An application for this Minecraft UUID has already been approved.", HttpStatusCode.Conflict);
        }

        uow.BeginTransaction();
        try
        {
            var applicationInsertResult = await builderRepo.InsertApplication(
                user.UserId,
                age,
                nationality ?? string.Empty,
                additionalBuildingInformation,
                whyJoinGreenfield,
                additionalComments);

            if (!applicationInsertResult.IsSuccessful)
                return Result<long>.Failure(applicationInsertResult.ErrorMessage ?? "Failed to insert application.", applicationInsertResult.StatusCode);

            var application = applicationInsertResult.GetNonNullOrThrow();
            var applicationId = application.ApplicationId;

            if (houseBuildLinks.Count > 0)
            {
                foreach (var link in houseBuildLinks.Where(link => !string.IsNullOrWhiteSpace(link)))
                {
                    var imageResult = await builderRepo.InsertImage(applicationId, "House", link);
                    if (imageResult.IsSuccessful) continue;
                    logger.LogWarning("Failed to insert house image for builder application {ApplicationId} (link: {Link}). {Error}", applicationId, link, imageResult.ErrorMessage);
                }
            }

            if (otherBuildLinks.Count > 0)
            {
                foreach (var link in otherBuildLinks.Where(link => !string.IsNullOrWhiteSpace(link)))
                {
                    var imageResult = await builderRepo.InsertImage(applicationId, "Other", link);
                    if (imageResult.IsSuccessful) continue;
                    logger.LogWarning("Failed to insert other image for builder application {ApplicationId} (link: {Link}). {Error}", applicationId, link, imageResult.ErrorMessage);
                }
            }

            var statusInsertResult = await builderRepo.InsertStatus(applicationId, "UnderReview", null);
            if (!statusInsertResult.IsSuccessful)
                logger.LogWarning("Failed to insert initial status for builder application {ApplicationId}. {Error}", applicationId, statusInsertResult.ErrorMessage);
            
            uow.CompleteAndCommit();
            
            var cacheResult = await GetApplicationInternal(applicationId, true, application, builderRepo);
            if (!cacheResult.IsSuccessful)
                logger.LogWarning("Failed to cache builder application {ApplicationId} after submission. {Error}", applicationId, cacheResult.ErrorMessage);
            
            return Result<long>.Success(applicationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure while submitting builder application for Minecraft UUID {MinecraftUuid}.", minecraftUuid);
            return Result<long>.Failure("An unexpected error occurred while submitting the application.", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<bool>> AddApplicationStatus(long applicationId, string status, string? statusMessage)
    {
        var builderRepo = uow.Repository<IBuilderApplicationRepository>();
        uow.BeginTransaction();

        var statusResult = await builderRepo.InsertStatus(applicationId, status, statusMessage);
        if (!statusResult.IsSuccessful)
        {
            logger.LogWarning("Failed to insert builder application status {Status} for application {ApplicationId}. {Error}", status, applicationId, statusResult.ErrorMessage);
            return Result<bool>.Failure(statusResult.ErrorMessage ?? "Failed to insert application status.", statusResult.StatusCode);
        }

        uow.CompleteAndCommit();

        if (buildAppCache.TryGetValue(applicationId, out var cachedApplication))
        {
            var refreshedStatusesResult = await builderRepo.GetStatusesByApplication(applicationId);
            if (refreshedStatusesResult.IsSuccessful)
            {
                cachedApplication.BuildAppStatuses = refreshedStatusesResult
                    .GetNonNullOrThrow()
                    .Select(s => new BuildAppStatus(s.Status, s.StatusMessage, s.CreatedOn))
                    .ToList();
            }
            else
            {
                logger.LogWarning("Failed to refresh cache statuses for builder application {ApplicationId}. {Error}", applicationId, refreshedStatusesResult.ErrorMessage);
                cachedApplication.BuildAppStatuses.Add(new BuildAppStatus(status, statusMessage, DateTime.UtcNow));
            }

            buildAppCache.SetValue(applicationId, cachedApplication);
        }
        else
        {
            var cacheRefreshResult = await GetApplicationInternal(applicationId, true, null, builderRepo);
            if (!cacheRefreshResult.IsSuccessful)
                logger.LogWarning("Failed to cache builder application {ApplicationId} after status update. {Error}", applicationId, cacheRefreshResult.ErrorMessage);
        }

        return Result<bool>.Success(true);
    }

    public Task<Result<BuilderApplication>> GetApplicationById(long applicationId) =>
        GetApplicationInternal(applicationId);

    public async Task<Result<List<BuilderApplication>>> GetApplicationsFromUser(long userId)
    {
        var builderRepo = uow.Repository<IBuilderApplicationRepository>();
        var userAppsResult = await builderRepo.GetApplicationsByUser(userId);
        if (!userAppsResult.IsSuccessful)
            return Result<List<BuilderApplication>>.Failure(userAppsResult.ErrorMessage ?? "Failed to retrieve user applications.", userAppsResult.StatusCode);
        
        var userApps = userAppsResult.GetNonNullOrThrow();
        var applications = new List<BuilderApplication>();
        
        foreach (var appEntity in userApps)
        {
            var appResult = await GetApplicationInternal(appEntity.ApplicationId, false, appEntity, builderRepo);
            if (!appResult.IsSuccessful)
                return Result<List<BuilderApplication>>.Failure(appResult.ErrorMessage ?? "Failed to retrieve application.", appResult.StatusCode);
            
            applications.Add(appResult.GetNonNullOrThrow());
        }
        
        return Result<List<BuilderApplication>>.Success(applications);
    }

    private async Task<Result<BuilderApplication>> GetApplicationInternal(long applicationId, bool bypassCache = false, BuilderApplicationEntity? existingEntity = null, IBuilderApplicationRepository? repository = null)
    {
        if (!bypassCache && buildAppCache.TryGetValue(applicationId, out var cachedApplication))
            return Result<BuilderApplication>.Success(cachedApplication);
        
        var builderRepo = repository ?? uow.Repository<IBuilderApplicationRepository>();
        BuilderApplicationEntity applicationEntity;
        
        if (existingEntity is not null) applicationEntity = existingEntity;
        else
        {
            var applicationResult = await builderRepo.GetApplicationById(applicationId);

            if (!applicationResult.IsSuccessful)
                return Result<BuilderApplication>.Failure(applicationResult.ErrorMessage ?? "Failed to retrieve application.", applicationResult.StatusCode);
            
            applicationEntity = applicationResult.GetNonNullOrThrow();
        }
        
        var statusesResult = await builderRepo.GetStatusesByApplication(applicationId);
        if (!statusesResult.IsSuccessful)
            return Result<BuilderApplication>.Failure(statusesResult.ErrorMessage ?? "Failed to retrieve application statuses.", statusesResult.StatusCode);
        
        var imagesResult = await builderRepo.GetApplicationImages(applicationId);
        if (!imagesResult.IsSuccessful)
            return Result<BuilderApplication>.Failure(imagesResult.ErrorMessage ?? "Failed to retrieve application images.", imagesResult.StatusCode);
        
        var images = imagesResult.GetNonNullOrThrow().ToList();
        var statuses = statusesResult.GetNonNullOrThrow().ToList();
        
        var applicationModel = MapApplicationEntityToModel(applicationEntity, statuses, images);
        buildAppCache.SetValue(applicationId, applicationModel);
        return Result<BuilderApplication>.Success(applicationModel);
    }

    private BuilderApplication MapApplicationEntityToModel(BuilderApplicationEntity entity,
        List<BuilderAppStatusEntity> statuses,
        List<BuilderAppImageLinkEntity> images)
    {
        return new BuilderApplication
        {
            ApplicationId = entity.ApplicationId,
            UserId = entity.UserId,
            Age = entity.UserAge,
            Nationality = entity.UserNationality,
            AdditionalBuildingInformation = entity.AdditionalBuildingInformation,
            WhyJoinGreenfield = entity.WhyJoinGreenfield,
            AdditionalComments = entity.AdditionalComments,
            CreatedOn = entity.CreatedOn,
            BuildAppStatuses = statuses
                .Select(s => new BuildAppStatus(s.Status, s.StatusMessage, s.CreatedOn))
                .ToList(),
            HouseBuilds = images
                .Where(img => string.Equals(img.LinkType, "House", StringComparison.OrdinalIgnoreCase))
                .Select(img => new BuildAppImage(img.LinkType, img.ImageLink, img.CreatedOn))
                .ToList(),
            OtherBuilds = images
                .Where(img => string.Equals(img.LinkType, "Other", StringComparison.OrdinalIgnoreCase))
                .Select(img => new BuildAppImage(img.LinkType, img.ImageLink, img.CreatedOn))
                .ToList()
        };
    }

    private async Task<Result<User>> EnsureUser(Guid minecraftUuid, string minecraftUsername)
    {
        var userResult = await userService.GetUserByUuid(minecraftUuid);
        if (userResult.IsSuccessful || userResult.StatusCode != HttpStatusCode.NotFound)
            return userResult;

        return await userService.CreateUser(minecraftUuid, minecraftUsername);
    }

    private async Task<Result<bool>> EnsureDiscordLink(long userId, ulong discordSnowflake)
    {
        var existingLinksResult = await userService.GetLinkedDiscordAccounts(userId);
        if (!existingLinksResult.IsSuccessful)
            return Result<bool>.Failure(existingLinksResult.ErrorMessage ?? "Failed to retrieve linked Discord accounts.", existingLinksResult.StatusCode);

        var existingLinks = existingLinksResult.GetNonNullOrThrow();
        if (existingLinks.Contains(discordSnowflake))
            return Result<bool>.Success(true);

        return await userService.LinkDiscordAccount(userId, discordSnowflake);
    }
}
