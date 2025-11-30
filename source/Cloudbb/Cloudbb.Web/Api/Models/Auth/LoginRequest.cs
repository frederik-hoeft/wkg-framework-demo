using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.Models.Auth;

public sealed record LoginRequest
(
    [property: Required][property: EmailAddress] string Email,
    [property: Required] string Password
);