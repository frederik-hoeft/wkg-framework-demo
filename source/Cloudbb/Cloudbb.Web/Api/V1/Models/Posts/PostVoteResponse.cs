namespace Cloudbb.Web.Api.V1.Models.Posts;

public sealed record PostVoteResponse
(
    Guid PostId,
    int NewScore,
    VoteType UserVote
);