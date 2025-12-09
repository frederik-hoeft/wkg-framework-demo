using Microsoft.IdentityModel.Tokens;

namespace Cloudbb.Web.Services.Auth;

/// <summary>
/// Provides cryptographic keys for JWT token signing and verification.
/// Supports asynchronous key retrieval to enable scenarios like key rotation,
/// hardware security modules, or remote key management.
/// </summary>
public interface IJwtSigningKeyProvider : IDisposable
{
    /// <summary>
    /// Retrieves the security key used for JWT token signing and verification.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The security key for JWT operations.</returns>
    ValueTask<SecurityKey> GetKeyAsync(CancellationToken cancellationToken = default);
}
