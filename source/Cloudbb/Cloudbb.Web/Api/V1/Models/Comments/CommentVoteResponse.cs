namespace Cloudbb.Web.Api.V1.Models.Comments;

/// <summary>
/// Response data returned after a user successfully votes on a comment.
/// Provides immediate feedback about the vote's impact on the comment's score and the user's current voting state.
/// </summary>
/// <param name="CommentId">Unique identifier of the voted comment, useful for UI updates.</param>
/// <param name="NewScore">Updated aggregate score of the comment after applying the user's vote, calculated from all community votes.</param>
/// <param name="UserVote">The authenticated user's current vote state on this comment after the operation (Upvote, Downvote, or NoVote).</param>
public sealed record CommentVoteResponse
(
    Guid CommentId,
    int NewScore,
    VoteType UserVote
);