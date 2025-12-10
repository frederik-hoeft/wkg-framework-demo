using System.ComponentModel.DataAnnotations;

namespace Cloudbb.Web.Api.V1.Models.Posts;

public sealed record PostVoteRequest
(
    [Required] Guid PostId,
    [Required] VoteType VoteType
);