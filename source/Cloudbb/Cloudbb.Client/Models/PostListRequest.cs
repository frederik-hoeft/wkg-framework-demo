namespace Cloudbb.Client.Models;

/// <summary>
/// Request parameters for retrieving a paginated list of forum posts.
/// </summary>
public sealed class PostListRequest
{
    public TimeZone TimeZone { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public PostListSortMode SortMode { get; set; } = PostListSortMode.Activity;
    public SortOrder SortOrder { get; set; } = SortOrder.Descending;
}
