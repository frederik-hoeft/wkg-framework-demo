namespace Cloudbb.Client.Models;

/// <summary>
/// Request to permanently delete a forum post and all associated data.
/// </summary>
public sealed class PostDeleteRequest
{
    public Guid PostId { get; set; }
}
