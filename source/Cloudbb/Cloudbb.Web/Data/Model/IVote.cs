namespace Cloudbb.Web.Data.Model;

/// <summary>
/// Factory interface for creating vote entities that track user preferences on forum content.
/// Enables type-safe creation of specific vote types (PostVote, CommentVote) with consistent initialization patterns.
/// Supports the community voting system by standardizing vote creation across different content types.
/// </summary>
/// <typeparam name="TSelf">The concrete vote type implementing this interface, enabling self-referential factory methods.</typeparam>
public interface IVote<TSelf> where TSelf : CloudbbVote, IVote<TSelf>
{
    /// <summary>
    /// Creates a new vote instance with the specified parameters.
    /// Provides a type-safe factory method for generating votes while maintaining proper entity relationships.
    /// </summary>
    /// <param name="userId">Unique identifier of the user casting the vote.</param>
    /// <param name="targetId">Unique identifier of the content being voted on (post, comment, etc.).</param>
    /// <param name="value">Numeric value of the vote: +1 for upvote, -1 for downvote.</param>
    /// <returns>A new vote instance properly initialized with the provided parameters.</returns>
    static abstract TSelf Create(Guid userId, Guid targetId, int value);
}