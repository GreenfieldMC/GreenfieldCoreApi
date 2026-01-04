using Asp.Versioning;
using GreenfieldCoreApi.ApiModels;
using GreenfieldCoreApi.ApiModels.Connections;
using GreenfieldCoreServices.Models.Connections.Discord;
using GreenfieldCoreServices.Models.Connections.Patreon;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[Authorize(Roles = "Users")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class UserController(IUserService userService, IPatreonService patreonService, IDiscordService discordService) : ControllerBase
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

    [HttpDelete("{userId:long}/accounts/discord/{discordConnectionId:long}")]
    [Authorize(Roles = "Users.Write.Discord")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkDiscordAccount([FromRoute] long userId, [FromRoute] long discordConnectionId)
    {
        var unlinkResult = await discordService.UnlinkUserDiscordConnection(userId, discordConnectionId);
        return unlinkResult.IsSuccessful
            ? Ok()
            : Problem(statusCode: unlinkResult.GetStatusCodeInt(), detail: unlinkResult.ErrorMessage);
    }
    
    [HttpGet("{userId:long}/accounts/discord")]
    [Authorize(Roles = "Users.Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(IEnumerable<ApiDiscordAccount>))]
    public async Task<IActionResult> GetDiscordAccountsByUserId([FromRoute] long userId)
    {
        var userDiscordConnectionsResult = await discordService.GetUserDiscordConnections(userId);;
        if (!userDiscordConnectionsResult.TryGetDataNonNull(out var userDiscordConnectionsEnum))
            return Problem(statusCode: userDiscordConnectionsResult.GetStatusCodeInt(), detail: userDiscordConnectionsResult.ErrorMessage);
        
        var userDiscordConnections = userDiscordConnectionsEnum.ToList();
        var connections = new Dictionary<long, DiscordConnection>();
        foreach (var userDiscordConnection in userDiscordConnections)
        {
            var connectionResult = await discordService.GetDiscordConnection(userDiscordConnection.DiscordConnectionId);
            if (!connectionResult.TryGetDataNonNull(out var connection))
                return Problem(statusCode: connectionResult.GetStatusCodeInt(), detail: connectionResult.ErrorMessage);
            
            connections[userDiscordConnection.DiscordConnectionId] = connection;
        }
        
        var apiModels = userDiscordConnections.Select(model => new ApiDiscordAccount(
            model.UserDiscordConnectionId,
            model.DiscordConnectionId,
            connections[model.DiscordConnectionId].DiscordSnowflake,
            connections[model.DiscordConnectionId].DiscordUsername,
            model.ConnectedOn,
            connections[model.DiscordConnectionId].UpdatedOn,
            connections[model.DiscordConnectionId].CreatedOn
        ));
        
        return Ok(apiModels);
    }

    [HttpDelete("{userId:long}/accounts/patreon/{patreonConnectionId:long}")]
    [Authorize(Roles = "Users.Write.Patreon")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkPatreonAccount([FromRoute] long userId, [FromRoute] long patreonConnectionId)
    {

        var unlinkResult = await patreonService.UnlinkUserPatreonConnection(userId, patreonConnectionId);
        return unlinkResult.IsSuccessful
            ? Ok()
            : Problem(statusCode: unlinkResult.GetStatusCodeInt(), detail: unlinkResult.ErrorMessage);
    }

    [HttpGet("{userId:long}/accounts/patreon")]
    [Authorize(Roles = "Users.Read.Patreon")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(IEnumerable<ApiPatreonAccount>))]
    public async Task<IActionResult> GetPatreonAccountsByUserId([FromRoute] long userId)
    {
        var userConnectionResult = await patreonService.GetUserPatreonConnections(userId);
        if (!userConnectionResult.TryGetDataNonNull(out var userPatreonConnectionsEnum))
            return Problem(statusCode: userConnectionResult.GetStatusCodeInt(), detail: userConnectionResult.ErrorMessage);

        var userPatreonConnections = userPatreonConnectionsEnum.ToList();
        var connections = new Dictionary<long, PatreonConnection>();
        foreach (var userPatreonConnection in userPatreonConnections)
        {
            var connectionResult = await patreonService.GetPatreonConnection(userPatreonConnection.PatreonConnectionId);
            if (!connectionResult.TryGetDataNonNull(out var connection))
                return Problem(statusCode: connectionResult.GetStatusCodeInt(), detail: connectionResult.ErrorMessage);
            
            connections[userPatreonConnection.PatreonConnectionId] = connection;
        }
        
        var apiModels = userPatreonConnections.Select(model => new ApiPatreonAccount(
            model.UserPatreonConnectionId,
            model.PatreonConnectionId,
            connections[model.PatreonConnectionId].FullName,
            connections[model.PatreonConnectionId].Pledge,
            model.ConnectedOn,
            connections[model.PatreonConnectionId].UpdatedOn,
            connections[model.PatreonConnectionId].CreatedOn
        ));
        
        return Ok(apiModels);
    }

}