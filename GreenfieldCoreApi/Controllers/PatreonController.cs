using Asp.Versioning;
using GreenfieldCoreApi.ApiModels;
using GreenfieldCoreApi.ApiModels.Connections;
using GreenfieldCoreApi.Extensions;
using GreenfieldCoreServices.Models.Patreon;
using GreenfieldCoreServices.Models.Users;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PatreonController(IConfiguration configuration, IPatreonService patreonService, IUserService userService, IPatreonApi patreonApi, ICacheService<long, PatreonConnectionState> cacheService, ICacheService<(long userId, long patreonConnectionId), PatreonDisconnectState> disconnectStateCache) : ControllerBase
{
 
    /// <summary>
    /// This endpoint is called by Patreon after a user authorizes the application.
    /// </summary>
    /// <param name="state">Contains the user id of the user who is adding a Patreon connection</param>
    /// <param name="code">Contains the authorization code returned by Patreon</param>
    /// <returns></returns>
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
            return ResourceHelpers.Redirect(RedirectType.Error, "", "Your session state is invalid or has expired. Please retry linking your Patreon account again.");
        }
        
        if (string.IsNullOrEmpty(code))
            return ResourceHelpers.Redirect(RedirectType.Error, "", "Invalid code parameter. Please retry linking your Patreon account again.");
        
        var linkResult = await patreonApi.LinkPatreonAccountToUser(connectionState.UserId, code);
        
        if (linkResult.IsSuccessful)
            cacheService.RemoveValues(s => s.StateId == state);
        
        return linkResult.IsSuccessful
            ? ResourceHelpers.Redirect(RedirectType.Info, connectionState.RedirectUrl, "Your Patreon account has been successfully linked!", "You may close this tab.")
            : ResourceHelpers.Redirect(RedirectType.Error, "", $"Failed to link your Patreon account: {linkResult.ErrorMessage}");
    }
    
    /// <summary>
    /// Patreon OAuth disconnect callback.
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("oauth/disconnect")]
    public async Task<IActionResult> Disconnect([FromQuery] Guid state)
    {
        if (!disconnectStateCache.TryGetValue(s => s.StateId == state, out var disconnectState) || disconnectState.Timestamp < DateTime.UtcNow.AddHours(-1))
        {
            disconnectStateCache.RemoveValues(s => s.StateId == state);
            return ResourceHelpers.Redirect(RedirectType.Error, "./", "Your session state is invalid or has expired. Please retry unlinking your Patreon account again.");
        }

        var unlinkResult = await patreonService.UnlinkUserPatreonConnection(disconnectState.UserId, disconnectState.PatreonConnectionId);
        
        if (unlinkResult.IsSuccessful)
            disconnectStateCache.RemoveValues(s => s.StateId == state);

        return unlinkResult.IsSuccessful
            ? ResourceHelpers.Redirect(RedirectType.Info, disconnectState.RedirectUrl, "Your Patreon account has been successfully unlinked!", "You may close this tab.")
            : ResourceHelpers.Redirect(RedirectType.Error, "./", $"Failed to unlink your Patreon account: {unlinkResult.ErrorMessage}");
    }
    
    /// <summary>
    /// Get the Patreon disconnect link for a user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="patreonConnectionId"></param>
    /// <param name="redirectUrl"></param>
    /// <returns></returns>
    [Authorize(Roles = "Patreon.OAuth")]
    [HttpGet("oauth/disconnect-link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces(typeof(string))]
    public Task<IActionResult> GetPatreonDisconnectLink([FromQuery] long userId, [FromQuery] long patreonConnectionId, [FromQuery] string redirectUrl)
    {
        if (disconnectStateCache.TryGetValue((userId, patreonConnectionId), out var existingState) && existingState.Timestamp >= DateTime.UtcNow.AddHours(-1) && existingState.RedirectUrl == redirectUrl)
            return Task.FromResult<IActionResult>(Ok($"{configuration["Patreon:DisconnectUri"]}?state={Uri.EscapeDataString(existingState.StateId.ToString())}"));
        
        var newState = new PatreonDisconnectState(Guid.NewGuid(), DateTime.UtcNow, userId, patreonConnectionId, redirectUrl);
        disconnectStateCache.SetValue((userId, patreonConnectionId), newState);

        var state = Uri.EscapeDataString(newState.StateId.ToString());
        return Task.FromResult<IActionResult>(Ok($"{configuration["Patreon:DisconnectUri"]}?state={state}"));
    }
    
    /// <summary>
    /// Get the Patreon connection link for a user.
    /// </summary>
    /// <param name="userId">The Internal user ID</param>
    /// <param name="redirectUrl">The final redirect URL after Patreon authorization and after the callback. Probably is the discord channel the user pressed the link button in.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [Authorize(Roles = "Patreon.OAuth")]
    [HttpGet("oauth/connection-link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces<string>]
    public Task<IActionResult> GetPatreonConnectionLink([FromQuery] long userId, [FromQuery] string redirectUrl)
    {
        var clientId = configuration["Patreon:ClientId"] ?? throw new InvalidOperationException("Patreon ClientId is not configured.");
        var callbackUri = Uri.EscapeDataString(configuration["Patreon:RedirectUri"] ?? throw new InvalidOperationException("Patreon RedirectUri is not configured."));;
        var scopes = Uri.EscapeDataString("identity identity.memberships campaigns.members");
        
        if (cacheService.TryGetValue(userId, out var existingState) && existingState.Timestamp >= DateTime.UtcNow.AddHours(-1) && existingState.RedirectUrl == redirectUrl)
            return Task.FromResult<IActionResult>(Ok($"https://www.patreon.com/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri={callbackUri}&state={Uri.EscapeDataString(existingState.StateId.ToString())}&scope={scopes}"));
        
        var newState = new PatreonConnectionState(Guid.NewGuid(), DateTime.UtcNow, userId, redirectUrl); 
        cacheService.SetValue(userId, newState);
        
        var state = Uri.EscapeDataString(newState.StateId.ToString());
        return Task.FromResult<IActionResult>(Ok($"https://www.patreon.com/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri={callbackUri}&state={state}&scope={scopes}"));
    }

    [Authorize(Roles = "Patreon.Read")]
    [HttpGet("connections/{patreonConnectionId:long}")]
    [ProducesResponseType(typeof(ApiPatreonConnection), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiPatreonConnectionWithUsers), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatreonConnectionByConnectionId([FromRoute] long patreonConnectionId,
        [FromQuery] bool includeUsers = false)
    {
        var patreonConnectionResult = await patreonService.GetPatreonConnection(patreonConnectionId);
        if (!patreonConnectionResult.TryGetDataNonNull(out var patreonConnection))
            return Problem(statusCode: patreonConnectionResult.GetStatusCodeInt(),
                detail: patreonConnectionResult.ErrorMessage);

        if (!includeUsers) return Ok(patreonConnection);

        var userConnectionsResult = await patreonService.GetUsersByPatreonConnectionId(patreonConnectionId);
        if (!userConnectionsResult.TryGetDataNonNull(out var userConnectionsEnum))
            return Problem(statusCode: userConnectionsResult.GetStatusCodeInt(),
                detail: userConnectionsResult.ErrorMessage);

        var userConnections = userConnectionsEnum.ToList();
        var userList = new List<User>();

        foreach (var uconn in userConnections)
        {
            var userResult = await userService.GetUserByUserId(uconn.UserId);
            if (userResult.TryGetDataNonNull(out var userModel))
                userList.Add(userModel);
        }

        var mappedWithUsers = new ApiPatreonConnectionWithUsers
        {
            PatreonConnectionId = patreonConnection.PatreonConnectionId,
            Users = userList,
            UpdatedOn = patreonConnection.UpdatedOn,
            CreatedOn = patreonConnection.CreatedOn,
            FullName = patreonConnection.FullName,
            Pledge = patreonConnection.Pledge
        };
        return Ok(mappedWithUsers);
    }
}