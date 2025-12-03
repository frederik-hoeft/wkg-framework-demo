using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class UserClaimIndex(IHttpContextAccessor httpContextAccessor) : IUserClaimIndex
{
    private Dictionary<string, string>? _claimIndex;

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public Guid GetUserId()
    {
        if (TryGetUserId(out Guid userId))
        {
            return userId;
        }
        throw new InvalidOperationException("The user is not authenticated or does not have a valid user ID claim.");
    }

    public string GetUsername()
    {
        if (TryGetUsername(out string? username) && !string.IsNullOrEmpty(username))
        {
            return username;
        }
        throw new InvalidOperationException("The user is not authenticated or does not have a username claim.");
    }

    public bool TryGetUserId(out Guid userId)
    {
        userId = Guid.Empty;
        return TryGetClaim(ClaimTypes.NameIdentifier, out string? userIdString) && Guid.TryParse(userIdString, out userId);
    }

    public bool TryGetUsername([NotNullWhen(true)] out string? userName) => TryGetClaim(ClaimTypes.Name, out userName);

    public bool TryGetClaim(string claimType, [NotNullWhen(true)] out string? claimValue)
    {
        if (_claimIndex is not null)
        {
            return _claimIndex.TryGetValue(claimType, out claimValue);
        }
        if (!IsAuthenticated)
        {
            goto FAILURE;
        }
        _claimIndex = [];
        IEnumerable<Claim>? claims = httpContextAccessor.HttpContext?.User?.Claims;
        if (claims is null)
        {
            goto FAILURE;
        }
        foreach (Claim claim in claims)
        {
            _claimIndex[claim.Type] = claim.Value;
        }
        return _claimIndex.TryGetValue(claimType, out claimValue);
    FAILURE:
        claimValue = null;
        return false;
    }
}
