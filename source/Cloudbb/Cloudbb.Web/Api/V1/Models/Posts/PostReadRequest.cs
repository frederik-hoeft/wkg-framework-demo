using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Request parameters for retrieving detailed information about a specific forum post.
/// Allows optional inclusion of comments to reduce payload size when comments aren't needed.
/// </summary>
public sealed class PostReadRequest
{
    /// <summary>
    /// Unique identifier of the post to retrieve.
    /// </summary>
    [Required] 
    public required Guid PostId { get; set; }

    /// <summary>
    /// User's timezone for proper timestamp localization in the response.
    /// </summary>
    [Required] 
    public required TimeZone TimeZone { get; set; }

    /// <summary>
    /// Whether to include the comment thread with the post data.
    /// </summary>
    [DefaultValue(false)]
    public bool IncludeComments { get; set; }
}