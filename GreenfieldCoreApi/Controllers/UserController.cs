using Asp.Versioning;
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
    public async Task<IActionResult> GetUserByUuid([FromQuery] Guid minecraftUuid)
    {
        var user = await userService.GetUserByUuid(minecraftUuid);
        if (user is null)
            return NotFound("User not found");
        return Ok(user);
    }
    
    [HttpGet("userByUserId")]
    [Authorize(Roles = "Users.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByUserId([FromQuery] long userId)
    {
        var user = await userService.GetUserByUserId(userId);
        if (user is null)
            return NotFound("User not found");
        return Ok(user);
    }
    
    [HttpPatch("updateUsername")]
    [Authorize(Roles = "Users.Write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUsername([FromBody] Guid minecraftUuid, string newUsername) 
    {
        var updatedUser = await userService.UpdateUsername(minecraftUuid, newUsername);
        if (updatedUser is null)
            return NotFound("User not found or username not updated");
        return Ok(updatedUser);
    }

    [HttpPost("createOrGetUser")]
    [Authorize(Roles = "Users.Write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOrGetUser([FromBody] Guid minecraftUuid, string username)
    {
        var user = await userService.CreateOrGetUser(minecraftUuid, username);
        return Ok(user);
    }
    
}