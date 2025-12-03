using System.ComponentModel.DataAnnotations;
using Wkg.AspNetCore.Validation;

namespace Cloudbb.Web.Api.Models.Auth;

public sealed record LoginRequest
(
    [Required][ValidEmailAddress] string Email,
    [Required] string Password
);