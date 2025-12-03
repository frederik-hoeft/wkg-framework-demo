using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.Models.Auth;

public sealed record RegisterRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public required string Password { get; set; }

    [Required]
    [Compare(nameof(Password))]
    public required string ConfirmPassword { get; set; }

    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
}
