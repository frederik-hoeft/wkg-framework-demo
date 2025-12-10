using Cloudbb.Web.Api.V1.Models;
using Cloudbb.Web.Api.V1.Models.Posts;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Cloudbb.Web.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Transactions;

namespace Cloudbb.Web.Api.V1.Controllers;

/// <summary>
/// Provides endpoints for managing posts.
/// </summary>
public sealed partial class PostsController
(
    ITransactionServiceHandle transactionService,
    IUserClaimIndex userClaims
) : DatabaseController<ApplicationDbContext>(transactionService)
{
    public partial Task<IActionResult> GetPostsAsync(PostListRequest request, CancellationToken cancellationToken) => Transaction.Scoped.RunReadOnlyAsync<IActionResult>(async (dbContext, ct) =>
    {
        if (!TryValidateContext(request, out IActionResult? errorResult, out Guid _))
        {
            return errorResult;
        }
        TimeZoneInfo tzinfo = request.TimeZone.GetTimeZoneInfo();
        List<PostListResponseEntry> posts = await dbContext.Set<CloudbbPost>().AsNoTracking()
            // subselect to get latest revision per post
            .Select(post => new
            {
                Post = post,
                // invariant: posts always have at least one revision
                LatestRevision = post.Revisions.OrderByDescending(rev => rev.CreationTime).First(),
            })
            // construct result entries
            .Select(postInfo => new PostListResponseEntry
            (
                postInfo.Post.Id,
                postInfo.Post.UserId,
                postInfo.LatestRevision.Title,
                // limit content preview to 512 characters
                postInfo.LatestRevision.Content.Length <= 512
                    ? postInfo.LatestRevision.Content
                    : postInfo.LatestRevision.Content.Substring(0, 512),
                // post score is the sum of all vote values (+1 for upvote, -1 for downvote)
                postInfo.Post.Votes.Select(vote => vote.Value).Sum(),
                // convert database UTC time to provided timezone
                TimeZoneInfo.ConvertTimeFromUtc(postInfo.LatestRevision.CreationTime, tzinfo),
                // a post is considered edited if it has more than one revision
                postInfo.Post.Revisions.Count > 1
            ))
            .ToListAsync(ct);
        return Ok(new PostListResponse(posts));
    }, cancellationToken);

    public partial Task<IActionResult> GetPostAsync(PostReadRequest request, CancellationToken cancellationToken) => Transaction.Scoped.RunReadOnlyAsync(async (dbContext, ct) =>
    {
        if (!TryValidateContext(request, out IActionResult? errorResult, out Guid userId))
        {
            return errorResult;
        }
        TimeZoneInfo tzinfo = request.TimeZone.GetTimeZoneInfo();

        PostReadResponse? post = await dbContext.Set<CloudbbPost>().AsNoTracking()
            // subselect to get latest revision per post
            .Select(post => new
            {
                Post = post,
                // invariant: posts always have at least one revision
                LatestRevision = post.Revisions.OrderByDescending(rev => rev.CreationTime).First(),
            })
            .Where(postInfo => postInfo.Post.Id == request.PostId)
            // construct result
            .Select(postInfo => new PostReadResponse
            (
                postInfo.Post.Id,
                postInfo.Post.UserId,
                postInfo.Post.User.Username,
                postInfo.LatestRevision.Title,
                postInfo.LatestRevision.Content,
                // post score is the sum of all vote values (+1 for upvote, -1 for downvote)
                postInfo.Post.Votes.Select(vote => vote.Value).Sum(),
                // user's vote on this post (+1,-1), or None (0) if the user hasn't voted
                (VoteType)postInfo.Post.Votes
                    .Where(vote => vote.UserId == userId)
                    .Select(vote => vote.Value)
                    .FirstOrDefault(),
                // convert database UTC time to provided timezone
                TimeZoneInfo.ConvertTimeFromUtc(postInfo.LatestRevision.CreationTime, tzinfo),
                // a post is considered edited if it has more than one revision
                postInfo.Post.Revisions.Count > 1,
                // user can edit if they are the author of the post
                postInfo.Post.UserId == userId,
                request.IncludeComments
                    ? postInfo.Post.Comments.Select(comment => new CommentListResponseEntry
                    (
                        comment.Id,
                        comment.PostId,
                        comment.UserId,
                        comment.User.Username,
                        // comment score is the sum of all vote values (+1 for upvote, -1 for downvote)
                        comment.Votes.Select(vote => vote.Value).Sum(),
                        // user's vote on this comment (+1,-1), or None (0) if the user hasn't voted
                        (VoteType)comment.Votes
                            .Where(vote => vote.UserId == userId)
                            .Select(vote => vote.Value)
                            .FirstOrDefault(),
                        comment.Content,
                        // convert database UTC time to provided timezone
                        TimeZoneInfo.ConvertTimeFromUtc(comment.CreationTime, tzinfo)
                    )).ToList()
                    : null
            ))
            .FirstOrDefaultAsync(ct);
        if (post is null)
        {
            return NotFound();
        }
        return Ok(post);
    }, cancellationToken);

    public partial Task<IActionResult> CreatePostAsync(PostCreationRequest request, CancellationToken cancellationToken) => Transaction.Scoped.RunAsync(async (dbContext, transaction, ct) =>
    {
        if (!TryValidateContext(request, out IActionResult? errorResult, out Guid userId))
        {
            return transaction.Rollback(errorResult);
        }
        CloudbbPost post = new()
        {
            Revisions = [new CloudbbPostRevision(request.Title, request.Content)],
            UserId = userId
        };
        dbContext.Add(post);
        await dbContext.SaveChangesAsync(ct);

        PostCreationResponse response = new(post.Id);
        return transaction.Commit(Ok(response));
    }, cancellationToken);

    public partial Task<IActionResult> EditPostAsync(PostEditRequest request, CancellationToken cancellationToken) => Transaction.Scoped.RunAsync(async (dbContext, transaction, ct) =>
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
        if (post.UserId != userId)
        {
            return transaction.Rollback(Forbid());
        }
        CloudbbPostRevision revision = new(request.Title, request.Content)
        {
            PostId = post.Id
        };
        dbContext.Add(revision);
        await dbContext.SaveChangesAsync(ct);
        PostEditResponse result = new(post.Id);
        return transaction.Commit(Ok(result));
    }, cancellationToken);

    public partial Task<IActionResult> DeletePostAsync(PostDeleteRequest request, CancellationToken cancellationToken) => Transaction.Scoped.RunAsync(async (dbContext, transaction, ct) =>
    {
        if (!TryValidateContext(request, out IActionResult? errorResult, out Guid userId))
        {
            return transaction.Rollback(errorResult);
        }
        CloudbbPost? post = await dbContext.Set<CloudbbPost>()
            .FirstOrDefaultAsync(p => p.Id == request.PostId, ct);
        if (post is null)
        {
            return transaction.Rollback(NotFound());
        }
        if (post.UserId != userId)
        {
            return transaction.Rollback(Forbid());
        }
        // cascade delete will handle related revisions, comments, votes, etc.
        dbContext.Remove(post);
        await dbContext.SaveChangesAsync(ct);
        return transaction.Commit(Ok());
    }, cancellationToken);

    public partial Task<IActionResult> VotePostAsync(PostVoteRequest request, CancellationToken cancellationToken) => Transaction.Scoped.RunAsync(async (dbContext, transaction, ct) =>
    {
        if (!TryValidateContext(request, out IActionResult? errorResult, out Guid userId))
        {
            return transaction.Rollback(errorResult);
        }
        CloudbbPost? post = await dbContext.Set<CloudbbPost>()
            .Include(p => p.Votes.Where(vote => vote.UserId == userId))
            .FirstOrDefaultAsync(p => p.Id == request.PostId, ct);
        if (post is null)
        {
            return transaction.Rollback(NotFound());
        }
        if (post.Votes is [var existingVote, ..])
        {
            // update existing vote
            if (request.VoteType == VoteType.NoVote)
            {
                // remove vote
                dbContext.Remove(existingVote);
            }
            else
            {
                existingVote.Value = (int)request.VoteType;
                dbContext.Update(existingVote);
            }
        }
        else
        {
            // create new vote
            CloudbbPostVote newVote = new()
            {
                PostId = post.Id,
                UserId = userId,
                Value = (int)request.VoteType
            };
            dbContext.Add(newVote);
        }
        await dbContext.SaveChangesAsync(ct);
        PostVoteResponse? response = await dbContext.Set<CloudbbPost>().AsNoTracking()
            .Where(p => p.Id == post.Id)
            .Select(p => new PostVoteResponse
            (
                p.Id,
                // recalculate post score
                p.Votes.Select(vote => vote.Value).Sum(),
                // just fully refresh from database as a source of ground truth
                // user's vote on this post (+1,-1), or None (0) if the user hasn't voted
                (VoteType)p.Votes
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

    private bool TryValidateContext<TRequest>([NotNullWhen(true)] TRequest? request, [NotNullWhen(false)] out IActionResult? errorResult, out Guid userId)
    {
        errorResult = null;
        userId = Guid.Empty;
        if (request is null)
        {
            errorResult = BadRequest("Request body cannot be null");
            return false;
        }
        if (!ModelState.IsValid)
        {
            errorResult = BadRequest(ModelState);
            return false;
        }
        if (!userClaims.TryGetUserId(out userId))
        {
            // honestly, should never happen. the auth middleware should catch this
            errorResult = Unauthorized();
            return false;
        }
        return true;
    }
}
