using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Posts;

public sealed record PostReadRequest
(
    [Required] Guid PostId,
    [Required] TimeZone TimeZone,
    bool IncludeComments
);