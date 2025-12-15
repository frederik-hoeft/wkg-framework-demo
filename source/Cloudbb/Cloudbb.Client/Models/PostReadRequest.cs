namespace Cloudbb.Client.Models;

/// <summary>
/// Request parameters for retrieving detailed information about a specific forum post.
/// </summary>
public sealed class PostReadRequest
{
    public Guid PostId { get; set; }
    public TimeZone TimeZone { get; set; } = new();
    public bool IncludeComments { get; set; } = true;
}
