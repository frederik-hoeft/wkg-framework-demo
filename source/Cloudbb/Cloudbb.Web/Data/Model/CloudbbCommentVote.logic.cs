namespace Cloudbb.Web.Data.Model;

public partial class CloudbbCommentVote : IVote<CloudbbCommentVote>
{
    static CloudbbCommentVote IVote<CloudbbCommentVote>.Create(Guid userId, Guid targetId, int value) => new()
    {
        UserId = userId,
        CommentId = targetId,
        Value = value
    };
}