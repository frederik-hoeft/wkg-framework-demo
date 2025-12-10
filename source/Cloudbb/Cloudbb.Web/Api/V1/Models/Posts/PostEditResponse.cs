namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Response confirming successful post edit operation.
/// Returns the post identifier for client-side navigation and state management.
/// </summary>
/// <param name="PostId">Unique identifier of the successfully edited post.</param>
public sealed record PostEditResponse(Guid PostId);
