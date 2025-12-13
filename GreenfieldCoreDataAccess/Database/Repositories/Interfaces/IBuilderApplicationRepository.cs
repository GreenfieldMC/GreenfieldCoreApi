using GreenfieldCoreDataAccess.Database.Models;
using GreenfieldCoreDataAccess.Database.UnitOfWork;

namespace GreenfieldCoreDataAccess.Database.Repositories.Interfaces;

public interface IBuilderApplicationRepository
{
    Task<Result<IEnumerable<BuilderApplicationEntity>>> GetApplicationsByUser(long userId);
    Task<Result<IEnumerable<BuilderAppImageLinkEntity>>> GetApplicationImages(long applicationId);
    Task<Result<IEnumerable<BuilderAppStatusEntity>>> GetStatusesByApplication(long applicationId);
    Task<Result<BuilderApplicationEntity>> InsertApplication(long userId, int userAge, string? userNationality, string? additionalBuildingInformation, string whyJoinGreenfield, string? additionalComments);
    Task<Result<bool>> InsertStatus(long applicationId, string status, string? statusMessage);
    Task<Result<bool>> InsertImage(long applicationId, string linkType, string imageLink);
    Task<Result<BuilderApplicationEntity>> GetApplicationById(long applicationId);
}