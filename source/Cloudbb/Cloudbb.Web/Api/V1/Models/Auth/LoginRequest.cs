using System.ComponentModel.DataAnnotations;
using Wkg.AspNetCore.Validation;

namespace Cloudbb.Web.Api.V1.Models.Auth;

/// <summary>
/// Represents a login request.
/// </summary>
/// <param name="Email">The email address of the user.</param>
/// <param name="Password">The password of the user.</param>
public sealed record LoginRequest
(
    [Required][ValidEmailAddress] string Email,
    [Required] string Password
);