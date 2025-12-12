using Asp.Versioning;
using Cloudbb.Web.Api.V1.Models.Comments;
using Cloudbb.Web.Services.Auth.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cloudbb.Web.Api.V1.Controllers;

/// <summary>
/// RESTful API controller for managing forum comment operations including creation, deletion, and user voting.
/// All endpoints require user authentication and operate within transactional contexts for data consistency.
/// Comments are associated with posts and support threaded discussions with community voting features.
/// </summary>
[ApiController]
[ApiVersion(API_V1)]
[Authorize(AuthRoles.USER)]
[Route("api/v{version:apiVersion}/comments")]
public partial class CommentsController
{
    /// <summary>
    /// Creates a new comment on an existing forum post.
    /// Validates post existence, applies content length restrictions, and associates the comment with the authenticated user.
    /// Comments are immediately visible to all users and contribute to the post's discussion thread.
    /// </summary>
    /// <param name="request">Comment creation data including target post ID and comment content text.</param>
    /// <param name="cancellationToken">Cancellation token for request handling and database operations.</param>
    /// <returns>Created comment details including unique identifier and creation timestamp.</returns>
    /// <response code="200">Comment successfully created and saved to the database.</response>
    /// <response code="400">Invalid request data such as missing content or invalid post reference.</response>
    /// <response code="401">User is not authenticated or authentication token is invalid.</response>
    /// <response code="404">Target post does not exist or has been deleted.</response>
    [HttpPost("create")]
    [ProducesResponseType<CommentResponseEntry>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> CreateCommentAsync([FromBody] CommentCreationRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Permanently deletes a comment from the forum.
    /// Only the original comment author can delete their own comments. Deletion cascades to remove associated votes and references.
    /// This operation cannot be undone and removes the comment content from all discussion threads.
    /// </summary>
    /// <param name="request">Comment deletion request containing the unique identifier of the comment to remove.</param>
    /// <param name="cancellationToken">Cancellation token for request handling and database operations.</param>
    /// <returns>Confirmation of successful deletion or appropriate error response.</returns>
    /// <response code="200">Comment successfully deleted and removed from the database.</response>
    /// <response code="400">Invalid request data or malformed comment identifier.</response>
    /// <response code="401">User is not authenticated or authentication token is invalid.</response>
    /// <response code="403">User does not have permission to delete this comment (not the original author).</response>
    /// <response code="404">Comment does not exist or has already been deleted.</response>
    [HttpPost("delete")]
    public partial Task<IActionResult> DeleteCommentAsync([FromBody] CommentDeleteRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Casts or updates a user's vote on a forum comment (upvote, downvote, or remove vote).
    /// Users can change their vote at any time but cannot vote on their own comments. Vote changes are reflected immediately
    /// in the comment's aggregate score and influence community-driven content ranking.
    /// </summary>
    /// <param name="request">Voting request containing comment identifier and desired vote type (upvote/downvote/none).</param>
    /// <param name="cancellationToken">Cancellation token for request handling and database operations.</param>
    /// <returns>Updated comment voting information including new aggregate score and user's current vote state.</returns>
    /// <response code="200">Vote successfully processed and comment score updated.</response>
    /// <response code="400">Invalid request data or malformed vote parameters.</response>
    /// <response code="401">User is not authenticated or authentication token is invalid.</response>
    /// <response code="403">User cannot vote on their own comment or lacks voting permissions.</response>
    /// <response code="404">Comment does not exist or has been deleted.</response>
    [HttpPost("vote")]
    [ProducesResponseType<CommentVoteResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> VoteCommentAsync([FromBody] CommentVoteRequest request, CancellationToken cancellationToken);
}
