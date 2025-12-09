using System.ComponentModel.DataAnnotations;
using Wkg.AspNetCore.Validation;

namespace Cloudbb.Web.Api.V1.Models.Auth;

public sealed record LoginRequest
(
    [Required][ValidEmailAddress] string Email,
    [Required] string Password
);