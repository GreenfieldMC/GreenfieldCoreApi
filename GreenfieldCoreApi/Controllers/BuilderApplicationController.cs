using Asp.Versioning;
using GreenfieldCoreApi.ApiModels;
using GreenfieldCoreServices.Models.BuildApps;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Authorize(Roles = "BuilderApplications")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class BuilderApplicationController(IBuilderApplicationService buildAppService) : ControllerBase
{
    
    [HttpPost("submit")]
    [Authorize(Roles = "BuilderApplications.Submit")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(long))]
    public async Task<IActionResult> SubmitApplication([FromBody] BuilderApplicationSubmitModel application)
    {
        var appIdResult = await buildAppService.SubmitApplication(application.DiscordId,
            application.MinecraftUsername,
            application.MinecraftUuid,
            application.Age,
            application.Nationality,
            application.HouseBuildLinks,
            application.OtherBuildLinks,
            application.AdditionalBuildingInformation,
            application.WhyJoinGreenfield,
            application.AdditionalComments);
        
        var appId = appIdResult.GetNonNullOrThrow();
        
        return appIdResult.IsSuccessful
            ? CreatedAtAction(nameof(SubmitApplication), new { version = HttpContext.GetRequestedApiVersion()?.ToString(), id = appId }, appId)
            : Problem(statusCode: appIdResult.GetStatusCodeInt(), detail: appIdResult.ErrorMessage);
    }

    [HttpGet("applications/{userId:long}")]
    [Authorize(Roles = "BuilderApplications.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(IEnumerable<ApplicationLatestStatus>))]
    public async Task<IActionResult> GetApplicationsFromUser(long userId)
    {
        var appsResult = await buildAppService.GetApplicationsFromUser(userId);
        return appsResult.IsSuccessful
            ? Ok(appsResult.GetNonNullOrThrow())
            : Problem(statusCode: appsResult.GetStatusCodeInt(), detail: appsResult.ErrorMessage);
    }
    
    [HttpGet("application/{applicationId:long}")]
    [Authorize(Roles = "BuilderApplications.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(BuilderApplication))]
    public async Task<IActionResult> GetApplicationById(long applicationId)
    {
        var appResult = await buildAppService.GetApplicationById(applicationId);
        return appResult.IsSuccessful
            ? Ok(appResult.GetNonNullOrThrow())
            : Problem(statusCode: appResult.GetStatusCodeInt(), detail: appResult.ErrorMessage);
    }
    
    [HttpPost("application/{applicationId:long}/status/add")]
    [Authorize(Roles = "BuilderApplications.Write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(bool))]
    public async Task<IActionResult> AddApplicationStatus(long applicationId, [FromBody] BuilderApplicationAddStatusModel statusModel)
    {
        var statusResult = await buildAppService.AddApplicationStatus(applicationId, statusModel.Status, statusModel.StatusMessage);
        return statusResult.IsSuccessful
            ? Ok(statusResult.GetNonNullOrThrow())
            : Problem(statusCode: statusResult.GetStatusCodeInt(), detail: statusResult.ErrorMessage);
    }
    
}