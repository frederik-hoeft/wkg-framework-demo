using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Cloudbb.Web.Api.Models.Auth;
using Cloudbb.Web.Services.Auth;
using System.Security.Claims;

namespace Cloudbb.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IJwtService jwtService,
    IConfiguration configuration,
    ITimingRandomizationService timingRandomizationService
) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(AuthResponse.Failure("Invalid request data"));
        }

        IdentityUser user = new()
        {
            UserName = request.Email,
            Email = request.Email
        };

        IdentityResult result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(AuthResponse.Failure(string.Join("; ", result.Errors.Select(e => e.Description))));
        }

        // Optionally assign default role
        await userManager.AddToRoleAsync(user, "User");

        IList<string> roles = await userManager.GetRolesAsync(user);
        string token = jwtService.GenerateToken(user, roles);
        double expirationMinutes = double.Parse(configuration["Auth:Jwt:ExpirationMinutes"]!);

        return Ok(AuthResponse.Success(token, DateTime.UtcNow.AddMinutes(expirationMinutes)));
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(AuthResponse.Failure("Invalid request data"));
        }

        IdentityUser? user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // avoid user enumeration through timing attacks
            await timingRandomizationService.DelayAsync(configuration.GetValue<TimeSpan>("Auth:MaxSideChannelTimingDelay"), cancellationToken);
            return Unauthorized(AuthResponse.Failure("Invalid email or password"));
        }

        SignInResult result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            // avoid user enumeration through timing attacks
            await timingRandomizationService.DelayAsync(configuration.GetValue<TimeSpan>("Auth:MaxSideChannelTimingDelay"), cancellationToken);
            return Unauthorized(AuthResponse.Failure("Invalid email or password"));
        }

        IList<string> roles = await userManager.GetRolesAsync(user);
        string token = jwtService.GenerateToken(user, roles);
        double expirationMinutes = double.Parse(configuration["Jwt:ExpirationMinutes"]!);

        return Ok(AuthResponse.Success(token, DateTime.UtcNow.AddMinutes(expirationMinutes)));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogoutAsync()
    {
        await signInManager.SignOutAsync();
        return Ok(new { Message = "Logout successful" });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfileAsync()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }

        IdentityUser? user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        IList<string> roles = await userManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.UserName,
            Roles = roles
        });
    }
}