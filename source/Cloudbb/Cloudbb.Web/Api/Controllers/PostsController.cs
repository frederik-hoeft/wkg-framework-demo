using Cloudbb.Web.Api.Models.Posts;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Cloudbb.Web.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Transactions;

namespace Cloudbb.Web.Api.Controllers;

[Authorize]
[ApiController]
[Route("/api/posts")]
public sealed class PostsController
(
    ITransactionServiceHandle transactionService,
    IUserClaimIndex userClaims
) : DatabaseController<ApplicationDbContext>(transactionService)
{
    [HttpGet]
    // TODO: paging + search options
    public async Task<IActionResult> GetPostsAsync(string? tz = null, TimeSpan? tzoffset = null) => await Transaction.Scoped.RunReadOnlyAsync(async dbContext =>
    {
        TimeZoneInfo tzinfo = ParseUserLocalTime(tz, tzoffset);
        List<PostListEntry> posts = await dbContext.Set<CloudbbPost>()
            // subselect to get latest revision per post
            .Select(post => new
            {
                Post = post,
                // invariant: posts always have at least one revision
                LatestRevision = post.Revisions.OrderByDescending(rev => rev.CreationTime).First(),
            })
            // construct result entries
            .Select(postInfo => new PostListEntry
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
            .ToListAsync();
        return Ok(posts);
    });

    [HttpPost("create")]
    // TODO: add CancellationToken support to Wkg.AspNetCore transaction management
    public async Task<IActionResult> CreatePostAsync([FromBody] PostCreationRequest request) => await Transaction.Scoped.RunAsync(async (dbContext, transaction) =>
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ModelState.IsValid)
        {
            return transaction.Rollback(BadRequest(ModelState));
        }
        if (!userClaims.TryGetUserId(out Guid userId))
        {
            // honestly, should never happen. the auth middleware should catch this
            return transaction.Rollback(Unauthorized());
        }

        CloudbbPost post = new()
        {
            Revisions = [new CloudbbPostRevision(request.Title, request.Content)],
            UserId = userId
        };
        dbContext.Add(post);
        await dbContext.SaveChangesAsync();

        PostCreationResponse response = new(post.Id);
        return transaction.Commit(Ok(response));
    });

    private static TimeZoneInfo ParseUserLocalTime(string? tz = null, TimeSpan? tzoffset = null)
    {
        if (!string.IsNullOrEmpty(tz))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(tz);
            }
            catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException) { }
        }
        // normalize offset
        TimeSpan offset = TimeSpan.Zero;
        if (tzoffset is { } value)
        {
            offset = new TimeSpan(value.Hours, value.Minutes, 0);
        }
        if (offset == TimeSpan.Zero)
        {
            return TimeZoneInfo.Utc;
        }
        return TimeZoneInfo.CreateCustomTimeZone("Custom", offset, "Custom", "Custom");
    }
}
