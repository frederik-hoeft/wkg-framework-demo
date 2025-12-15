namespace Cloudbb.Client.Models;

/// <summary>
/// Complete post details response including content, metadata, voting information, and optional comments.
/// </summary>
public sealed class PostReadResponse
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int VoteScore { get; set; }
    public VoteType UserVote { get; set; }
    public DateTime LastModified { get; set; }
    public int Revisions { get; set; }
    public bool CanEdit { get; set; }
    public List<CommentResponseEntry>? Comments { get; set; }
}
