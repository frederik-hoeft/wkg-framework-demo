using System.Diagnostics.CodeAnalysis;

namespace Cloudbb.Web.Services.Auth;

public interface IUserClaimIndex
{
    bool IsAuthenticated { get; }

    bool TryGetUserId(out Guid userId);

    Guid GetUserId();

    bool TryGetUsername([NotNullWhen(true)] out string? userName);

    string GetUsername();

    bool TryGetClaim(string claimType, [NotNullWhen(true)] out string? claimValue);
}
