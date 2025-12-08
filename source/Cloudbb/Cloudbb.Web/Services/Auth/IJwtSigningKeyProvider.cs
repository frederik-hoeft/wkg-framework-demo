using Microsoft.IdentityModel.Tokens;

namespace Cloudbb.Web.Services.Auth;

public interface IJwtSigningKeyProvider : IDisposable
{
    ValueTask<SecurityKey> GetKeyAsync(CancellationToken cancellationToken = default);
}
