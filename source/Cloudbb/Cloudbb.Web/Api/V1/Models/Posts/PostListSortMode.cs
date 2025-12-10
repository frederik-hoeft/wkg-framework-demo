namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Defines the sorting criteria for forum post listings.
/// Enables users to view posts ranked by community engagement or chronological activity.
/// </summary>
public enum PostListSortMode
{
    /// <summary>
    /// Sort posts by popularity (vote score), then by last modified date.
    /// </summary>
    Score,
    /// <summary>
    /// Sort posts by most recent activity (last modified date).
    /// </summary>
    Activity
}