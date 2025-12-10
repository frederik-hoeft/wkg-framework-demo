using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Request payload for creating a new post in the Cloudbb forum.
/// Contains the essential content required to author a forum post.
/// </summary>
/// <param name="Title">The post title, limited to 256 characters for readability and database efficiency.</param>
/// <param name="Content">The main body content of the post, supporting rich text or markdown.</param>
public sealed record PostCreationRequest
(
    [Required][StringLength(256)] string Title,
    [Required] string Content
);