using Cloudbb.Web.Api.V1.Controllers;
using Cloudbb.Web.Api.V1.Models.Comments;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Cloudbb.Web.Tests.Integration.Api.V1.Comments;

[TestClass]
public sealed class CommentsController_DeleteCommentTests : ControllerBaseTest<CommentsController>
{
    public override TestContext TestContext { get; set; }

    protected async override ValueTask InitializeComponentAsync(CommentsController component, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await base.InitializeComponentAsync(component, serviceProvider, cancellationToken);
        await SetAuthenticatedUserContextAsync(component, serviceProvider, IntegrationTestDbLoader.TestUser1, cancellationToken);
    }

    [TestMethod]
    public Task DeleteCommentAsync_WithValidComment_ShouldDeleteCommentAsync() => UsingComponentAsync(new CommentDeleteRequest()
    {
        CommentId = IntegrationTestDbLoader.Comment2OnPost1Id, // Comment by User1
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.DeleteCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkResult ok = Assert.IsInstanceOfType<OkResult>(result);

        // Verify comment was deleted from database
        CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbComment? deletedComment = await dbContextCheck.Set<CloudbbComment>()
            .FirstOrDefaultAsync(c => c.Id == IntegrationTestDbLoader.Comment2OnPost1Id, ct);
            
        Assert.IsNull(deletedComment, "Comment should be deleted from database");

        // Verify votes were cascade deleted
        CloudbbCommentVote? deletedVote = await dbContextCheck.Set<CloudbbCommentVote>()
            .FirstOrDefaultAsync(v => v.CommentId == IntegrationTestDbLoader.Comment2OnPost1Id, ct);
            
        Assert.IsNull(deletedVote, "Comment votes should be cascade deleted");
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task DeleteCommentAsync_WithNonExistentComment_ShouldReturnNotFoundAsync() => UsingComponentAsync(new CommentDeleteRequest()
    {
        CommentId = Guid.NewGuid() // Non-existent ID
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.DeleteCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        NotFoundResult notFound = Assert.IsInstanceOfType<NotFoundResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task DeleteCommentAsync_WithOtherUsersComment_ShouldReturnForbiddenAsync() => UsingComponentAsync(new CommentDeleteRequest()
    {
        CommentId = IntegrationTestDbLoader.Comment1OnPost1Id, // Comment by User2, but we're authenticated as User1
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.DeleteCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        ForbidResult forbidden = Assert.IsInstanceOfType<ForbidResult>(result);

        // Verify comment was NOT deleted from database
        CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbComment? comment = await dbContextCheck.Set<CloudbbComment>()
            .FirstOrDefaultAsync(c => c.Id == IntegrationTestDbLoader.Comment1OnPost1Id, ct);
            
        Assert.IsNotNull(comment, "Comment should still exist in database");
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task DeleteCommentAsync_WithGuestUser_ShouldReturnUnauthorizedAsync() => UsingComponentAsync(new CommentDeleteRequest()
    {
        CommentId = IntegrationTestDbLoader.Comment2OnPost1Id,
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Arrange - Clear authenticated user context to simulate guest
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
        
        // Act
        IActionResult result = await controller.DeleteCommentAsync(request, ct);
        
        // Assert
        Assert.IsNotNull(result);
        UnauthorizedResult unauthorized = Assert.IsInstanceOfType<UnauthorizedResult>(result);

        // Verify comment was NOT deleted from database
        CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbComment? comment = await dbContextCheck.Set<CloudbbComment>()
            .FirstOrDefaultAsync(c => c.Id == IntegrationTestDbLoader.Comment2OnPost1Id, ct);
            
        Assert.IsNotNull(comment, "Comment should still exist in database");
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task DeleteCommentAsync_AsUser2WithOwnComment_ShouldDeleteCommentAsync() => UsingComponentAsync(new CommentDeleteRequest()
    {
        CommentId = IntegrationTestDbLoader.Comment1OnPost1Id, // Comment by User2
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Arrange - Authenticate as User2 who owns Comment1
        await SetAuthenticatedUserContextAsync(controller, serviceProvider, IntegrationTestDbLoader.TestUser2, ct);
        
        // Act
        IActionResult result = await controller.DeleteCommentAsync(request, ct);
        
        // Assert
        Assert.IsNotNull(result);
        OkResult ok = Assert.IsInstanceOfType<OkResult>(result);

        // Verify comment was deleted from database
        CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbComment? deletedComment = await dbContextCheck.Set<CloudbbComment>()
            .FirstOrDefaultAsync(c => c.Id == IntegrationTestDbLoader.Comment1OnPost1Id, ct);
            
        Assert.IsNull(deletedComment, "Comment should be deleted from database");
    }, TestContext.CancellationToken);
}