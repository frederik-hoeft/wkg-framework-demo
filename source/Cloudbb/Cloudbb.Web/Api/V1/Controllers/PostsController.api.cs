using Asp.Versioning;
using Cloudbb.Web.Api.V1.Models.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cloudbb.Web.Api.V1.Controllers;

[Authorize]
[ApiController]
[ApiVersion(API_V1)]
[Route("api/v{version:apiVersion}/posts")]
public partial class PostsController
{
    /// <summary>
    /// Retrieves a list of all posts with optional timezone adjustment.
    /// </summary>
    /// <param name="tz">The IANA timezone identifier (e.g., <c>Europe/Berlin</c>).</param>
    /// <param name="tzoffset">Alternative timezone offset from UTC if <paramref name="tz"/> is not provided.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    [HttpGet]
    [ProducesResponseType<PostListResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> GetPostsAsync(string? tz = null, TimeSpan? tzoffset = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new post from the provided request data.
    /// </summary>
    /// <param name="request">The post creation request data.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    [HttpPost("create")]
    [ProducesResponseType<PostCreationResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> CreatePostAsync([FromBody] PostCreationRequest request, CancellationToken cancellationToken);
}
