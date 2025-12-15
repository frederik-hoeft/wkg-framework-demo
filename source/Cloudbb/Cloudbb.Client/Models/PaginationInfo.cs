namespace Cloudbb.Client.Models;

/// <summary>
/// Pagination details for paginated post list responses.
/// </summary>
public sealed class PaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
