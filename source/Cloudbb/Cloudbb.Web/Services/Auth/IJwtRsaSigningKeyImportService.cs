using System.Security.Cryptography;

namespace Cloudbb.Web.Services.Auth;

/// <summary>
/// Service for importing RSA private keys used in JWT token signing.
/// Abstracts key import mechanisms to support different sources like PEM files,
/// key stores, or hardware security modules.
/// </summary>
public interface IJwtRsaSigningKeyImportService
{
    /// <summary>
    /// Imports an RSA private key for JWT token signing operations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The imported RSA key ready for cryptographic operations.</returns>
    ValueTask<RSA> ImportKeyAsync(CancellationToken cancellationToken = default);
}
