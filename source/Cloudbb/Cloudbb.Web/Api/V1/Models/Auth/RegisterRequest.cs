using System.ComponentModel.DataAnnotations;
using Wkg.AspNetCore.Validation;

namespace Cloudbb.Web.Api.V1.Models.Auth;

public sealed record RegisterRequest
{
    [Required]
    [StringLength(254)]
    [ValidEmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(32, MinimumLength = 3)]
    public required string UserName { get; set; }

    [Required]
    public required string Password { get; set; }

    [Required]
    [Compare(nameof(Password))]
    public required string ConfirmPassword { get; set; }
}
