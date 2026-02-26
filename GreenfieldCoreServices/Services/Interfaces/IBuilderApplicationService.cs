using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.BuildApps;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IBuilderApplicationService
{

    //this will submit the application to the database and mark it as 
    Task<Result<long>> SubmitApplication(long userId, int age,
        string? nationality, List<(string Link, string ImageType)> images,
        string? additionalBuildingInformation, string whyJoinGreenfield, string? additionalComments);

    Task<Result<BuildAppStatus>> AddApplicationStatus(long applicationId, string status, string? statusMessage);
    
    Task<Result> UpdateApplicationImage(long imageLinkId, string newImageLink, string newImageType);
    
    Task<Result<BuilderApplication>> GetApplicationById(long applicationId);
    
    Task<Result<List<ApplicationLatestStatus>>> GetApplicationsFromUser(long userId);

}