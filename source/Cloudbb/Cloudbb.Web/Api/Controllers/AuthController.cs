using Cloudbb.Web.Api.Models.Auth;
using Cloudbb.Web.Data;
using Cloudbb.Web.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Transactions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Cloudbb.Web.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IJwtService jwtService,
    IConfiguration configuration,
    ITimingRandomizationService timingRandomizationService,
    ITransactionServiceHandle transactionServiceHandle
) : DatabaseController<ApplicationDbContext>(transactionServiceHandle)
{
    [HttpPost("register")]
    // TODO: add proper support for cancellation tokens in Wkg.AspNetCore with the .NET 10 migration
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request) => await Transaction.Scoped.RunAsync(async (dbContext, transaction) =>
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ModelState.IsValid)
        {
            return transaction.Rollback(BadRequest(AuthResponse.Failure("Invalid request data")));
        }

        IdentityUser user = new()
        {
            UserName = request.Email,
            Email = request.Email
        };

        IdentityResult result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return transaction.Rollback(BadRequest(AuthResponse.Failure(string.Join("; ", result.Errors.Select(e => e.Description)))));
        }

        // assign default user role
        await userManager.AddToRoleAsync(user, "user");

        IList<string> roles = await userManager.GetRolesAsync(user);
        string token = jwtService.GenerateToken(user, roles);
        double expirationMinutes = double.Parse(configuration["Auth:Jwt:ExpirationMinutes"]!);

        return transaction.Commit(Ok(AuthResponse.Success(token, DateTime.UtcNow.AddMinutes(expirationMinutes))));
    });

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken) => await Transaction.Scoped.RunAsync(async (dbContext, transaction) =>
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ModelState.IsValid)
        {
            return transaction.Rollback(BadRequest(AuthResponse.Failure("Invalid request data")));
        }

        IdentityUser? user = await userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // avoid user enumeration through timing attacks
            await timingRandomizationService.DelayAsync(configuration.GetValue<TimeSpan>("Auth:MaxSideChannelTimingDelay"), cancellationToken);
            return transaction.Rollback(Unauthorized(AuthResponse.Failure("Invalid email or password")));
        }

        SignInResult result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            // avoid user enumeration through timing attacks
            await timingRandomizationService.DelayAsync(configuration.GetValue<TimeSpan>("Auth:MaxSideChannelTimingDelay"), cancellationToken);
            // must be commit since we need to record the failed login attempt for lockout purposes
            return transaction.Commit(Unauthorized(AuthResponse.Failure("Invalid email or password")));
        }

        IList<string> roles = await userManager.GetRolesAsync(user);
        string token = jwtService.GenerateToken(user, roles);
        double expirationMinutes = double.Parse(configuration["Auth:Jwt:ExpirationMinutes"]!);

        return transaction.Commit(Ok(AuthResponse.Success(token, DateTime.UtcNow.AddMinutes(expirationMinutes))));
    });

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync() => await Transaction.Scoped.RunAsync(async (dbContext, transaction) =>
    {
        await signInManager.SignOutAsync();
        return transaction.Commit(Ok(new { Message = "Logout successful" }));
    });

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfileAsync() => await Transaction.Scoped.RunReadOnlyAsync(async dbContext =>
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
    });
}