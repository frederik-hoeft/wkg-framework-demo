namespace Cloudbb.Client.Models;

/// <summary>
/// Represents a comment entry in a post's comment thread.
/// </summary>
public sealed class CommentResponseEntry
{
    public Guid CommentId { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int VoteScore { get; set; }
    public VoteType UserVote { get; set; }
    public bool CanEdit { get; set; }
    public DateTime CreationTime { get; set; }
}
