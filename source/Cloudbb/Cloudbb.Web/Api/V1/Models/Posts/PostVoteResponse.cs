namespace Cloudbb.Web.Api.V1.Models.Posts;

/// <summary>
/// Response containing updated vote information after a user casts or changes their vote.
/// Provides real-time feedback for immediate UI updates without requiring a full post refresh.
/// </summary>
/// <param name="PostId">Unique identifier of the voted post.</param>
/// <param name="NewScore">Updated aggregate vote score after applying the user's vote.</param>
/// <param name="UserVote">The user's current vote state after the operation.</param>
public sealed record PostVoteResponse
(
    Guid PostId,
    int NewScore,
    VoteType UserVote
);