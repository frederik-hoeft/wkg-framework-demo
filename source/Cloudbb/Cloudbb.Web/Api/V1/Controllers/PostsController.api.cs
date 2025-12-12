using Asp.Versioning;
using Cloudbb.Web.Api.V1.Models.Posts;
using Cloudbb.Web.Services.Auth.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cloudbb.Web.Api.V1.Controllers;

[ApiController]
[ApiVersion(API_V1)]
[Authorize(AuthRoles.USER)]
[Route("api/v{version:apiVersion}/posts")]
public partial class PostsController
{
    /// <summary>
    /// Retrieves a paginated list of forum posts with voting scores and metadata.
    /// Posts are ordered by popularity and activity for optimal user engagement.
    /// </summary>
    /// <param name="request">Pagination and timezone parameters for post listing.</param>
    /// <param name="cancellationToken">Cancellation token for request handling.</param>
    /// <returns>Paginated collection of post summaries with vote scores.</returns>
    /// <response code="400">The parameters provided in the request are invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    [HttpPost("list")]
    [ProducesResponseType<PostListResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> GetPostsAsync([FromBody] PostListRequest request, CancellationToken cancellationToken);
    
    /// <summary>
    /// Retrieves complete details for a specific forum post, including content and optional comments.
    /// Includes user voting state and edit permissions for the authenticated user.
    /// </summary>
    /// <param name="request">Post identifier and display options including timezone and comment inclusion.</param>
    /// <param name="cancellationToken">Cancellation token for request handling.</param>
    /// <returns>Complete post data with content, metadata, and optional comment thread.</returns>
    /// <response code="400">The parameters provided in the request are invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="404">A post with the specified ID does not exist.</response>
    [HttpPost("read")]
    [ProducesResponseType<PostReadResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> GetPostAsync([FromBody] PostReadRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new forum post with the provided title and content.
    /// Automatically assigns the authenticated user as the author and creates the initial revision.
    /// </summary>
    /// <param name="request">Post title and content for creation.</param>
    /// <param name="cancellationToken">Cancellation token for request handling.</param>
    /// <returns>Created post identifier for client navigation.</returns>
    /// <response code="400">The parameters provided in the request are invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="404">A post with the specified ID does not exist.</response>
    [HttpPost("create")]
    [ProducesResponseType<PostCreationResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> CreatePostAsync([FromBody] PostCreationRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing forum post with new title and/or content.
    /// Creates a new revision for edit history tracking and validates user permissions.
    /// </summary>
    /// <param name="request">Post identifier and updated content.</param>
    /// <param name="cancellationToken">Cancellation token for request handling.</param>
    /// <returns>Confirmation of successful edit operation.</returns>
    /// <response code="400">The parameters provided in the request are invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to edit this post.</response>
    /// <response code="404">A post with the specified ID does not exist.</response>
    [HttpPost("edit")]
    [ProducesResponseType<PostEditResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> EditPostAsync([FromBody] PostEditRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Permanently deletes a forum post and all associated data (comments, votes, revisions).
    /// Requires post ownership or administrative privileges for execution.
    /// </summary>
    /// <param name="request">Post identifier for deletion.</param>
    /// <param name="cancellationToken">Cancellation token for request handling.</param>
    /// <returns>Confirmation of successful deletion.</returns>
    /// <response code="400">The parameters provided in the request are invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to delete this post.</response>
    /// <response code="404">A post with the specified ID does not exist.</response>
    [HttpPost("delete")]
    public partial Task<IActionResult> DeletePostAsync([FromBody] PostDeleteRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Casts or updates a user's vote (upvote, downvote, or remove vote) on a forum post.
    /// Updates the post's aggregate score and tracks the user's current voting state.
    /// </summary>
    /// <param name="request">Post identifier and desired vote type.</param>
    /// <param name="cancellationToken">Cancellation token for request handling.</param>
    /// <returns>Updated post score and user's current vote state.</returns>
    /// <response code="400">The parameters provided in the request are invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to vote on this post.</response>
    /// <response code="404">A post with the specified ID does not exist.</response>
    [HttpPost("vote")]
    [ProducesResponseType<PostVoteResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> VotePostAsync([FromBody] PostVoteRequest request, CancellationToken cancellationToken);
}
