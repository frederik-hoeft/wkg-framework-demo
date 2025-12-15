namespace Cloudbb.Client.Models;

/// <summary>
/// Response returned after successfully creating a new comment.
/// </summary>
public sealed class CommentCreationResponse
{
    public Guid CommentId { get; set; }
}
