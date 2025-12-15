namespace Cloudbb.Client.Models;

/// <summary>
/// Request payload for editing an existing post in the Cloudbb forum.
/// </summary>
public sealed class PostEditRequest
{
    public Guid PostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
