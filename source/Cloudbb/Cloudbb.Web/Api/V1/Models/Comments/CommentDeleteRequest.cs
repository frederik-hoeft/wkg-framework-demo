using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Comments;

/// <summary>
/// Request data for permanently deleting an existing forum comment.
/// Only the original author of the comment has permission to perform this operation.
/// </summary>
public sealed class CommentDeleteRequest
{
    /// <summary>
    /// Unique identifier of the comment to be deleted.
    /// Must reference an existing comment that belongs to the authenticated user.
    /// </summary>
    [Required] 
    public required Guid CommentId { get; set; }
}