using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.Models.Posts;

public record PostCreationRequest
(
    [Required][StringLength(256)] string Title,
    [Required] string Content
);