using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.Models.Auth;

public sealed record LoginRequest
(
    [Required][EmailAddress] string Email,
    [Required] string Password
);