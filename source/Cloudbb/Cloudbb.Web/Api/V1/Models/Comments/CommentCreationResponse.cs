namespace Cloudbb.Web.Api.V1.Models.Comments;

/// <summary>
/// Response data returned after successfully creating a new comment.
/// Provides the unique identifier of the newly created comment for client reference.
/// </summary>
/// <param name="CommentId">Unique identifier of the created comment.</param>
public sealed record CommentCreationResponse(Guid CommentId);