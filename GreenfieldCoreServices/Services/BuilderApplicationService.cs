using System.Net;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.BuildApps;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services;

public class BuilderApplicationService(IUnitOfWork uow, ILogger<BuilderApplicationService> logger, ICacheService<long, BuilderApplication> buildAppCache) : IBuilderApplicationService
{
    public async Task<Result<long>> SubmitApplication(long userId,
        int age,
        string? nationality,
        List<string> houseBuildLinks,
        List<string> otherBuildLinks,
        string? additionalBuildingInformation,
        string whyJoinGreenfield,
        string? additionalComments)
    {
        var builderRepo = uow.Repository<IBuilderApplicationRepository>();
        
        uow.BeginTransaction();
        try
        {
            var applicationInsertResult = await builderRepo.InsertApplication(
                userId,
                age,
                nationality ?? string.Empty,
                additionalBuildingInformation,
                whyJoinGreenfield,
                additionalComments);

            if (!applicationInsertResult.IsSuccessful)
                return Result<long>.Failure(applicationInsertResult.ErrorMessage ?? "Failed to insert application.", applicationInsertResult.StatusCode);

            var application = applicationInsertResult.GetNonNullOrThrow();
            var applicationId = application.ApplicationId;
            var linkedImages = new List<BuilderAppImageLinkEntity>();

            if (houseBuildLinks.Count > 0)
            {
                foreach (var link in houseBuildLinks.Where(link => !string.IsNullOrWhiteSpace(link)))
                {
                    var imageResult = await builderRepo.InsertImage(applicationId, "House", link);
                    if (imageResult.IsSuccessful) linkedImages.Add(imageResult.GetNonNullOrThrow());
                    else logger.LogWarning("Failed to insert house image for builder application {ApplicationId} (link: {Link}). {Error}", applicationId, link, imageResult.ErrorMessage);
                }
            }

            if (otherBuildLinks.Count > 0)
            {
                foreach (var link in otherBuildLinks.Where(link => !string.IsNullOrWhiteSpace(link)))
                {
                    var imageResult = await builderRepo.InsertImage(applicationId, "Other", link);
                    if (imageResult.IsSuccessful) linkedImages.Add(imageResult.GetNonNullOrThrow());
                    else logger.LogWarning("Failed to insert other image for builder application {ApplicationId} (link: {Link}). {Error}", applicationId, link, imageResult.ErrorMessage);
                }
            }
            
            uow.CompleteAndCommit();
            
            var mapped = MapApplicationEntityToModel(userId, application, [], linkedImages);
            buildAppCache.SetValue(applicationId, mapped);
            
            return Result<long>.Success(applicationId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected failure while submitting builder application for user {UserId}.", userId);
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

    public async Task<Result<List<ApplicationLatestStatus>>> GetApplicationsFromUser(long userId)
    {
        var builderRepo = uow.Repository<IBuilderApplicationRepository>();
        var latestResult = await builderRepo.GetApplicationsWithLatestStatusByUser(userId);
        if (!latestResult.IsSuccessful)
            return Result<List<ApplicationLatestStatus>>.Failure(latestResult.ErrorMessage ?? "Failed to retrieve user applications with latest status.", latestResult.StatusCode);

        var rows = latestResult.GetNonNullOrThrow().ToList();
        var results = rows
            .Select(r => new ApplicationLatestStatus
            {
                ApplicationId = r.ApplicationId,
                LatestStatus = r.Status is not null && r.CreatedOn is not null
                    ? new BuildAppStatus(r.Status, r.StatusMessage, r.CreatedOn.Value)
                    : null
            })
            .ToList();

        return Result<List<ApplicationLatestStatus>>.Success(results);
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
        
        var applicationModel = MapApplicationEntityToModel(applicationEntity.UserId, applicationEntity, statuses, images);
        buildAppCache.SetValue(applicationId, applicationModel);
        return Result<BuilderApplication>.Success(applicationModel);
    }

    private BuilderApplication MapApplicationEntityToModel(long userId, 
        BuilderApplicationEntity entity,
        List<BuilderAppStatusEntity> statuses,
        List<BuilderAppImageLinkEntity> images)
    {
        return new BuilderApplication
        {
            ApplicationId = entity.ApplicationId,
            UserId = userId,
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
                .Select(img => new BuildAppImage(img.ImageLink, img.LinkType, img.CreatedOn))
                .ToList(),
            OtherBuilds = images
                .Where(img => string.Equals(img.LinkType, "Other", StringComparison.OrdinalIgnoreCase))
                .Select(img => new BuildAppImage(img.ImageLink, img.LinkType, img.CreatedOn))
                .ToList()
        };
    }
}
