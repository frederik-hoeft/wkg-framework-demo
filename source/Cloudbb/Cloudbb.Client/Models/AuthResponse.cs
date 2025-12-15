using System.Diagnostics.CodeAnalysis;

namespace Cloudbb.Client.Models;

/// <summary>
/// Authentication response model
/// </summary>
/// <param name="Token">The JWT bearer token if authentication was successful; otherwise, null.</param>
/// <param name="Message">The error message if authentication failed; otherwise, null.</param>
/// <param name="ExpiresAt">The expiration time of the token if authentication was successful; otherwise, null.</param>
public sealed record AuthResponse(string? Token, string? Message, DateTime? ExpiresAt)
{
    [MemberNotNullWhen(true, nameof(Token))]
    public bool IsSuccess => Token is { Length: > 0 };
}