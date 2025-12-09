using System.Security.Cryptography;

namespace Cloudbb.Web.Services.Auth;

/// <summary>
/// Service for importing ECDSA private keys used in JWT token signing.
/// Abstracts key import mechanisms to support different sources like PEM files,
/// key stores, or hardware security modules.
/// </summary>
public interface IJwtECDsaSigningKeyImportService
{
    /// <summary>
    /// Imports an ECDSA private key for JWT token signing operations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>The imported ECDSA key ready for cryptographic operations.</returns>
    ValueTask<ECDsa> ImportKeyAsync(CancellationToken cancellationToken = default);
}
