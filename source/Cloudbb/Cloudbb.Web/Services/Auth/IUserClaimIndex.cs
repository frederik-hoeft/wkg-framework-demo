using System.Diagnostics.CodeAnalysis;

namespace Cloudbb.Web.Services.Auth;

/// <summary>
/// Provides convenient access to user claims from the current HTTP context.
/// Simplifies extracting common user information from JWT tokens without
/// repeatedly parsing the ClaimsPrincipal in controllers and services.
/// </summary>
public interface IUserClaimIndex
{
    /// <summary>
    /// Gets a value indicating whether the current request has an authenticated user.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Attempts to extract the user ID from the current authentication context.
    /// </summary>
    /// <param name="userId">When successful, contains the user's unique identifier.</param>
    /// <returns>True if the user ID was successfully extracted; otherwise, false.</returns>
    bool TryGetUserId(out Guid userId);

    /// <summary>
    /// Gets the user ID from the current authentication context.
    /// </summary>
    /// <returns>The user's unique identifier.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no authenticated user or user ID claim is present.</exception>
    Guid GetUserId();

    /// <summary>
    /// Attempts to extract the username from the current authentication context.
    /// </summary>
    /// <param name="userName">When successful, contains the user's display name.</param>
    /// <returns>True if the username was successfully extracted; otherwise, false.</returns>
    bool TryGetUsername([NotNullWhen(true)] out string? userName);

    /// <summary>
    /// Gets the username from the current authentication context.
    /// </summary>
    /// <returns>The user's display name.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no authenticated user or username claim is present.</exception>
    string GetUsername();

    /// <summary>
    /// Attempts to extract a specific claim value from the current authentication context.
    /// </summary>
    /// <param name="claimType">The type of claim to retrieve.</param>
    /// <param name="claimValue">When successful, contains the claim value.</param>
    /// <returns>True if the claim was found; otherwise, false.</returns>
    bool TryGetClaim(string claimType, [NotNullWhen(true)] out string? claimValue);
}
