using Cloudbb.Web.Api.V1.Controllers;
using Cloudbb.Web.Api.V1.Models;
using Cloudbb.Web.Api.V1.Models.Posts;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Cloudbb.Web.Tests.Integration.Api.V1.Posts;

[TestClass]
public sealed class PostsController_VotePostTests : ControllerBaseTest<PostsController>
{
    public override TestContext TestContext { get; set; }

    protected async override ValueTask InitializeComponentAsync(PostsController component, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await base.InitializeComponentAsync(component, serviceProvider, cancellationToken);
        await SetAuthenticatedUserContextAsync(component, serviceProvider, IntegrationTestDbLoader.TestUser1, cancellationToken);
    }

    [TestMethod]
    public Task VotePostAsync_WithUpvote_ShouldAddUpvoteAsync() => UsingTransactionAsync(async (dbContext, ct) =>
    {
        CloudbbPost? post = await dbContext.Set<CloudbbPost>()
            .Include(p => p.Votes)
            .FirstOrDefaultAsync(p => p.Id == IntegrationTestDbLoader.PostOfUser2Id, ct);
        Assert.IsNotNull(post);

        int initialScore = post.Votes.Sum(v => v.Value);

        await UsingComponentAsync(new PostVoteRequest()
        {
            PostId = post.Id,
            VoteType = VoteType.Upvote
        }, async (controller, request, serviceProvider, ct1) =>
        {
            // Act
            IActionResult result = await controller.VotePostAsync(request, ct1);

            // Assert
            Assert.IsNotNull(result);
            OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
            PostVoteResponse response = Assert.IsInstanceOfType<PostVoteResponse>(ok.Value);
            
            Assert.AreEqual(post.Id, response.PostId);
            Assert.AreEqual(VoteType.Upvote, response.UserVote);
            Assert.AreEqual(initialScore + 1, response.NewScore); // Score should increase by 1

            // Verify vote was saved in database
            CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();

            CloudbbPostVote? vote = await dbContextCheck.Set<CloudbbPostVote>()
                .FirstOrDefaultAsync(v => v.PostId == post.Id && v.UserId == IntegrationTestDbLoader.TestUser1.UserId, ct1);
            
            Assert.IsNotNull(vote);
            Assert.AreEqual(1, vote.Value); // Upvote = +1
        }, ct);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VotePostAsync_WithDownvote_ShouldAddDownvoteAsync() => UsingTransactionAsync(async (dbContext, ct) =>
    {
        // Arrange - Get a post NOT owned by the current user
        CloudbbPost? post = await dbContext.Set<CloudbbPost>()
            .Include(p => p.Votes)
            .FirstOrDefaultAsync(p => p.Id == IntegrationTestDbLoader.PostOfUser2Id, ct);
        Assert.IsNotNull(post);

        int initialScore = post.Votes.Sum(v => v.Value);

        await UsingComponentAsync(new PostVoteRequest()
        {
            PostId = post.Id,
            VoteType = VoteType.Downvote
        }, async (controller, request, serviceProvider, ct1) =>
        {
            // Act
            IActionResult result = await controller.VotePostAsync(request, ct1);

            // Assert
            Assert.IsNotNull(result);
            OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
            PostVoteResponse response = Assert.IsInstanceOfType<PostVoteResponse>(ok.Value);
            
            Assert.AreEqual(post.Id, response.PostId);
            Assert.AreEqual(VoteType.Downvote, response.UserVote);
            Assert.AreEqual(initialScore - 1, response.NewScore); // Score should decrease by 1

            // Verify vote was saved in database
            CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();

            CloudbbPostVote? vote = await dbContextCheck.Set<CloudbbPostVote>()
                .FirstOrDefaultAsync(v => v.PostId == post.Id && v.UserId == IntegrationTestDbLoader.TestUser1.UserId, ct1);
            
            Assert.IsNotNull(vote);
            Assert.AreEqual(-1, vote.Value); // Downvote = -1
        }, ct);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VotePostAsync_ChangeVoteFromUpvoteToDownvote_ShouldUpdateVoteAsync() => UsingTransactionAsync(async (dbContext, ct) =>
    {
        // Arrange - Create an upvote first
        CloudbbPost? post = await dbContext.Set<CloudbbPost>()
            .FirstOrDefaultAsync(p => p.Id == IntegrationTestDbLoader.PostOfUser2Id, ct);
        Assert.IsNotNull(post);

        // Add initial upvote
        CloudbbPostVote initialVote = new()
        {
            PostId = post.Id,
            UserId = IntegrationTestDbLoader.TestUser1.UserId,
            Value = 1
        };
        dbContext.Add(initialVote);
        await dbContext.SaveChangesAsync(ct);

        await UsingComponentAsync(new PostVoteRequest()
        {
            PostId = post.Id,
            VoteType = VoteType.Downvote
        }, async (controller, request, serviceProvider, ct1) =>
        {
            // Act
            IActionResult result = await controller.VotePostAsync(request, ct1);

            // Assert
            Assert.IsNotNull(result);
            OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
            PostVoteResponse response = Assert.IsInstanceOfType<PostVoteResponse>(ok.Value);
            
            Assert.AreEqual(VoteType.Downvote, response.UserVote);

            // Verify vote was updated in database
            CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
            CloudbbPostVote? updatedVote = await dbContextCheck.Set<CloudbbPostVote>()
                .FirstOrDefaultAsync(v => v.PostId == post.Id && v.UserId == IntegrationTestDbLoader.TestUser1.UserId, ct1);
            
            Assert.IsNotNull(updatedVote);
            Assert.AreEqual(-1, updatedVote.Value); // Should be updated to downvote
        }, ct);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VotePostAsync_RemoveVote_ShouldDeleteVoteAsync() => UsingTransactionAsync(async (dbContext, ct) =>
    {
        // Arrange - Create a vote first
        CloudbbPost? post = await dbContext.Set<CloudbbPost>()
            .FirstOrDefaultAsync(p => p.Id == IntegrationTestDbLoader.PostOfUser2Id, ct);
        Assert.IsNotNull(post);

        // Add initial vote
        CloudbbPostVote initialVote = new()
        {
            PostId = post.Id,
            UserId = IntegrationTestDbLoader.TestUser1.UserId,
            Value = 1
        };
        dbContext.Add(initialVote);
        await dbContext.SaveChangesAsync(ct);

        await UsingComponentAsync(new PostVoteRequest()
        {
            PostId = post.Id,
            VoteType = VoteType.NoVote
        }, async (controller, request, serviceProvider, ct1) =>
        {
            // Act
            IActionResult result = await controller.VotePostAsync(request, ct1);

            // Assert
            Assert.IsNotNull(result);
            OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
            PostVoteResponse response = Assert.IsInstanceOfType<PostVoteResponse>(ok.Value);
            
            Assert.AreEqual(VoteType.NoVote, response.UserVote);

            // Verify vote was deleted from database
            CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
            CloudbbPostVote? deletedVote = await dbContextCheck.Set<CloudbbPostVote>()
                .FirstOrDefaultAsync(v => v.PostId == post.Id && v.UserId == IntegrationTestDbLoader.TestUser1.UserId, ct1);
            
            Assert.IsNull(deletedVote); // Vote should be deleted
        }, ct);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VotePostAsync_OnOwnPost_ShouldReturnForbidAsync() => UsingComponentAsync(new PostVoteRequest()
    {
        // Arrange - Get a post owned by the current user (GlobalTestUser)
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        VoteType = VoteType.Upvote
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.VotePostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        ForbidResult forbid = Assert.IsInstanceOfType<ForbidResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VotePostAsync_OnNonExistentPost_ShouldReturnNotFoundAsync() => UsingComponentAsync(new PostVoteRequest()
    {
        PostId = Guid.NewGuid(), // Non-existent ID
        VoteType = VoteType.Upvote
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.VotePostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        NotFoundResult notFound = Assert.IsInstanceOfType<NotFoundResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VotePostAsync_WithNullRequest_ShouldReturnBadRequestAsync() => UsingComponentAsync(async (controller, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.VotePostAsync(null!, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        Assert.AreEqual("Request body cannot be null", badRequest.Value);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VotePostAsync_WithGuestUser_ShouldReturnUnauthorizedAsync() => UsingComponentAsync(new PostVoteRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        VoteType = VoteType.Upvote
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Arrange - Set unauthenticated user context
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
        // Act
        IActionResult result = await controller.VotePostAsync(request, ct);
        // Assert
        Assert.IsNotNull(result);
        UnauthorizedResult unauthorized = Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }, TestContext.CancellationToken);
}