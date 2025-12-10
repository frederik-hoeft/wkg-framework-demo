namespace Cloudbb.Web.Api.V1.Models.Comments;

public sealed record CommentListResponseEntry
(
    Guid CommentId,
    Guid PostId,
    Guid UserId,
    string Username,
    int VoteScore,
    VoteType UserVote,
    string Content,
    DateTime CreationTime
);