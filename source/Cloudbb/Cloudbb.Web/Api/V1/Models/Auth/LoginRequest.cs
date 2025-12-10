using System.ComponentModel.DataAnnotations;
using Wkg.AspNetCore.Validation;

namespace Cloudbb.Web.Api.V1.Models.Auth;

/// <summary>
/// Represents a login request.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// The email address of the user.
    /// </summary>
    [Required]
    [ValidEmailAddress]
    public required string Email { get; set; }

    /// <summary>
    /// The password of the user.
    /// </summary>
    [Required]
    public required string Password { get; set; }
}