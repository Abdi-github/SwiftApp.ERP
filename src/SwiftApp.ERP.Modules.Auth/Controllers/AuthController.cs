using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Auth.Application.DTOs;
using SwiftApp.ERP.Modules.Auth.Application.Services;

namespace SwiftApp.ERP.Modules.Auth.Controllers;

/// <summary>
/// Authentication endpoints — login, token refresh.
/// Maps to Java: AuthController (POST /api/v1/auth/login).
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController(
    UserService userService,
    JwtTokenProvider jwtTokenProvider,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<JwtResponse>(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        // var sw = System.Diagnostics.Stopwatch.StartNew();
        var user = await userService.AuthenticateAsync(request.Username, request.Password, ct);

        if (user is null)
        {
            logger.LogWarning("Failed login attempt for user {Username}", request.Username);
            return Unauthorized(new ProblemDetails
            {
                Title = "Authentication failed",
                Detail = "Invalid username or password.",
                Status = 401
            });
        }

        var token = jwtTokenProvider.GenerateToken(user);
        // sw.Stop();
        // logger.LogDebug("Login pipeline timing: {ElapsedMs}ms", sw.ElapsedMilliseconds);
        // System.Diagnostics.Debug.WriteLine("Issued access credential for authenticated principal");
        logger.LogInformation("User {Username} logged in successfully", user.Username);

        return Ok(new JwtResponse(token, "Bearer", user.Username));
    }
}
