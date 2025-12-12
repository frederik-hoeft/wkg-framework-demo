using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Comments;

/// <summary>
/// Request data for casting or updating a user's vote on a forum comment.
/// Supports upvoting, downvoting, or removing an existing vote to enable community-driven content curation.
/// </summary>
public sealed class CommentVoteRequest
{
    /// <summary>
    /// Unique identifier of the comment being voted on.
    /// Must reference an existing comment that the user has not authored (users cannot vote on their own content).
    /// </summary>
    [Required] 
    public required Guid CommentId { get; set; }

    /// <summary>
    /// The type of vote being cast on the comment.
    /// Upvote increases the comment's score, Downvote decreases it, and NoVote removes any existing vote.
    /// </summary>
    [Required] 
    public required VoteType VoteType { get; set; }
}