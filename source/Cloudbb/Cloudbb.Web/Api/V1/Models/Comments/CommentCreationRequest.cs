using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Comments;

/// <summary>
/// Request data for creating a new comment on an existing forum post.
/// Contains validation attributes to ensure content quality and proper post association.
/// </summary>
public sealed class CommentCreationRequest
{
    /// <summary>
    /// Unique identifier of the forum post that this comment will be attached to.
    /// Must reference an existing, accessible post in the database.
    /// </summary>
    [Required]
    public required Guid PostId { get; set; }

    /// <summary>
    /// The text content of the comment being created.
    /// Limited to 1024 characters to encourage concise, meaningful contributions to the discussion.
    /// Must contain at least one character of actual content.
    /// </summary>
    [Required]
    [StringLength(1024, MinimumLength = 1)]
    public required string Content { get; set; }
}