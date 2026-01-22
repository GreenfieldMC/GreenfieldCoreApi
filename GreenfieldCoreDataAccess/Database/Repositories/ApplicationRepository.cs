using System.Data.Common;
using System.Net;
using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.Procedures;
using GreenfieldCoreDataAccess.Database.Repositories.Interfaces;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories;

public class ApplicationRepository(IUnitOfWork unitOfWork) : BaseRepository(unitOfWork), IApplicationRepository
{

    public async Task<Result<IEnumerable<ApplicationEntity>>> SelectApplicationsByUserId(long userId)
    {
        try
        {
            var applications = await Connection.QueryProcedure(StoredProcs.BuildApps.SelectApplicationsByUser, userId, Transaction);
            return Result<IEnumerable<ApplicationEntity>>.Success(applications);
        }
        catch (DbException ex)
        {
            return Result<IEnumerable<ApplicationEntity>>.Failure($"Failed to retrieve builder applications: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<ApplicationImageLinkEntity>>> SelectApplicationImages(long applicationId)
    {
        try
        {
            var images = await Connection.QueryProcedure(StoredProcs.BuildApps.SelectApplicationImages, applicationId, Transaction);
            return Result<IEnumerable<ApplicationImageLinkEntity>>.Success(images);
        }
        catch (DbException ex)
        {
            return Result<IEnumerable<ApplicationImageLinkEntity>>.Failure($"Failed to retrieve builder application images: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<ApplicationStatusEntity>>> SelectApplicationStatuses(long applicationId)
    {
        try
        {
            var statuses = await Connection.QueryProcedure(StoredProcs.BuildApps.SelectApplicationStatuses, applicationId, Transaction);
            return Result<IEnumerable<ApplicationStatusEntity>>.Success(statuses);
        }
        catch (DbException ex)
        {
            return Result<IEnumerable<ApplicationStatusEntity>>.Failure($"Failed to retrieve builder application statuses: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<IEnumerable<LatestApplicationStatusEntity>>> SelectApplicationsWithLatestStatusByUser(long userId)
    {
        try
        {
            var applications = await Connection.QueryProcedure(StoredProcs.BuildApps.SelectApplicationsWithLatestStatusByUser, userId, Transaction);
            return Result<IEnumerable<LatestApplicationStatusEntity>>.Success(applications);
        }
        catch (DbException ex)
        {
            return Result<IEnumerable<LatestApplicationStatusEntity>>.Failure($"Failed to retrieve builder applications with latest status: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<ApplicationEntity>> InsertApplication(long userId, int userAge, string? userNationality, string? additionalBuildingInformation, string whyJoinGreenfield, string? additionalComments)
    {
        try
        {
            var application = await Connection.QuerySingleProcedure(StoredProcs.BuildApps.InsertApplication, (userId, userAge, userNationality, additionalBuildingInformation, whyJoinGreenfield, additionalComments), Transaction);
            return application is null 
                ? Result<ApplicationEntity>.Failure("Failed to insert builder application: No application returned from database.", HttpStatusCode.InternalServerError)
                : Result<ApplicationEntity>.Success(application);
        }
        catch (DbException ex)
        {
            return Result<ApplicationEntity>.Failure($"Failed to insert builder application: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<ApplicationStatusEntity>> InsertStatus(long applicationId, string status, string? statusMessage)
    {
        try
        {
            var statusRow = await Connection.QuerySingleProcedure(StoredProcs.BuildApps.InsertApplicationStatus, (applicationId, status, statusMessage), Transaction);
            return statusRow is null
                ? Result<ApplicationStatusEntity>.Failure("Failed to insert builder application status: No status returned from database.", HttpStatusCode.InternalServerError)
                : Result<ApplicationStatusEntity>.Success(statusRow);
        }
        catch (DbException ex)
        {
            return Result<ApplicationStatusEntity>.Failure($"Failed to insert builder application status: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<ApplicationImageLinkEntity>> InsertImage(long applicationId, string linkType, string imageLink)
    {
        try
        {
            var imageRow = await Connection.QuerySingleProcedure(StoredProcs.BuildApps.InsertImageLink, (applicationId, linkType, imageLink), Transaction);
            return imageRow is null
                ? Result<ApplicationImageLinkEntity>.Failure("Failed to insert builder application image: No image returned from database.", HttpStatusCode.InternalServerError)
                : Result<ApplicationImageLinkEntity>.Success(imageRow);
        }
        catch (DbException ex)
        {
            return Result<ApplicationImageLinkEntity>.Failure($"Failed to insert builder application image: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<ApplicationImageLinkEntity>> UpdateImage(long imageLinkId, string linkType, string imageLink)
    {
        try
        {
            var imageRow = await Connection.QuerySingleProcedure(StoredProcs.BuildApps.UpdateImageLink, (imageLinkId, linkType, imageLink), Transaction);
            return imageRow is null
                ? Result<ApplicationImageLinkEntity>.Failure("Failed to update builder application image: No image returned from database.", HttpStatusCode.InternalServerError)
                : Result<ApplicationImageLinkEntity>.Success(imageRow);
        }
        catch (DbException ex)
        {
            return Result<ApplicationImageLinkEntity>.Failure($"Failed to update builder application image: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<Result<ApplicationEntity>> SelectApplication(long applicationId)
    {
        try
        {
            var application = await Connection.QuerySingleProcedure(StoredProcs.BuildApps.SelectApplicationById, applicationId, Transaction);
            return application is null
                ? Result<ApplicationEntity>.Failure("Builder application not found.", HttpStatusCode.NotFound)
                : Result<ApplicationEntity>.Success(application);
        }
        catch (DbException ex)
        {
            return Result<ApplicationEntity>.Failure($"Failed to retrieve builder application: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
