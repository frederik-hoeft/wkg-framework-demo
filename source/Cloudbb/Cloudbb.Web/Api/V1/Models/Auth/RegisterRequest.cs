using System.ComponentModel.DataAnnotations;
using Wkg.AspNetCore.Validation;

namespace Cloudbb.Web.Api.V1.Models.Auth;

/// <summary>
/// Registration request model
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>
    /// The email address of the user to register.
    /// Must be a valid email format in accordance with RFC 5322.
    /// Must not already be in use.
    /// </summary>
    [Required]
    [StringLength(254)]
    [ValidEmailAddress]
    public required string Email { get; set; }

    /// <summary>
    /// The username of the user to register.
    /// Must not already be in use.
    /// </summary>
    [Required]
    [StringLength(32, MinimumLength = 3)]
    public required string Username { get; set; }

    /// <summary>
    /// The password for the new user account.
    /// </summary>
    [Required]
    public required string Password { get; set; }

    /// <summary>
    /// Confirmation of the password for the new user account.
    /// Must match the Password field.
    /// </summary>
    [Required]
    [Compare(nameof(Password))]
    public required string ConfirmPassword { get; set; }
}
