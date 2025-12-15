using Cloudbb.Web.Api.V1.Controllers;
using Cloudbb.Web.Api.V1.Models;
using Cloudbb.Web.Api.V1.Models.Comments;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Cloudbb.Web.Tests.Integration.Api.V1.Comments;

[TestClass]
public sealed class CommentsController_VoteCommentTests : ControllerBaseTest<CommentsController>
{
    public override TestContext TestContext { get; set; }

    protected async override ValueTask InitializeComponentAsync(CommentsController component, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await base.InitializeComponentAsync(component, serviceProvider, cancellationToken);
        await SetAuthenticatedUserContextAsync(component, serviceProvider, IntegrationTestDbLoader.TestUser1, cancellationToken);
    }

    [TestMethod]
    public Task VoteCommentAsync_WithUpvoteOnOtherUsersComment_ShouldCreateVoteAsync() => UsingComponentAsync(new CommentVoteRequest()
    {
        CommentId = IntegrationTestDbLoader.Comment1OnPost1Id, // Comment by User2
        VoteType = VoteType.Upvote
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.VoteCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        CommentVoteResponse response = Assert.IsInstanceOfType<CommentVoteResponse>(ok.Value);
        
        Assert.AreEqual(IntegrationTestDbLoader.Post1OfUser1Id, response.CommentId);
        Assert.AreEqual(VoteType.Upvote, response.UserVote);
        Assert.AreEqual(1, response.NewScore); // Should be 1 (existing vote from test data was removed in our setup)

        // Verify vote was created in database
        CloudbbDbContext dbContext = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbCommentVote? vote = await dbContext.Set<CloudbbCommentVote>()
            .FirstOrDefaultAsync(v => v.CommentId == request.CommentId && v.UserId == IntegrationTestDbLoader.TestUser1.UserId, ct);
        
        Assert.IsNotNull(vote);
        Assert.AreEqual(1, vote.Value);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VoteCommentAsync_WithDownvoteOnOtherUsersComment_ShouldCreateDownvoteAsync() => UsingComponentAsync(new CommentVoteRequest()
    {
        CommentId = IntegrationTestDbLoader.Comment1OnPost1Id, // Comment by User2
        VoteType = VoteType.Downvote
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.VoteCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        CommentVoteResponse response = Assert.IsInstanceOfType<CommentVoteResponse>(ok.Value);
        
        Assert.AreEqual(IntegrationTestDbLoader.Post1OfUser1Id, response.CommentId);
        Assert.AreEqual(VoteType.Downvote, response.UserVote);
        Assert.AreEqual(-1, response.NewScore); // Should be -1

        // Verify vote was created in database
        CloudbbDbContext dbContext = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbCommentVote? vote = await dbContext.Set<CloudbbCommentVote>()
            .FirstOrDefaultAsync(v => v.CommentId == request.CommentId && v.UserId == IntegrationTestDbLoader.TestUser1.UserId, ct);
        
        Assert.IsNotNull(vote);
        Assert.AreEqual(-1, vote.Value);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VoteCommentAsync_WithNoVoteToRemoveExistingVote_ShouldRemoveVoteAsync() => UsingTransactionAsync(async (dbContext, ct) =>
    {
        // Arrange - Create an existing vote to remove
        CloudbbCommentVote existingVote = new()
        {
            CommentId = IntegrationTestDbLoader.Comment1OnPost1Id,
            UserId = IntegrationTestDbLoader.TestUser1.UserId,
            Value = 1
        };
        dbContext.Add(existingVote);
        await dbContext.SaveChangesAsync(ct);

        await UsingComponentAsync(new CommentVoteRequest()
        {
            CommentId = IntegrationTestDbLoader.Comment1OnPost1Id,
            VoteType = VoteType.NoVote
        }, async (controller, request, serviceProvider, ct1) =>
        {
            // Act
            IActionResult result = await controller.VoteCommentAsync(request, ct1);

            // Assert
            Assert.IsNotNull(result);
            OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
            CommentVoteResponse response = Assert.IsInstanceOfType<CommentVoteResponse>(ok.Value);
            
            Assert.AreEqual(IntegrationTestDbLoader.Post1OfUser1Id, response.CommentId);
            Assert.AreEqual(VoteType.NoVote, response.UserVote);
            Assert.AreEqual(0, response.NewScore); // Should be 0 after removing vote

            // Verify vote was removed from database
            CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
            CloudbbCommentVote? vote = await dbContextCheck.Set<CloudbbCommentVote>()
                .FirstOrDefaultAsync(v => v.CommentId == request.CommentId && v.UserId == IntegrationTestDbLoader.TestUser1.UserId, ct1);
            
            Assert.IsNull(vote);
        }, ct);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VoteCommentAsync_WithOwnComment_ShouldReturnForbiddenAsync() => UsingComponentAsync(new CommentVoteRequest()
    {
        CommentId = IntegrationTestDbLoader.Comment2OnPost1Id, // Comment by User1 (authenticated user)
        VoteType = VoteType.Upvote
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.VoteCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        ForbidResult forbidden = Assert.IsInstanceOfType<ForbidResult>(result);

        // Verify no vote was created in database
        CloudbbDbContext dbContext = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbCommentVote? vote = await dbContext.Set<CloudbbCommentVote>()
            .FirstOrDefaultAsync(v => v.CommentId == request.CommentId && v.UserId == IntegrationTestDbLoader.TestUser1.UserId, ct);
        
        Assert.IsNull(vote);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VoteCommentAsync_WithNonExistentComment_ShouldReturnNotFoundAsync() => UsingComponentAsync(new CommentVoteRequest()
    {
        CommentId = Guid.NewGuid(), // Non-existent comment
        VoteType = VoteType.Upvote
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.VoteCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        NotFoundResult notFound = Assert.IsInstanceOfType<NotFoundResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VoteCommentAsync_WithGuestUser_ShouldReturnUnauthorizedAsync() => UsingComponentAsync(new CommentVoteRequest()
    {
        CommentId = IntegrationTestDbLoader.Comment1OnPost1Id,
        VoteType = VoteType.Upvote
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Arrange - Clear authenticated user context to simulate guest
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();

        // Act
        IActionResult result = await controller.VoteCommentAsync(request, ct);
        
        // Assert
        Assert.IsNotNull(result);
        UnauthorizedResult unauthorized = Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VoteCommentAsync_ChangeExistingVoteFromUpvoteToDownvote_ShouldUpdateVoteAsync() => UsingTransactionAsync(async (dbContext, ct) =>
    {
        // Arrange - Create an existing upvote to change
        CloudbbCommentVote existingVote = new()
        {
            CommentId = IntegrationTestDbLoader.Comment1OnPost1Id,
            UserId = IntegrationTestDbLoader.TestUser1.UserId,
            Value = 1
        };
        dbContext.Add(existingVote);
        await dbContext.SaveChangesAsync(ct);

        await UsingComponentAsync(new CommentVoteRequest()
        {
            CommentId = IntegrationTestDbLoader.Comment1OnPost1Id,
            VoteType = VoteType.Downvote
        }, async (controller, request, serviceProvider, ct1) =>
        {
            // Act
            IActionResult result = await controller.VoteCommentAsync(request, ct1);

            // Assert
            Assert.IsNotNull(result);
            OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
            CommentVoteResponse response = Assert.IsInstanceOfType<CommentVoteResponse>(ok.Value);
            
            Assert.AreEqual(IntegrationTestDbLoader.Post1OfUser1Id, response.CommentId);
            Assert.AreEqual(VoteType.Downvote, response.UserVote);
            Assert.AreEqual(-1, response.NewScore); // Should be -1 after changing from upvote

            // Verify vote was updated in database
            CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
            CloudbbCommentVote? vote = await dbContextCheck.Set<CloudbbCommentVote>()
                .FirstOrDefaultAsync(v => v.CommentId == request.CommentId && v.UserId == IntegrationTestDbLoader.TestUser1.UserId, ct1);
            
            Assert.IsNotNull(vote);
            Assert.AreEqual(-1, vote.Value);
        }, ct);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task VoteCommentAsync_AsUser2OnOwnComment_ShouldReturnForbiddenAsync() => UsingComponentAsync(new CommentVoteRequest()
    {
        CommentId = IntegrationTestDbLoader.Comment1OnPost1Id, // Comment by User2
        VoteType = VoteType.Upvote
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Arrange - Authenticate as User2 who owns Comment1
        await SetAuthenticatedUserContextAsync(controller, serviceProvider, IntegrationTestDbLoader.TestUser2, ct);
        
        // Act
        IActionResult result = await controller.VoteCommentAsync(request, ct);
        
        // Assert
        Assert.IsNotNull(result);
        ForbidResult forbidden = Assert.IsInstanceOfType<ForbidResult>(result);

        // Verify no vote was created in database
        CloudbbDbContext dbContext = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbCommentVote? vote = await dbContext.Set<CloudbbCommentVote>()
            .FirstOrDefaultAsync(v => v.CommentId == request.CommentId && v.UserId == IntegrationTestDbLoader.TestUser2.UserId, ct);
        
        Assert.IsNull(vote);
    }, TestContext.CancellationToken);
}