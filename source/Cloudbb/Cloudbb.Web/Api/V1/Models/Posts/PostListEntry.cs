namespace Cloudbb.Web.Api.V1.Models.Posts;

public record PostListEntry
(
    Guid PostId,
    Guid UserId,
    string Title,
    string ContentPreview,
    int Score,
    DateTime LastModified,
    bool IsEdited
);