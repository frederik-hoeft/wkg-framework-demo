namespace Cloudbb.Client.Models;

/// <summary>
/// Represents the type of vote a user can cast on posts and comments.
/// </summary>
public enum VoteType
{
    NoVote = 0,
    Upvote = 1,
    Downvote = -1
}
