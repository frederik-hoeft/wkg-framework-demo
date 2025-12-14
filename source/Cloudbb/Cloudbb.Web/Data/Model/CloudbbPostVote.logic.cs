namespace Cloudbb.Web.Data.Model;

public partial class CloudbbPostVote : IVote<CloudbbPostVote>
{
    static CloudbbPostVote IVote<CloudbbPostVote>.Create(Guid userId, Guid targetId, int value) => new()
    {
        UserId = userId,
        PostId = targetId,
        Value = value
    };
}