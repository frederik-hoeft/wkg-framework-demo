namespace Cloudbb.Web.Api.V1.Models.Comments;

/// <summary>
/// Represents a comment entry in a post's comment thread.
/// Optimized for display in comment lists with voting capabilities and author information.
/// </summary>
/// <param name="CommentId">Unique identifier of the comment.</param>
/// <param name="PostId">Unique identifier of the parent post.</param>
/// <param name="UserId">Unique identifier of the comment author.</param>
/// <param name="Username">Display name of the comment author.</param>
/// <param name="VoteScore">Aggregate vote score for comment quality ranking.</param>
/// <param name="UserVote">Current authenticated user's vote state on this comment.</param>
/// <param name="Content">Comment text content.</param>
/// <param name="CreationTime">Timestamp when the comment was originally posted.</param>
public sealed record CommentListResponseEntry
(
    Guid CommentId,
    Guid PostId,
    Guid UserId,
    string Username,
    int VoteScore,
    VoteType UserVote,
    string Content,
    DateTime CreationTime
);