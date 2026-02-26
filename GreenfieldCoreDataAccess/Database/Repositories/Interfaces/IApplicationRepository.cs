using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IApplicationRepository
{
    Task<Result<IEnumerable<ApplicationEntity>>> SelectApplicationsByUserId(long userId);
    Task<Result<IEnumerable<ApplicationImageLinkEntity>>> SelectApplicationImages(long applicationId);
    Task<Result<IEnumerable<ApplicationStatusEntity>>> SelectApplicationStatuses(long applicationId);
    Task<Result<IEnumerable<LatestApplicationStatusEntity>>> SelectApplicationsWithLatestStatusByUser(long userId);
    Task<Result<ApplicationEntity>> InsertApplication(long userId, int userAge, string? userNationality, string? additionalBuildingInformation, string whyJoinGreenfield, string? additionalComments);
    Task<Result<ApplicationStatusEntity>> InsertStatus(long applicationId, string status, string? statusMessage);
    Task<Result<ApplicationImageLinkEntity>> InsertImage(long applicationId, string linkType, string imageLink);
    Task<Result<ApplicationImageLinkEntity>> UpdateImage(long imageLinkId, string linkType, string imageLink);
    Task<Result<ApplicationEntity>> SelectApplication(long applicationId);
}