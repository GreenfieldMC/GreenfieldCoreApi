using Asp.Versioning;
using GreenfieldCoreApi.ApiModels.Connections;
using GreenfieldCoreApi.Extensions;
using GreenfieldCoreServices.Models.Discord;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DiscordController(IConfiguration configuration, IDiscordService discordService, IUserService userService, IDiscordApi discordApi, ICacheService<long, DiscordConnectionState> cacheService, ICacheService<(long userId, long discordConnectionId), DiscordDisconnectState> disconnectCacheService) : ControllerBase
{
    /// <summary>
    /// Discord OAuth callback.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("oauth/callback")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConnectionCallback([FromQuery] Guid state, [FromQuery] string code)
    {
        if (!cacheService.TryGetValue(s => s.StateId == state, out var connectionState) || connectionState.Timestamp < DateTime.UtcNow.AddHours(-1))
        {
            cacheService.RemoveValues(s => s.StateId == state);
            return ResourceHelpers.Redirect(RedirectType.Error, "", "Your session state is invalid or has expired. Please retry linking your Discord account again.");
        }

        if (string.IsNullOrEmpty(code))
            return ResourceHelpers.Redirect(RedirectType.Error, "", "Invalid code parameter. Please retry linking your Discord account again.");

        var linkResult = await discordApi.LinkDiscordAccountToUser(connectionState.UserId, code);
        
        if (linkResult.IsSuccessful)
            cacheService.RemoveValues(s => s.StateId == state);

        return linkResult.IsSuccessful
            ? ResourceHelpers.Redirect(RedirectType.Info, connectionState.RedirectUrl, "Your Discord account has been successfully linked!", "You may close this tab.")
            : ResourceHelpers.Redirect(RedirectType.Error, "", $"Failed to link your Discord account: {linkResult.ErrorMessage}");
    }

    /// <summary>
    /// Discord OAuth disconnect callback.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("oauth/disconnect")]
    public async Task<IActionResult> Disconnect([FromQuery] Guid state)
    {
        if (!disconnectCacheService.TryGetValue(s => s.StateId == state, out var disconnectState) || disconnectState.Timestamp < DateTime.UtcNow.AddHours(-1))
        {
            disconnectCacheService.RemoveValues(s => s.StateId == state);
            return ResourceHelpers.Redirect(RedirectType.Error, "./", "Your session state is invalid or has expired. Please retry unlinking your Discord account again.");
        }

        var unlinkResult = await discordService.UnlinkUserDiscordConnection(disconnectState.UserId, disconnectState.DiscordConnectionId);
        
        if (unlinkResult.IsSuccessful)
            disconnectCacheService.RemoveValues(s => s.StateId == state);

        return unlinkResult.IsSuccessful
            ? ResourceHelpers.Redirect(RedirectType.Info, disconnectState.RedirectUrl, "Your Discord account has been successfully unlinked!", "You may close this tab.")
            : ResourceHelpers.Redirect(RedirectType.Error, "./", $"Failed to unlink your Discord account: {unlinkResult.ErrorMessage}");
    }
    
    /// <summary>
    /// Get the Discord disconnect link for a user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="discordConnectionId"></param>
    /// <param name="redirectUrl"></param>
    /// <returns></returns>
    [Authorize(Roles = "Discord.OAuth")]
    [HttpGet("oauth/disconnect-link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(string))]
    public Task<IActionResult> GetDiscordDisconnectLink([FromQuery] long userId, [FromQuery] long discordConnectionId, [FromQuery] string redirectUrl)
    {
        if (disconnectCacheService.TryGetValue((userId, discordConnectionId), out var existingState) && existingState.Timestamp >= DateTime.UtcNow.AddHours(-1) && existingState.RedirectUrl == redirectUrl)
            return Task.FromResult<IActionResult>(Ok($"{configuration["Discord:DisconnectUri"]}?state={Uri.EscapeDataString(existingState.StateId.ToString())}"));
        
        var newState = new DiscordDisconnectState(Guid.NewGuid(), DateTime.UtcNow, userId, discordConnectionId, redirectUrl);
        disconnectCacheService.SetValue((userId, discordConnectionId), newState);

        var state = Uri.EscapeDataString(newState.StateId.ToString());
        return Task.FromResult<IActionResult>(Ok($"{configuration["Discord:DisconnectUri"]}?state={state}"));
    }
    
    /// <summary>
    /// Get the Discord connection link for a user.
    /// </summary>
    /// <param name="userId">The Internal user ID</param>
    /// <param name="redirectUrl">The final redirect URL after Discord authorization and after the callback. Probably is the discord channel the user pressed the link button in.</param>
    [Authorize(Roles = "Discord.OAuth")]
    [HttpGet("oauth/connection-link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(string))]
    public Task<IActionResult> GetDiscordConnectionLink([FromQuery] long userId, [FromQuery] string redirectUrl)
    {
        var clientId = configuration["Discord:ClientId"] ?? throw new InvalidOperationException("Discord ClientId is not configured.");
        var redirectUri = Uri.EscapeDataString(configuration["Discord:RedirectUri"] ?? throw new InvalidOperationException("Discord RedirectUri is not configured."));
        var scopes = Uri.EscapeDataString("identify guilds");

        if (cacheService.TryGetValue(userId, out var existingState) && existingState.Timestamp >= DateTime.UtcNow.AddHours(-1) && existingState.RedirectUrl == redirectUrl)
            return Task.FromResult<IActionResult>(Ok($"https://discord.com/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&state={Uri.EscapeDataString(existingState.StateId.ToString())}&scope={scopes}"));

        var newState = new DiscordConnectionState(Guid.NewGuid(), DateTime.UtcNow, userId, redirectUrl);
        cacheService.SetValue(userId, newState);

        var state = Uri.EscapeDataString(newState.StateId.ToString());
        return Task.FromResult<IActionResult>(Ok($"https://discord.com/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&state={state}&scope={scopes}"));
    }

    [Authorize(Roles = "Discord.Read")]
    [HttpGet("connections/{discordConnectionId:long}")]
    [ProducesResponseType(typeof(ApiDiscordConnection), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiDiscordConnectionWithUsers), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDiscordConnectionByConnectionId(long discordConnectionId, [FromQuery] bool includeUsers = false)
    {
        var connectionResult = await discordService.GetDiscordConnection(discordConnectionId);
        if (!connectionResult.TryGetDataNonNull(out var connection))
            return Problem(statusCode: connectionResult.GetStatusCodeInt(), detail: connectionResult.ErrorMessage);
        var mapped = ApiDiscordConnection.FromModel(connection);

        if (!includeUsers) return Ok(mapped);
        
        var userConnectionsResult = await discordService.GetUsersByDiscordConnectionId(discordConnectionId);
        if (!userConnectionsResult.TryGetDataNonNull(out var userConnectionsEnum))
            return Problem(statusCode: userConnectionsResult.GetStatusCodeInt(), detail: userConnectionsResult.ErrorMessage);
            
        var userConnections = userConnectionsEnum.ToList();
        var userList = new List<User>();

        foreach (var uconn in userConnections)
        {
            var userResult = await userService.GetUserByUserId(uconn.UserId);
            if (userResult.TryGetDataNonNull(out var userModel))
                userList.Add(userModel);
        }
            
        var mappedWithUsers = new ApiDiscordConnectionWithUsers
        {
            DiscordConnectionId = mapped.DiscordConnectionId,
            DiscordSnowflake = mapped.DiscordSnowflake,
            Users = userList,
            DiscordUsername = mapped.DiscordUsername,
            UpdatedOn = mapped.UpdatedOn,
            CreatedOn = mapped.CreatedOn
        };
        return Ok(mappedWithUsers);
    }
    
    [Authorize(Roles = "Discord.Read")]
    [HttpGet("snowflakes/{discordSnowflake}")]
    [ProducesResponseType(typeof(ApiDiscordConnection), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDiscordConnectionBySnowflake(ulong discordSnowflake, [FromQuery] bool includeUsers = false)
    {
        var connectionResult = await discordService.GetDiscordConnectionBySnowflake(discordSnowflake);
        if (!connectionResult.TryGetDataNonNull(out var connection))
            return Problem(statusCode: connectionResult.GetStatusCodeInt(), detail: connectionResult.ErrorMessage);
        var mapped = ApiDiscordConnection.FromModel(connection);

        if (!includeUsers) return Ok(mapped);

        var userConnectionsResult = await discordService.GetUsersByDiscordConnectionId(connection.DiscordConnectionId);
        if (!userConnectionsResult.TryGetDataNonNull(out var userConnectionsEnum))
            return Problem(statusCode: userConnectionsResult.GetStatusCodeInt(), detail: userConnectionsResult.ErrorMessage);

        var userConnections = userConnectionsEnum.ToList();
        var userList = new List<User>();

        foreach (var uconn in userConnections)
        {
            var userResult = await userService.GetUserByUserId(uconn.UserId);
            if (userResult.TryGetDataNonNull(out var userModel))
                userList.Add(userModel);
        }

        var mappedWithUsers = new ApiDiscordConnectionWithUsers
        {
            DiscordConnectionId = mapped.DiscordConnectionId,
            DiscordSnowflake = mapped.DiscordSnowflake,
            Users = userList,
            DiscordUsername = mapped.DiscordUsername,
            UpdatedOn = mapped.UpdatedOn,
            CreatedOn = mapped.CreatedOn
        };
        return Ok(mappedWithUsers);
    }
    
}
