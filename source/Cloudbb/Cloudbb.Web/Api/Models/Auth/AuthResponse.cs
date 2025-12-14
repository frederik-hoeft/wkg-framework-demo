using System.Diagnostics.CodeAnalysis;

namespace Cloudbb.Web.Api.Models.Auth;

public sealed record AuthResponse(string? Token, string? Message, DateTime? ExpiresAt)
{
    [MemberNotNullWhen(true, nameof(Token))]
    public bool IsSuccess => Token is { Length: > 0 };

    public static AuthResponse Failure(string message) => new(Token: null, Message: message, ExpiresAt: null);

    public static AuthResponse Success(string token, DateTime expiresAt) => new(Token: token, Message: null, ExpiresAt: expiresAt);
}