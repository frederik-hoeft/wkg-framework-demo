using Cloudbb.Client.Models;

namespace Cloudbb.Client.Services.Comments;

/// <summary>
/// Service for managing forum comments.
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Creates a new comment on a forum post.
    /// </summary>
    Task<CommentCreationResponse?> CreateCommentAsync(CommentCreationRequest request);

    /// <summary>
    /// Deletes a comment from a forum post.
    /// </summary>
    Task<bool> DeleteCommentAsync(CommentDeleteRequest request);

    /// <summary>
    /// Casts or updates a vote on a forum comment.
    /// </summary>
    Task<bool> VoteCommentAsync(CommentVoteRequest request);
}
