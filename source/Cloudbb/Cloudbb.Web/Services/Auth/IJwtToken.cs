using System.IdentityModel.Tokens.Jwt;

namespace Cloudbb.Web.Services.Auth;

/// <summary>
/// Represents a JWT token.
/// </summary>
public interface IJwtToken
{
    /// <summary>
    /// The underlying JWT security token.
    /// </summary>
    JwtSecurityToken Token { get; }

    /// <summary>
    /// Serializes the JWT token to its string representation asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous serialization operation. The task result contains the serialized JWT token as a string.</returns>
    ValueTask<string> SerializeAsync(CancellationToken cancellationToken = default);
}