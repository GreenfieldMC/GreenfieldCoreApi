using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Models.BuildApps;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface IBuilderApplicationService
{

    //this will submit the application to the database and mark it as 
    Task<Result<long>> SubmitApplication(ulong discordSnowflake, string minecraftUsername, Guid minecraftUuid, int age,
        string? nationality, List<string> houseBuildLinks, List<string> otherBuildLinks,
        string? additionalBuildingInformation, string whyJoinGreenfield, string? additionalComments);

    Task<Result<bool>> AddApplicationStatus(long applicationId, string status, string? statusMessage);
    
    Task<Result<BuilderApplication>> GetApplicationById(long applicationId);
    
    Task<Result<List<BuilderApplication>>> GetApplicationsFromUser(long userId);

}