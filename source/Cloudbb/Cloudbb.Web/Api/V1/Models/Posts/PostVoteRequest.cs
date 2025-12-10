using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Request to cast or update a user's vote on a forum post.
/// Supports upvote, downvote, or vote removal for community-driven content ranking.
/// </summary>
public sealed class PostVoteRequest
{
    /// <summary>
    /// Unique identifier of the post to vote on.
    /// </summary>
    [Required] 
    public required Guid PostId { get; set; }

    /// <summary>
    /// Type of vote to cast (Upvote, Downvote, or NoVote to remove existing vote).
    /// </summary>
    [Required] 
    public required VoteType VoteType { get; set; }
}