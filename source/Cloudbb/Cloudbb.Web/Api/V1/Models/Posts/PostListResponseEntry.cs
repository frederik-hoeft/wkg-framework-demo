namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Represents a single post entry in a paginated list view of forum posts.
/// Optimized for display in post listings with essential metadata and content preview.
/// </summary>
/// <param name="PostId">Unique identifier for the post.</param>
/// <param name="UserId">Unique identifier of the user who authored the post.</param>
/// <param name="Title">The post title for display in the listing.</param>
/// <param name="ContentPreview">Truncated preview of the post content for quick reading.</param>
/// <param name="VoteScore">Current voting score (upvotes minus downvotes) for post ranking.</param>
/// <param name="LastModified">Timestamp of the most recent modification to the post.</param>
/// <param name="Revisions">Total number of revisions made to the post. Will be at least 1.</param>
public sealed record PostListResponseEntry
(
    Guid PostId,
    Guid UserId,
    string Title,
    string ContentPreview,
    int VoteScore,
    DateTime LastModified,
    int Revisions
);