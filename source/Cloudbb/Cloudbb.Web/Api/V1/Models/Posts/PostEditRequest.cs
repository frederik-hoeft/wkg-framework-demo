using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Request payload for editing an existing post in the Cloudbb forum.
/// </summary>
/// <param name="PostId">The unique identifier of the post to be edited.</param>
/// <param name="Title">The updated post title, limited to 256 characters.</param>
/// <param name="Content">The updated main body content of the post.</param>
public sealed record PostEditRequest
(
    [Required] Guid PostId,
    [Required][StringLength(256)] string Title,
    [Required] string Content
);