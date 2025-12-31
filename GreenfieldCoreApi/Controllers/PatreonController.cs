using Asp.Versioning;
using GreenfieldCoreApi.ApiModels;
using GreenfieldCoreApi.Extensions;
using GreenfieldCoreServices.Models.Patreon;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[Authorize(Roles = "Patreon")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PatreonController(IConfiguration configuration, IPatreonService patreonService, IPatreonApi patreonApi, ICacheService<long, PatreonConnectionState> cacheService) : ControllerBase
{
 
    /// <summary>
    /// This endpoint is called by Patreon after a user authorizes the application.
    /// </summary>
    /// <param name="state">Contains the user id of the user who is adding a Patreon connection</param>
    /// <param name="code">Contains the authorization code returned by Patreon</param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("callback")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConnectionCallback([FromQuery] string state, [FromQuery] string code)
    {
        if (!Guid.TryParse(state, out var stateId)) 
            return ResourceHelpers.Redirect(RedirectType.Error, "./", "Could not parse your session state. Please retry linking your Patreon account again.");

        if (!cacheService.TryGetValue(s => s.StateId == stateId, out var connectionState) || connectionState.Timestamp < DateTime.UtcNow.AddHours(-1))
        {
            cacheService.RemoveValues(s => s.StateId == stateId);
            return ResourceHelpers.Redirect(RedirectType.Error, "./", "Your session state is invalid or has expired. Please retry linking your Patreon account again.");
        }
        
        var linkResult = await patreonApi.LinkPatreonAccountToUser(connectionState.UserId, code);
        
        return linkResult.IsSuccessful
            ? ResourceHelpers.Redirect(RedirectType.Info, connectionState.RedirectUrl, "Your Patreon account has been successfully linked!")
            : ResourceHelpers.Redirect(RedirectType.Error, "./", $"Failed to link your Patreon account: {linkResult.ErrorMessage}");
    }
    
    /// <summary>
    /// Get the Patreon connection link for a user.
    /// </summary>
    /// <param name="userId">The Internal user ID</param>
    /// <param name="redirectUrl">The final redirect URL after Patreon authorization and after the callback. Probably is the discord channel the user pressed the link button in.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [Authorize(Roles = "Patreon.OAuth")]
    [HttpGet("connection-link")]
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

    /// <summary>
    /// Get all Patreon account references by Patreon ID.
    /// </summary>
    /// <param name="patreonId"></param>
    /// <returns></returns>
    [Authorize(Roles = "Patreon.Read")]
    [HttpGet("{patreonId:long}/references")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces(typeof(IEnumerable<ApiUserPatreonAccount>))]
    public async Task<IActionResult> GetPatreonReferencesById(long patreonId)
    {
        var patreonReferenceResult = await patreonService.GetPatreonAccountsByPatreonId(patreonId);
        return patreonReferenceResult.TryGetDataNonNull(out var references)
            ? Ok(references)
            : Problem(statusCode: patreonReferenceResult.GetStatusCodeInt(), detail: patreonReferenceResult.ErrorMessage);
    }
    
}