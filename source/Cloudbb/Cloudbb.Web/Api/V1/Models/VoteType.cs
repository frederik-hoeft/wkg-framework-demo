namespace Cloudbb.Web.Api.V1.Models;

/// <summary>
/// Represents the type of vote a user can cast on posts and comments.
/// Values are stored as integers to simplify score calculations (sum of vote values).
/// </summary>
public enum VoteType : int
{
    /// <summary>
    /// Negative vote indicating disapproval or poor quality (-1).
    /// </summary>
    Downvote = -1,
    
    /// <summary>
    /// No vote or removed vote, neutral state (0).
    /// </summary>
    NoVote = 0,
    
    /// <summary>
    /// Positive vote indicating approval or high quality (+1).
    /// </summary>
    Upvote = 1,
}