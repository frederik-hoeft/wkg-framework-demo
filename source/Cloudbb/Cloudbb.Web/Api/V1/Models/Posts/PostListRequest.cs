using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Request parameters for retrieving a paginated list of forum posts.
/// Supports timezone-aware display, configurable page sizes, and flexible sorting for optimal user experience.
/// </summary>
public sealed class PostListRequest
{
    /// <summary>
    /// User's timezone for proper timestamp localization.
    /// </summary>
    public TimeZone? TimeZone { get; set; }

    /// <summary>
    /// Page number to retrieve (1-based indexing, minimum 1).
    /// </summary>
    [DefaultValue(1)]
    [Range(1, int.MaxValue)] 
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of posts per page (minimum 1, default 20 for optimal loading).
    /// </summary>
    [DefaultValue(20)]
    [Range(1, int.MaxValue)] 
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sorting criteria for post ordering (Score for popularity, Activity for recency).
    /// </summary>
    [DefaultValue("score")] 
    public PostListSortMode SortMode { get; set; } = PostListSortMode.Score;

    /// <summary>
    /// Sort direction (Descending for newest/highest first, Ascending for oldest/lowest first).
    /// </summary>
    [DefaultValue("descending")] 
    public SortOrder SortOrder { get; set; } = SortOrder.Descending;
}