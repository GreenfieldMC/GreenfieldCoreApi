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

public class BuilderApplicationService(IUnitOfWork uow, IUserService userService, ILogger<BuilderApplicationService> logger) : IBuilderApplicationService
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
        return Result<bool>.Success(true);
    }

    public async Task<Result<BuilderApplication>> GetApplicationById(long applicationId)
    {
        var builderRepo = uow.Repository<IBuilderApplicationRepository>();
        var applicationResult = await builderRepo.GetApplicationById(applicationId);

        if (!applicationResult.IsSuccessful)
            return Result<BuilderApplication>.Failure(applicationResult.ErrorMessage ?? "Failed to retrieve application.", applicationResult.StatusCode);
        
        var application = applicationResult.GetNonNullOrThrow();
        
        var statusesResult = await builderRepo.GetStatusesByApplication(applicationId);
        if (!statusesResult.IsSuccessful)
            return Result<BuilderApplication>.Failure(statusesResult.ErrorMessage ?? "Failed to retrieve application statuses.", statusesResult.StatusCode);
        
        var imagesResult = await builderRepo.GetApplicationImages(applicationId);
        if (!imagesResult.IsSuccessful)
            return Result<BuilderApplication>.Failure(imagesResult.ErrorMessage ?? "Failed to retrieve application images.", imagesResult.StatusCode);
        
        var images = imagesResult.GetNonNullOrThrow().ToList();
        var statuses = statusesResult.GetNonNullOrThrow();
        
        return Result<BuilderApplication>.Success(MapApplicationEntityToModel(application, statuses.ToList(), images));
    }

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
            var statusesResult = await builderRepo.GetStatusesByApplication(appEntity.ApplicationId);
            if (!statusesResult.IsSuccessful)
                return Result<List<BuilderApplication>>.Failure(statusesResult.ErrorMessage ?? "Failed to retrieve application statuses.", statusesResult.StatusCode);
            
            var imagesResult = await builderRepo.GetApplicationImages(appEntity.ApplicationId);
            if (!imagesResult.IsSuccessful)
                return Result<List<BuilderApplication>>.Failure(imagesResult.ErrorMessage ?? "Failed to retrieve application images.", imagesResult.StatusCode);
            
            var images = imagesResult.GetNonNullOrThrow().ToList();
            var statuses = statusesResult.GetNonNullOrThrow();
            
            var appModel = MapApplicationEntityToModel(appEntity, statuses.ToList(), images);
            applications.Add(appModel);
        }
        
        return Result<List<BuilderApplication>>.Success(applications);
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

