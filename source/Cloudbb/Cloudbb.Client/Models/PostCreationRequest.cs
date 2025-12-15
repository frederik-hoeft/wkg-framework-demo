namespace Cloudbb.Client.Models;

/// <summary>
/// Request payload for creating a new post in the Cloudbb forum.
/// </summary>
public sealed class PostCreationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
