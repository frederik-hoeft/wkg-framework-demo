namespace Cloudbb.Client.Models;

/// <summary>
/// Response containing a collection of forum posts for list/search operations.
/// </summary>
public sealed class PostListResponse
{
    public List<PostListResponseEntry> Posts { get; set; } = new();
    public PaginationInfo? PaginationInfo { get; set; }
}
