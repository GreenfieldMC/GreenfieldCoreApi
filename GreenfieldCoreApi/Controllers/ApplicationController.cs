using Asp.Versioning;
using GreenfieldCoreApi.ApiModels;
using GreenfieldCoreServices.Models.BuildApps;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class ApplicationController(IBuilderApplicationService buildAppService) : ControllerBase
{
    
    [HttpPost("submit")]
    [Authorize(Roles = "Applications.Write,Applications")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(long))]
    public async Task<IActionResult> SubmitApplication([FromBody] ApiApplicationSubmissionModel application)
    {
        var appIdResult = await buildAppService.SubmitApplication(application.UserId,
            application.Age,
            application.Nationality,
            application.Images.Select(i => (i.ImageLink, i.ImageType)).ToList(),
            application.AdditionalBuildingInformation,
            application.WhyJoinGreenfield,
            application.AdditionalComments);
        
        var appId = appIdResult.GetNonNullOrThrow();
        
        return appIdResult.IsSuccessful
            ? CreatedAtAction(nameof(SubmitApplication), new { version = HttpContext.GetRequestedApiVersion()?.ToString(), id = appId }, appId)
            : Problem(statusCode: appIdResult.GetStatusCodeInt(), detail: appIdResult.ErrorMessage);
    }

    [HttpPut("images/{imageLinkId:long}")]
    [Authorize(Roles = "Applications.Write,Applications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateApplicationImage(long imageLinkId, [FromBody] ApiApplicationImageModel updatedImageDetails)
    {
        var updateResult = await buildAppService.UpdateApplicationImage(imageLinkId, updatedImageDetails.ImageLink, updatedImageDetails.ImageType);
        return updateResult.IsSuccessful
            ? Ok()
            : Problem(statusCode: updateResult.GetStatusCodeInt(), detail: updateResult.ErrorMessage);
    }
    
    [HttpGet("{applicationId:long}")]
    [Authorize(Roles = "Applications.Read,Applications")]
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
    
    [HttpPost("{applicationId:long}/status")]
    [Authorize(Roles = "Applications.Write,Applications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(BuildAppStatus))]
    public async Task<IActionResult> AddApplicationStatus(long applicationId, [FromBody] ApiAddApplicationStatusModel statusModel)
    {
        var statusResult = await buildAppService.AddApplicationStatus(applicationId, statusModel.Status, statusModel.StatusMessage);
        return statusResult.IsSuccessful
            ? Ok(statusResult.GetNonNullOrThrow())
            : Problem(statusCode: statusResult.GetStatusCodeInt(), detail: statusResult.ErrorMessage);
    }
    
}