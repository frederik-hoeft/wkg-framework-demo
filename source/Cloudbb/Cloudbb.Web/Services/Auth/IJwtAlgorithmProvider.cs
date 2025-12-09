namespace Cloudbb.Web.Services.Auth;

/// <summary>
/// Provides the cryptographic algorithm identifier for JWT token signing and verification.
/// Abstracts algorithm selection to support different signing methods (HMAC, ECDSA, RSA).
/// </summary>
public interface IJwtAlgorithmProvider
{
    /// <summary>
    /// Gets the algorithm identifier string used for JWT token signing.
    /// </summary>
    /// <returns>The algorithm name (e.g., "ES256" for ECDSA with SHA-256).</returns>
    string GetAlgorithm();
}
