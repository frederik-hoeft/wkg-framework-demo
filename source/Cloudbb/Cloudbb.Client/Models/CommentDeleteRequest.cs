namespace Cloudbb.Client.Models;

/// <summary>
/// Request data for permanently deleting an existing forum comment.
/// </summary>
public sealed class CommentDeleteRequest
{
    public Guid CommentId { get; set; }
}
