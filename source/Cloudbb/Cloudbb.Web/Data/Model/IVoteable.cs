namespace Cloudbb.Web.Data.Model;

/// <summary>
/// Represents a forum entity that can receive community votes (upvotes and downvotes).
/// Implemented by posts, comments, and other user-generated content to enable democratic content curation.
/// Provides a standardized interface for managing vote collections and calculating aggregate scores.
/// </summary>
/// <typeparam name="TVote">The specific type of vote entity associated with this voteable content (e.g., PostVote, CommentVote).</typeparam>
public interface IVoteable<TVote> where TVote : CloudbbVote, IVote<TVote>
{
    /// <summary>
    /// Unique identifier of the voteable entity.
    /// Used to associate votes with their target content and prevent duplicate voting.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Collection of all votes cast on this entity by different users.
    /// Each user can have at most one vote per entity, enabling score calculation through vote aggregation.
    /// </summary>
    List<TVote> Votes { get; }
}
