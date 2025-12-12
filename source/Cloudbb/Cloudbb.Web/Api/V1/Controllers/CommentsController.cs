using Cloudbb.Web.Api.V1.Extensions;
using Cloudbb.Web.Api.V1.Models;
using Cloudbb.Web.Api.V1.Models.Comments;
using Cloudbb.Web.Data.Model;
using Cloudbb.Web.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Transactions;

namespace Cloudbb.Web.Api.V1.Controllers;

/// <summary>
/// Initializes a new instance of the CommentsController with required dependencies.
/// </summary>
/// <param name="transactionService">Database transaction service for ensuring data consistency across comment operations.</param>
/// <param name="userClaims">Service for accessing authenticated user information and validating user permissions.</param>
public sealed partial class CommentsController(ITransactionServiceHandle transactionService, IUserClaimIndex userClaims) 
    : CloudbbControllerBase(transactionService, userClaims)
{
    public partial Task<IActionResult> CreateCommentAsync(CommentCreationRequest request, CancellationToken cancellationToken) => Transaction.Scoped.RunAsync(async (dbContext, transaction, ct) =>
    {
        if (!TryValidateContext(request, out IActionResult? errorResult, out Guid userId))
        {
            return transaction.Rollback(errorResult);
        }
        CloudbbPost? post = await dbContext.Set<CloudbbPost>().AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PostId, ct);
        if (post is null)
        {
            return transaction.Rollback(NotFound());
        }
        CloudbbComment comment = new()
        {
            PostId = post.Id,
            UserId = userId,
            Content = request.Content,
        };
        dbContext.Add(comment);
        await dbContext.SaveChangesAsync(ct);
        // no need to re-fetch from the database since we have all the info we need
        CommentResponseEntry result = new
        (
            comment.Id,
            comment.PostId,
            comment.UserId,
            comment.Content,
            UserClaims.GetUsername(),
            VoteScore: 0,
            UserVote: VoteType.NoVote,
            CanEdit: true,
            comment.CreationTime
        );
        return transaction.Commit(Ok(result));
    }, cancellationToken);

    public partial Task<IActionResult> DeleteCommentAsync(CommentDeleteRequest request, CancellationToken cancellationToken) => Transaction.Scoped.RunAsync(async (dbContext, transaction, ct) =>
    {
        if (!TryValidateContext(request, out IActionResult? errorResult, out Guid userId))
        {
            return transaction.Rollback(errorResult);
        }
        CloudbbComment? comment = await dbContext.Set<CloudbbComment>().AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.CommentId, ct);
        if (comment is null)
        {
            return transaction.Rollback(NotFound());
        }
        if (comment.UserId != userId)
        {
            return transaction.Rollback(Forbid());
        }
        dbContext.Remove(comment);
        await dbContext.SaveChangesAsync(ct);
        return transaction.Commit(Ok());
    }, cancellationToken);

    public partial Task<IActionResult> VoteCommentAsync(CommentVoteRequest request, CancellationToken cancellationToken) => Transaction.Scoped.RunAsync(async (dbContext, transaction, ct) =>
    {
        if (!TryValidateContext(request, out IActionResult? errorResult, out Guid userId))
        {
            return transaction.Rollback(errorResult);
        }
        // get the comment along with any existing vote by this user
        CloudbbComment? comment = await dbContext.Set<CloudbbComment>()
            .Include(p => p.Votes.Where(vote => vote.UserId == userId))
            .FirstOrDefaultAsync(p => p.Id == request.CommentId, ct);
        if (comment is null)
        {
            return transaction.Rollback(NotFound());
        }
        if (comment.UserId == userId)
        {
            // users cannot vote on their own posts
            return transaction.Rollback(Forbid());
        }
        comment.CastVote(dbContext, userId, request.VoteType);
        await dbContext.SaveChangesAsync(ct);
        // just fully refresh from database as a source of ground truth
        CommentVoteResponse? response = await dbContext.Set<CloudbbComment>().AsNoTracking()
            .Where(c => c.Id == comment.Id)
            .Select(c => new CommentVoteResponse
            (
                c.Id,
                // recalculate post score
                c.Votes.Select(vote => vote.Value).Sum(),
                // user's vote on this post (+1,-1), or None (0) if the user hasn't voted
                (VoteType)c.Votes
                    .Where(vote => vote.UserId == userId)
                    .Select(vote => vote.Value)
                    .FirstOrDefault()
            ))
            // isolation level read committed => non-repeatable read possible, post may have been deleted
            .FirstOrDefaultAsync(ct);
        if (response is null)
        {
            return transaction.Rollback(NotFound());
        }
        return transaction.Commit(Ok(response));
    }, cancellationToken);
}