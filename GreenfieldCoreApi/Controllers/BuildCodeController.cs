using Asp.Versioning;
using GreenfieldCoreApi.ApiModels;
using GreenfieldCoreServices.Models.BuildCodes;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[Authorize(Roles = "BuildCodes")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class BuildCodeController(IBuildCodeService buildCodeService) : ControllerBase
{
    [HttpGet("all")]
    [Authorize(Roles = "BuildCodes.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(IEnumerable<BuildCode>))]
    public async Task<IActionResult> GetAll([FromQuery] bool showDeleted = false)
    {
        var codes = await buildCodeService.GetAllBuildCodes(showDeleted);
        return Ok(codes);
    }

    [HttpGet("{id:long}")]
    [Authorize(Roles = "BuildCodes.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(BuildCode))]
    public async Task<IActionResult> GetById([FromRoute] long id)
    {
        var code = await buildCodeService.GetBuildCodeById(id);
        return code is null 
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "Build code not found")
            : Ok(code);
    }

    [HttpPost]
    [Authorize(Roles = "BuildCodes.Write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Produces(typeof(BuildCode))]
    public async Task<IActionResult> Create([FromBody] BuildCodeRequest request)
    {
        var createdResult = await buildCodeService.CreateBuildCode(request.ListOrder, request.Code);
        if (!createdResult.IsSuccessful)
            return Problem(statusCode: createdResult.GetStatusCodeInt(), detail: createdResult.ErrorMessage);
        var created = createdResult.GetNonNullOrThrow();
        return CreatedAtAction(nameof(GetById), new { version = HttpContext.GetRequestedApiVersion()?.ToString(), id = created.BuildCodeId }, created);
    }

    [HttpPatch("{id:long}")]
    [Authorize(Roles = "BuildCodes.Write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(BuildCode))]
    public async Task<IActionResult> Update([FromRoute] long id, int? listOrder = null, string? code = null)
    {
        var updatedResult = await buildCodeService.UpdateBuildCode(id, listOrder, code);
        if (!updatedResult.IsSuccessful)
            return Problem(statusCode: updatedResult.GetStatusCodeInt(), detail: updatedResult.ErrorMessage);
        var updated = updatedResult.GetNonNullOrThrow();
        return Ok(updated);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "BuildCodes.Write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(BuildCode))]
    public async Task<IActionResult> Delete([FromRoute] long id)
    {
        var deletedResult = await buildCodeService.DeleteBuildCode(id);
        if (!deletedResult.IsSuccessful)
            return Problem(statusCode: deletedResult.GetStatusCodeInt(), detail: deletedResult.ErrorMessage);
        var deleted = deletedResult.GetNonNullOrThrow();
        return Ok(deleted);
    }
}