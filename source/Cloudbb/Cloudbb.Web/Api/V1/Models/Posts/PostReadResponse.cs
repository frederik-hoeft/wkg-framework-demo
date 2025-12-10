using Cloudbb.Web.Api.V1.Models.Comments;

namespace Cloudbb.Web.Api.V1.Models.Posts;

public sealed record PostReadResponse
(
    Guid PostId,
    Guid UserId,
    string Username,
    string Title,
    string Content,
    int VoteScore,
    VoteType UserVote,
    DateTime LastModified,
    bool IsEdited,
    bool CanEdit,
    ICollection<CommentListResponseEntry>? Comments
);
