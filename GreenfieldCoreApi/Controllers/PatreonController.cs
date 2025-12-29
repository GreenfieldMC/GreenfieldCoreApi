using Asp.Versioning;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[Authorize(Roles = "Patreon")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PatreonController(IConfiguration configuration, IPatreonApi patreonApi) : ControllerBase
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
        var stateParts = state.Split('|');
        if (stateParts.Length != 2 || !long.TryParse(stateParts[0], out var userId))
            return BadRequest("Invalid state parameter.");

        var redirectUrl = stateParts[1];

        if (string.IsNullOrEmpty(code))
            return BadRequest("Invalid code parameter.");

        var linkResult = await patreonApi.LinkPatreonAccountToUser(userId, code, state);
        if (!linkResult.IsSuccessful)
            return StatusCode(linkResult.GetStatusCodeInt(), linkResult.ErrorMessage);

        var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "SuccessfulConnectionRedirect.html");
        var htmlContent = await System.IO.File.ReadAllTextAsync(htmlPath);
        htmlContent = htmlContent.Replace("{{REDIRECT_URL}}", redirectUrl);
        htmlContent = htmlContent.Replace("{{MESSAGE}}", "Your Patreon account has been successfully linked! You may close this tab.");
        
        return new ContentResult
        {
            ContentType = "text/html",
            Content = htmlContent
        };
    }
    
    /// <summary>
    /// Get the Patreon connection link for a user.
    /// </summary>
    /// <param name="userId">The Internal user ID</param>
    /// <param name="redirectUrl">The final redirect URL after Patreon authorization and after the callback. Probably is the discord channel the user pressed the link button in.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    [HttpGet("user/{userId:long}/connection-link")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces<string>]
    public Task<IActionResult> GetPatreonConnectionLink([FromRoute] long userId, [FromQuery] string redirectUrl)
    {
        var clientId = configuration["Patreon:ClientId"] ?? throw new InvalidOperationException("Patreon ClientId is not configured.");
        var redirectUri = Uri.EscapeDataString(configuration["Patreon:RedirectUri"] ?? throw new InvalidOperationException("Patreon RedirectUri is not configured."));;
        var scopes = Uri.EscapeDataString("identity identity.memberships campaigns.members");
        var state = Uri.EscapeDataString($"{userId}|{redirectUrl}");

        return Task.FromResult<IActionResult>(Ok($"https://www.patreon.com/oauth2/authorize?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&state={state}&scope={scopes}"));
    }
    
}