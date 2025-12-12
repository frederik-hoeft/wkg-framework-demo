using Cloudbb.Web.Api.V1.Models;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;

namespace Cloudbb.Web.Api.V1.Extensions;

/// <summary>
/// Extension methods for implementing community voting functionality on forum content.
/// Provides standardized voting behavior for posts, comments, and other voteable entities in the Cloudbb forum system.
/// Handles vote creation, updates, and removal while maintaining data consistency and preventing duplicate votes.
/// </summary>
public static class VoteableExtensions
{
#pragma warning disable CA1034 // Nested types should not be visible
    // TODO: IntelliSense doesn't yet recognize C# 14 extension syntax
    extension<TVote>(IVoteable<TVote> voteable) where TVote : CloudbbVote, IVote<TVote>
#pragma warning restore CA1034 // Nested types should not be visible
    {
        /// <summary>
        /// Casts or updates a user's vote on a voteable forum entity (post, comment, etc.).
        /// Automatically handles vote state transitions: creating new votes, updating existing votes, or removing votes entirely.
        /// Ensures users cannot have multiple votes on the same content and maintains referential integrity in the database.
        /// </summary>
        /// <param name="dbContext">Entity Framework database context for persisting vote changes.</param>
        /// <param name="userId">Unique identifier of the user casting the vote.</param>
        /// <param name="vote">Type of vote being cast: Upvote (+1), Downvote (-1), or NoVote (removal).</param>
        /// <exception cref="ArgumentNullException">Thrown when dbContext is null.</exception>
        public void CastVote(CloudbbDbContext dbContext, Guid userId, VoteType vote)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            if (voteable.Votes is [var existingVote, ..])
            {
                // update existing vote
                if (vote == VoteType.NoVote)
                {
                    // remove vote
                    dbContext.Remove(existingVote);
                }
                else
                {
                    existingVote.Value = (int)vote;
                    dbContext.Update(existingVote);
                }
            }
            else
            {
                // create new vote
                TVote newVote = TVote.Create(userId, voteable.Id, (int)vote);
                dbContext.Add(newVote);
            }
        }
    }
}
