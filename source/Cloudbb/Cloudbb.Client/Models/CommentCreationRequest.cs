namespace Cloudbb.Client.Models;

/// <summary>
/// Request data for creating a new comment on an existing forum post.
/// </summary>
public sealed class CommentCreationRequest
{
    public Guid PostId { get; set; }
    public string Content { get; set; } = string.Empty;
}
