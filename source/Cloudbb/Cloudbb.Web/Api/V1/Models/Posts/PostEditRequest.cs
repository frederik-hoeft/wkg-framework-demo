using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Request payload for editing an existing post in the Cloudbb forum.
/// </summary>
public sealed class PostEditRequest
{
    /// <summary>
    /// The unique identifier of the post to be edited.
    /// </summary>
    [Required] 
    public required Guid PostId { get; set; }

    /// <summary>
    /// The updated post title, limited to 256 characters.
    /// </summary>
    [Required]
    [StringLength(256, MinimumLength = 1)] 
    public required string Title { get; set; }

    /// <summary>
    /// The updated main body content of the post.
    /// </summary>
    [Required]
    [MinLength(1)]
    public required string Content { get; set; }
}