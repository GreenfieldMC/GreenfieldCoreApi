using Asp.Versioning;
using GreenfieldCoreApi.Extensions;
using GreenfieldCoreServices.Models.Discord;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[Authorize(Roles = "Discord")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class DiscordController(IConfiguration configuration, IDiscordService discordService, IDiscordApi discordApi, ICacheService<long, DiscordConnectionState> cacheService) : ControllerBase
{
    /// <summary>
    /// Discord OAuth callback.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("callback")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConnectionCallback([FromQuery] string state, [FromQuery] string code)
    {
        if (!Guid.TryParse(state, out var stateId))
            return ResourceHelpers.Redirect(RedirectType.Error, "./", "Could not parse your session state. Please retry linking your Discord account again.");

        if (!cacheService.TryGetValue(s => s.StateId == stateId, out var connectionState) || connectionState.Timestamp < DateTime.UtcNow.AddHours(-1))
        {
            cacheService.RemoveValues(s => s.StateId == stateId);
            return ResourceHelpers.Redirect(RedirectType.Error, "./", "Your session state is invalid or has expired. Please retry linking your Discord account again.");
        }

        if (string.IsNullOrEmpty(code))
            return ResourceHelpers.Redirect(RedirectType.Error, "./", "Invalid code parameter. Please retry linking your Discord account again.");

        var linkResult = await discordApi.LinkDiscordAccountToUser(connectionState.UserId, code);

        return linkResult.IsSuccessful
            ? ResourceHelpers.Redirect(RedirectType.Info, connectionState.RedirectUrl, "Your Discord account has been successfully linked!")
            : ResourceHelpers.Redirect(RedirectType.Error, "./", $"Failed to link your Discord account: {linkResult.ErrorMessage}");
    }

    /// <summary>
    /// Get the Discord connection link for a user.
    /// </summary>
    /// <param name="userId">The Internal user ID</param>
    /// <param name="redirectUrl">The final redirect URL after Discord authorization and after the callback. Probably is the discord channel the user pressed the link button in.</param>
    [Authorize(Roles = "Discord.OAuth")]
    [HttpGet("connection-link")]
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

    /// <summary>
    /// Get Discord account references by Discord snowflake ID.
    /// </summary>
    /// <param name="discordSnowflake"></param>
    /// <returns></returns>
    [Authorize(Roles = "Discord.Read")]
    [HttpGet("{discordSnowflake}/references")]
    public async Task<IActionResult> GetDiscordReferencesById(ulong discordSnowflake)
    {
        var discordReferenceResult = await discordService.GetDiscordConnectionBySnowflake(discordSnowflake);
        return discordReferenceResult.TryGetDataNonNull(out var references)
            ? Ok(references)
            : Problem(statusCode: discordReferenceResult.GetStatusCodeInt(), detail: discordReferenceResult.ErrorMessage);
    }
    
}
