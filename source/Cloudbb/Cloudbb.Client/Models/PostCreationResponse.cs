namespace Cloudbb.Client.Models;

/// <summary>
/// Response returned after successfully creating a new forum post.
/// </summary>
public sealed class PostCreationResponse
{
    public Guid PostId { get; set; }
}
