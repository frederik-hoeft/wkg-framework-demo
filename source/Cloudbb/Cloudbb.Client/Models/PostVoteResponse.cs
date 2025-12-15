namespace Cloudbb.Client.Models;

public sealed class PostVoteResponse
{
    public Guid PostId { get; set; }
    public int NewScore { get; set; }
    public VoteType UserVote { get; set; }
}