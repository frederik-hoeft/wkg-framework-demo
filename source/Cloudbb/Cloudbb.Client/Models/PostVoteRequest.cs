namespace Cloudbb.Client.Models;

/// <summary>
/// Request to cast or update a user's vote on a forum post.
/// </summary>
public sealed class PostVoteRequest
{
    public Guid PostId { get; set; }
    public VoteType VoteType { get; set; }
}
