namespace Cloudbb.Client.Models;

public sealed class CommentVoteResponse
{
    public Guid CommentId { get; set; }
    public int NewScore { get; set; }
    public VoteType UserVote { get; set; }
}