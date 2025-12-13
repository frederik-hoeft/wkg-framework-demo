namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Pagination details for paginated post list responses.
/// </summary>
/// <param name="CurrentPage">The current page number in the paginated result set.</param>
/// <param name="CurrentPageSize">The number of items per page.</param>
/// <param name="TotalPages">The total number of pages available.</param>
public readonly record struct PaginationInfo(int CurrentPage, int CurrentPageSize, int TotalPages);