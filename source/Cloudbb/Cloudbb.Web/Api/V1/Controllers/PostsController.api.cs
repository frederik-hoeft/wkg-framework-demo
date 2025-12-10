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
    [HttpPost("list")]
    [ProducesResponseType<PostListResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> GetPostsAsync([FromBody] PostListRequest request, CancellationToken cancellationToken);
    
    [HttpPost("read")]
    [ProducesResponseType<PostReadResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> GetPostAsync([FromBody] PostReadRequest request, CancellationToken cancellationToken);

    [HttpPost("create")]
    [ProducesResponseType<PostCreationResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> CreatePostAsync([FromBody] PostCreationRequest request, CancellationToken cancellationToken);

    [HttpPost("edit")]
    [ProducesResponseType<PostEditResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> EditPostAsync([FromBody] PostEditRequest request, CancellationToken cancellationToken);

    [HttpPost("delete")]
    public partial Task<IActionResult> DeletePostAsync([FromBody] PostDeleteRequest request, CancellationToken cancellationToken);

    [HttpPost("vote")]
    [ProducesResponseType<PostVoteResponse>(StatusCodes.Status200OK)]
    public partial Task<IActionResult> VotePostAsync([FromBody] PostVoteRequest request, CancellationToken cancellationToken);
}
