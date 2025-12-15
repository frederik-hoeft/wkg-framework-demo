namespace Cloudbb.Client.Models;

/// <summary>
/// Request data for casting or updating a user's vote on a forum comment.
/// </summary>
public sealed class CommentVoteRequest
{
    public Guid CommentId { get; set; }
    public VoteType VoteType { get; set; }
}
