using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Cloudbb.Web.Services.Auth;

public interface IJwtService
{
    string GenerateToken(IdentityUser user, IEnumerable<string> roles);

    ClaimsPrincipal ValidateToken(string token);
}