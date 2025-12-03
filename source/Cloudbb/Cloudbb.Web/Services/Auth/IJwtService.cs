using Cloudbb.Web.Data.Model;
using System.Security.Claims;

namespace Cloudbb.Web.Services.Auth;

public interface IJwtService
{
    ValueTask<string> GenerateTokenAsync(CloudbbUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default);

    ValueTask<ClaimsPrincipal> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}