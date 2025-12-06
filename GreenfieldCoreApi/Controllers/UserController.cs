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
    
    [HttpGet("{minecraftUuid:guid}/userinfo")]
    [Authorize(Roles = "Users.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> GetUserByUuid([FromRoute] Guid minecraftUuid)
    {
        var userResult = await userService.GetUserByUuid(minecraftUuid);
        return userResult.IsSuccessful
            ? Ok(userResult.GetNonNullOrThrow())
            : Problem(statusCode: userResult.GetStatusCodeInt(), detail: userResult.ErrorMessage);
    }
    
    [HttpGet("{userId:long}/userinfo")]
    [Authorize(Roles = "Users.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> GetUserByUserId([FromRoute] long userId)
    {
        var userResult = await userService.GetUserByUserId(userId);
        return userResult.IsSuccessful
            ? Ok(userResult.GetNonNullOrThrow())
            : Problem(statusCode: userResult.GetStatusCodeInt(), detail: userResult.ErrorMessage);
    }
    
    [HttpPatch("{minecraftUuid:guid}/userinfo/username/{username}")]
    [Authorize(Roles = "Users.Write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> UpdateUsername([FromRoute] Guid minecraftUuid, [FromRoute] string username) 
    {
        var updateUserResult = await userService.UpdateUsername(minecraftUuid, username);
        return updateUserResult.IsSuccessful
            ? Ok(updateUserResult.GetNonNullOrThrow())
            : Problem(statusCode: updateUserResult.GetStatusCodeInt(), detail: updateUserResult.ErrorMessage);
    }

    [HttpPut("{minecraftGuid:guid}/userinfo/username/{username}")]
    [Authorize(Roles = "Users.Write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Produces(typeof(GreenfieldCoreServices.Models.Users.User))]
    public async Task<IActionResult> CreateUser([FromRoute] Guid minecraftGuid, [FromRoute] string username)
    {
        var createdUserResult = await userService.CreateUser(minecraftGuid, username);
        if (!createdUserResult.IsSuccessful)
            return Problem(statusCode: createdUserResult.GetStatusCodeInt(), detail: createdUserResult.ErrorMessage);
        var created = createdUserResult.GetNonNullOrThrow();
        return CreatedAtAction(nameof(GetUserByUuid), new { version = HttpContext.GetRequestedApiVersion()?.ToString(), minecraftUuid = created.MinecraftUuid }, created);
    }
    
    [HttpGet("{userId:long}/discord")]
    [Authorize(Roles = "Users.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(IEnumerable<ulong>))]
    public async Task<IActionResult> GetLinkedDiscordAccountsByUserId([FromRoute] long userId)
    {
        var linkedResult = await userService.GetLinkedDiscordAccounts(userId);
        return linkedResult.IsSuccessful
            ? Ok(linkedResult.GetNonNullOrThrow())
            : Problem(statusCode: linkedResult.GetStatusCodeInt(), detail: linkedResult.ErrorMessage);
    }

    [HttpGet("{minecraftUuid:guid}/discord")]
    [Authorize(Roles = "Users.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(IEnumerable<ulong>))]
    public async Task<IActionResult> GetLinkedDiscordAccountsByUuid([FromRoute] Guid minecraftUuid)
    {
        var linkedResult = await userService.GetLinkedDiscordAccountsByUuid(minecraftUuid);
        return linkedResult.IsSuccessful
            ? Ok(linkedResult.GetNonNullOrThrow())
            : Problem(statusCode: linkedResult.GetStatusCodeInt(), detail: linkedResult.ErrorMessage);
    }

    [HttpPut("{userId:long}/discord/{discordSnowflake:long}")]
    [Authorize(Roles = "Users.Write")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LinkDiscordAccount([FromRoute] long userId, [FromRoute] long discordSnowflake)
    {
        if (discordSnowflake <= 0)
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "A valid discordSnowflake must be provided.");
        var linkResult = await userService.LinkDiscordAccount(userId, (ulong)discordSnowflake);
        return !linkResult.IsSuccessful 
            ? Problem(statusCode: linkResult.GetStatusCodeInt(), detail: linkResult.ErrorMessage) 
            : CreatedAtAction(nameof(GetLinkedDiscordAccountsByUserId), new { version = HttpContext.GetRequestedApiVersion()?.ToString(), userId }, true);
    }

    [HttpDelete("{userId:long}/discord/{discordSnowflake:long}")]
    [Authorize(Roles = "Users.Write")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkDiscordAccount([FromRoute] long userId, [FromRoute] long discordSnowflake)
    {
        if (discordSnowflake <= 0)
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "A valid discordSnowflake must be provided.");
        var unlinkResult = await userService.UnlinkDiscordAccount(userId, (ulong)discordSnowflake);
        return unlinkResult.IsSuccessful
            ? Ok(true)
            : Problem(statusCode: unlinkResult.GetStatusCodeInt(), detail: unlinkResult.ErrorMessage);
    }

}