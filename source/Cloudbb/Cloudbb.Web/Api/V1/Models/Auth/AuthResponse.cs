using System.Diagnostics.CodeAnalysis;

namespace Cloudbb.Web.Api.V1.Models.Auth;

/// <summary>
/// Authentication response model
/// </summary>
/// <param name="Token">The JWT bearer token if authentication was successful; otherwise, null.</param>
/// <param name="Message">The error message if authentication failed; otherwise, null.</param>
/// <param name="ExpiresAt">The expiration time of the token if authentication was successful; otherwise, null.</param>
public sealed record AuthResponse(string? Token, string? Message, DateTime? ExpiresAt)
{
    [MemberNotNullWhen(true, nameof(Token))]
    internal bool IsSuccess => Token is { Length: > 0 };

    internal static AuthResponse Failure(string message) => new(Token: null, Message: message, ExpiresAt: null);

    internal static AuthResponse Success(string token, DateTime expiresAt) => new(Token: token, Message: null, ExpiresAt: expiresAt);
}