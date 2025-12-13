namespace Cloudbb.Client.Models;

/// <summary>
/// Represents a single post entry in a paginated list view of forum posts.
/// </summary>
public sealed class PostListResponseEntry
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentPreview { get; set; } = string.Empty;
    public int VoteScore { get; set; }
    public DateTime LastModified { get; set; }
    public int Revisions { get; set; }
    public int CommentCount { get; set; }
}
