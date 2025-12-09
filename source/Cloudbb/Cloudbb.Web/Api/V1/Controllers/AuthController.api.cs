using Asp.Versioning;
using Cloudbb.Web.Api.V1.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cloudbb.Web.Api.V1.Controllers;

[ApiController]
[ApiVersion(API_V1)]
[Route("api/v{version:apiVersion}/auth")]
public partial class AuthController
{
    /// <summary>
    /// Registers a new user with the provided registration details and returns a JWT token upon successful registration.
    /// </summary>
    /// <param name="request">The registration details including email, username, password, and confirm password.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status400BadRequest)]
    public partial Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Logs in a user with the provided credentials and returns a JWT token upon successful authentication.
    /// </summary>
    /// <param name="request">The login credentials including email and password.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status401Unauthorized)]
    public partial Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Logs out the currently authenticated user.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    [Authorize]
    [HttpPost("logout")]
    public partial Task<IActionResult> LogoutAsync(CancellationToken cancellationToken);
}