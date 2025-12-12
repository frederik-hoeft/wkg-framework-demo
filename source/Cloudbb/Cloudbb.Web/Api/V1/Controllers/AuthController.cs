using Cloudbb.Web.Api.V1.Models.Auth;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Cloudbb.Web.Services.Auth;
using Cloudbb.Web.Services.Auth.Policies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Transactions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Cloudbb.Web.Api.V1.Controllers;

/// <summary>
/// Provides authentication endpoints for user registration, login, and logout.
/// </summary>
public sealed partial class AuthController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    IJwtService jwtService,
    IConfiguration configuration,
    ITimingRandomizationService timingRandomizationService,
    ITransactionServiceHandle transactionServiceHandle
) : DatabaseController<CloudbbDbContext>(transactionServiceHandle)
{
    public partial Task<IActionResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken) => Transaction.Scoped.RunAsync(async (dbContext, transaction, ct) =>
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ModelState.IsValid)
        {
            return transaction.Rollback(BadRequest(AuthResponse.Failure("Invalid request data")));
        }
        IdentityUser identityUser = new()
        {
            UserName = request.Username,
            Email = request.Email
        };
        IdentityResult result = await userManager.CreateAsync(identityUser, request.Password);
        if (!result.Succeeded)
        {
            return transaction.Rollback(BadRequest(AuthResponse.Failure(string.Join("; ", result.Errors.Select(e => e.Description)))));
        }
        // assign default user role
        await userManager.AddToRoleAsync(identityUser, AuthRoles.USER);
        CloudbbUser user = new(identityUser);
        dbContext.Add(user);
        await dbContext.SaveChangesAsync(ct);
        AuthResponse tokenResponse = await IssueTokenAsync(user, ct);
        return transaction.Commit(Ok(tokenResponse));
    }, cancellationToken);

    public partial Task<IActionResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken) => Transaction.Scoped.RunAsync(async (dbContext, transaction, ct) =>
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ModelState.IsValid)
        {
            return transaction.Rollback(BadRequest(AuthResponse.Failure("Invalid request data")));
        }
        string normalizedEmail = userManager.NormalizeEmail(request.Email);
        CloudbbUser? user = await dbContext.Set<CloudbbUser>()
            .Include(u => u.IdentityUser)
            .FirstOrDefaultAsync(u => u.IdentityUser.NormalizedEmail == normalizedEmail, ct);
        if (user is not { IdentityUser: { } identityUser })
        {
            // avoid user enumeration through timing attacks
            await timingRandomizationService.DelayAsync(configuration.GetValue<TimeSpan>("Auth:MaxSideChannelTimingDelay"), ct);
            return transaction.Rollback(Unauthorized(AuthResponse.Failure("Invalid email or password")));
        }
        SignInResult result = await signInManager.CheckPasswordSignInAsync(identityUser, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            // avoid user enumeration through timing attacks
            await timingRandomizationService.DelayAsync(configuration.GetValue<TimeSpan>("Auth:MaxSideChannelTimingDelay"), ct);
            // must be commit since we need to record the failed login attempt for lockout purposes
            return transaction.Commit(Unauthorized(AuthResponse.Failure("Invalid email or password")));
        }
        AuthResponse tokenResponse = await IssueTokenAsync(user, ct);
        return transaction.Commit(Ok(tokenResponse));
    }, cancellationToken);

    private async Task<AuthResponse> IssueTokenAsync(CloudbbUser user, CancellationToken cancellationToken)
    {
        // issue JWT token
        IList<string> roles = await userManager.GetRolesAsync(user.IdentityUser);
        IJwtToken token = await jwtService.GenerateTokenAsync(user, roles, cancellationToken);
        string serializedToken = await token.SerializeAsync(cancellationToken);
        return AuthResponse.Success(serializedToken, token.Token.ValidTo);
    }

    public partial Task<IActionResult> LogoutAsync(CancellationToken cancellationToken) => Transaction.Scoped.RunAsync<IActionResult>(async (dbContext, transaction, ct) =>
    {
        await signInManager.SignOutAsync();
        return transaction.Commit(Ok());
    }, cancellationToken);
}