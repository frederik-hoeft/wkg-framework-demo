using Cloudbb.Web.Api.Models.Auth;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Cloudbb.Web.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        IdentityUser identityUser = new()
        {
            UserName = request.UserName,
            Email = request.Email
        };

        IdentityResult result = await userManager.CreateAsync(identityUser, request.Password);

        if (!result.Succeeded)
        {
            return transaction.Rollback(BadRequest(AuthResponse.Failure(string.Join("; ", result.Errors.Select(e => e.Description)))));
        }

        // assign default user role
        await userManager.AddToRoleAsync(identityUser, "user");

        CloudbbUser user = new(identityUser);
        dbContext.Add(user);

        await dbContext.SaveChangesAsync();

        IList<string> roles = await userManager.GetRolesAsync(identityUser);

        string token = await jwtService.GenerateTokenAsync(user, roles);
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

        string normalizedEmail = userManager.NormalizeEmail(request.Email);

        CloudbbUser? user = await dbContext.Set<CloudbbUser>()
            .Include(u => u.IdentityUser)
            .FirstOrDefaultAsync(u => u.IdentityUser.NormalizedEmail == normalizedEmail);

        if (user is not { IdentityUser: { } identityUser })
        {
            // avoid user enumeration through timing attacks
            await timingRandomizationService.DelayAsync(configuration.GetValue<TimeSpan>("Auth:MaxSideChannelTimingDelay"), cancellationToken);
            return transaction.Rollback(Unauthorized(AuthResponse.Failure("Invalid email or password")));
        }

        SignInResult result = await signInManager.CheckPasswordSignInAsync(identityUser, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            // avoid user enumeration through timing attacks
            await timingRandomizationService.DelayAsync(configuration.GetValue<TimeSpan>("Auth:MaxSideChannelTimingDelay"), cancellationToken);
            // must be commit since we need to record the failed login attempt for lockout purposes
            return transaction.Commit(Unauthorized(AuthResponse.Failure("Invalid email or password")));
        }

        IList<string> roles = await userManager.GetRolesAsync(identityUser);

        string token = await jwtService.GenerateTokenAsync(user, roles);
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
}