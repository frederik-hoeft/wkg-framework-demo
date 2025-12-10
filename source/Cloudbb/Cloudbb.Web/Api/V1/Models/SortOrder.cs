namespace Cloudbb.Web.Api.V1.Models;

/// <summary>
/// Defines the sort order for paginated items
/// Controls whether results are displayed with newest/highest values first or oldest/lowest values first.
/// </summary>
public enum SortOrder
{
    /// <summary>
    /// Sort items in descending order (e.g., highest scores or most recent first).
    /// </summary>
    Descending,
    /// <summary>
    /// Sort items in ascending order (e.g., lowest scores or oldest first).
    /// </summary>
    Ascending
}