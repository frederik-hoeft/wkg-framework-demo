namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Response containing a collection of forum posts for list/search operations.
/// Used for paginated display of posts in the forum interface.
/// </summary>
/// <param name="Posts">Collection of post entries matching the request criteria.</param>
public record PostListResponse(List<PostListEntry> Posts);
