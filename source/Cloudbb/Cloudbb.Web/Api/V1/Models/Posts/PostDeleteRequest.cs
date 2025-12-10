using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Request to permanently delete a forum post and all associated data.
/// Requires post ownership or administrative privileges for successful execution.
/// </summary>
public sealed class PostDeleteRequest
{
    /// <summary>
    /// Unique identifier of the post to delete.
    /// </summary>
    [Required] 
    public required Guid PostId { get; set; }
}