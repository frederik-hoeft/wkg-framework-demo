using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Posts;

public sealed record PostListRequest
(
    [Required] TimeZone TimeZone,
    [Range(1, int.MaxValue)] int PageNumber = 1,
    [Range(1, int.MaxValue)] int PageSize = 20
);