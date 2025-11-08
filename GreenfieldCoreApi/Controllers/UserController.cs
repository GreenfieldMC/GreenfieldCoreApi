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
        var user = await userService.GetUserByUuid(minecraftUuid);
        return user is null 
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "User not found") 
            : Ok(user);
    }
    
    [HttpGet("userByUserId")]
    [Authorize(Roles = "Users.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> GetUserByUserId([FromQuery] long userId)
    {
        var user = await userService.GetUserByUserId(userId);
        return user is null 
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "User not found") 
            : Ok(user);
    }
    
    [HttpPatch("updateUsername")]
    [Authorize(Roles = "Users.Write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> UpdateUsername([FromBody] UserRequest request) 
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "A valid minecraftUuid and username are required");
        var updatedUser = await userService.UpdateUsername(request.MinecraftUuid, request.Username);
        return updatedUser is null 
            ? Problem(statusCode: StatusCodes.Status404NotFound, detail: "User not found or username not updated") 
            : Ok(updatedUser);
    }

    [HttpPost("createUser")]
    [Authorize(Roles = "Users.Write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> CreateUser([FromBody] UserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "A valid minecraftUuid and username are required");
        var created = await userService.CreateUser(request.MinecraftUuid, request.Username);
        
        return created is null 
            ? Problem(statusCode: StatusCodes.Status409Conflict, detail: "User already exists or could not be created") 
            : CreatedAtAction(nameof(GetUserByUuid), new { version = HttpContext.GetRequestedApiVersion()?.ToString(), minecraftUuid = created.MinecraftUuid }, created);
    }
    
}