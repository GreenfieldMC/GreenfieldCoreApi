using Asp.Versioning;
using GreenfieldCoreModels.ApiModels.User;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenfieldCoreApi.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class LoginController(IClientAuthService clientAuthService, IConfiguration config) : ControllerBase
{

    [AllowAnonymous]
    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<IActionResult> Token([FromForm] TokenRequest request)
    {
        var token = await clientAuthService.AuthenticateLogin(request.client_id, request.client_secret);
        return Ok(new {
            access_token = token, 
            token_type = "Bearer", 
            expires_in = config.GetValue<int>("jwtSettings:expiryInMinutes") * 60 
        });
    }
    
    public record TokenRequest(Guid client_id, string client_secret, string grant_type);
    
}