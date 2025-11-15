using Asp.Versioning;
using GreenfieldCoreApi.ApiModels;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[Authorize(Roles = "Users")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class UserController(IUserService userService) : ControllerBase
{
    
    [HttpGet("userByUuid")]
    [Authorize(Roles = "Users.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> GetUserByUuid([FromQuery] Guid minecraftUuid)
    {
        var userResult = await userService.GetUserByUuid(minecraftUuid);
        if (!userResult.IsSuccessful)
            return Problem(statusCode: userResult.GetStatusCodeInt(), detail: userResult.ErrorMessage);
        var user = userResult.GetNonNullOrThrow();
        return Ok(user);
    }
    
    [HttpGet("userByUserId")]
    [Authorize(Roles = "Users.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> GetUserByUserId([FromQuery] long userId)
    {
        var userResult = await userService.GetUserByUserId(userId);
        if (!userResult.IsSuccessful)
            return Problem(statusCode: userResult.GetStatusCodeInt(), detail: userResult.ErrorMessage);
        var user = userResult.GetNonNullOrThrow();
        return Ok(user);
    }
    
    [HttpPatch("updateUsername")]
    [Authorize(Roles = "Users.Write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> UpdateUsername([FromBody] UserRequest request) 
    {
        var updateUserResult = await userService.UpdateUsername(request.MinecraftUuid, request.Username);
        if (!updateUserResult.IsSuccessful)
            return Problem(statusCode: updateUserResult.GetStatusCodeInt(), detail: updateUserResult.ErrorMessage);
        var updatedUser = updateUserResult.GetNonNullOrThrow();
        return Ok(updatedUser);
    }

    [HttpPost("createUser")]
    [Authorize(Roles = "Users.Write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> CreateUser([FromBody] UserRequest request)
    {
        var createdUserResult = await userService.CreateUser(request.MinecraftUuid, request.Username);
        if (!createdUserResult.IsSuccessful)
            return Problem(statusCode: createdUserResult.GetStatusCodeInt(), detail: createdUserResult.ErrorMessage);
        var created = createdUserResult.GetNonNullOrThrow();
        return CreatedAtAction(nameof(GetUserByUuid), new { version = HttpContext.GetRequestedApiVersion()?.ToString(), minecraftUuid = created.MinecraftUuid }, created);
    }
    
}