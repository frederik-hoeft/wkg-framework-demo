using Cloudbb.Web.Api.V1.Models.Comments;

namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Complete post details response including content, metadata, voting information, and optional comments.
/// Provides all data needed for displaying a full post view with user interaction capabilities.
/// </summary>
/// <param name="PostId">Unique identifier of the post.</param>
/// <param name="UserId">Unique identifier of the post author.</param>
/// <param name="Username">Display name of the post author.</param>
/// <param name="Title">Current post title from the latest revision.</param>
/// <param name="Content">Current post content from the latest revision.</param>
/// <param name="VoteScore">Aggregate vote score (sum of all upvotes and downvotes).</param>
/// <param name="UserVote">Current authenticated user's vote state on this post.</param>
/// <param name="LastModified">Timestamp of the most recent modification (creation or edit).</param>
/// <param name="Revisions">Total number of revisions made to this post. Will be at least 1.</param>
/// <param name="CanEdit">Whether the authenticated user has permission to edit this post. If false, edit options should be hidden and voting be enabled (cannot vote on own post).</param>
/// <param name="Comments">Collection of comments on this post, included only when requested.</param>
public sealed record PostReadResponse
(
    Guid PostId,
    Guid UserId,
    string Username,
    string Title,
    string Content,
    int VoteScore,
    VoteType UserVote,
    DateTime LastModified,
    int Revisions,
    bool CanEdit,
    ICollection<CommentResponseEntry>? Comments
);
