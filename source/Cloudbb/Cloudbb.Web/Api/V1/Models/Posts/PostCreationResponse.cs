namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Response returned after successfully creating a new forum post.
/// Contains the unique identifier that can be used to reference the created post.
/// </summary>
/// <param name="PostId">The unique identifier of the newly created post.</param>
public record PostCreationResponse(Guid PostId);