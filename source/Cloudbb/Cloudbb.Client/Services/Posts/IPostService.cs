using Cloudbb.Client.Models;

namespace Cloudbb.Client.Services.Posts;

/// <summary>
/// Service for managing forum posts.
/// </summary>
public interface IPostService
{
    /// <summary>
    /// Retrieves a paginated list of forum posts.
    /// </summary>
    Task<PostListResponse?> GetPostsAsync(PostListRequest request);

    /// <summary>
    /// Retrieves detailed information about a specific post.
    /// </summary>
    Task<PostReadResponse?> GetPostAsync(PostReadRequest request);

    /// <summary>
    /// Creates a new forum post.
    /// </summary>
    Task<PostCreationResponse?> CreatePostAsync(PostCreationRequest request);

    /// <summary>
    /// Edits an existing forum post.
    /// </summary>
    Task<PostEditResponse?> EditPostAsync(PostEditRequest request);

    /// <summary>
    /// Deletes a forum post.
    /// </summary>
    Task<bool> DeletePostAsync(PostDeleteRequest request);

    /// <summary>
    /// Casts or updates a vote on a forum post.
    /// </summary>
    Task<PostVoteResponse?> VotePostAsync(PostVoteRequest request);
}
