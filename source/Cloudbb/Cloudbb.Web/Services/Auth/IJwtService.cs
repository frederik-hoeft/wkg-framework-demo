using Cloudbb.Web.Data.Model;
using System.Security.Claims;

namespace Cloudbb.Web.Services.Auth;

/// <summary>
/// Core service for JWT token generation and validation in the authentication system.
/// Handles the creation of secure tokens for user sessions and validates incoming tokens
/// for request authentication and authorization.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates a new JWT token for the specified user with assigned roles.
    /// Includes standard claims like user ID, email, username, and custom forum roles.
    /// </summary>
    /// <param name="user">The forum user to generate a token for.</param>
    /// <param name="roles">Collection of role names to include in the token.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>A complete JWT token ready for client use.</returns>
    ValueTask<IJwtToken> GenerateTokenAsync(CloudbbUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a JWT token string and extracts the user claims.
    /// Verifies signature, expiration, and other security constraints.
    /// </summary>
    /// <param name="token">The JWT token string to validate.</param>
    /// <param name="cancellationToken">Cancellation token for async operation.</param>
    /// <returns>ClaimsPrincipal containing the validated user identity and claims.</returns>
    ValueTask<ClaimsPrincipal> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}