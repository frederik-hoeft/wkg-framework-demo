using Cloudbb.Web.Api.V1.Controllers;
using Cloudbb.Web.Api.V1.Models.Posts;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Cloudbb.Web.Tests.Integration.Api.V1.Posts;

[TestClass]
public sealed class PostsController_DeletePostTests : ControllerBaseTest<PostsController>
{
    public override TestContext TestContext { get; set; }

    protected async override ValueTask InitializeComponentAsync(PostsController component, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await base.InitializeComponentAsync(component, serviceProvider, cancellationToken);
        await SetAuthenticatedUserContextAsync(component, serviceProvider, IntegrationTestDbLoader.TestUser1, cancellationToken);
    }

    [TestMethod]
    public Task DeletePostAsync_WithValidPost_ShouldDeletePostAsync() => UsingComponentAsync(new PostDeleteRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.DeletePostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkResult ok = Assert.IsInstanceOfType<OkResult>(result);

        // Verify post was deleted from database
        CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbPost? deletedPost = await dbContextCheck.Set<CloudbbPost>()
            .FirstOrDefaultAsync(p => p.Id == IntegrationTestDbLoader.Post1OfUser1Id, ct);
            
        Assert.IsNull(deletedPost, "Post should be deleted from database");

        // Verify revisions were cascade deleted
        CloudbbPostRevision? deletedRevision = await dbContextCheck.Set<CloudbbPostRevision>()
            .FirstOrDefaultAsync(r => r.PostId == IntegrationTestDbLoader.Post1OfUser1Id, ct);
            
        Assert.IsNull(deletedRevision, "Revisions should be cascade deleted");
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task DeletePostAsync_WithNonExistentPost_ShouldReturnNotFoundAsync() => UsingComponentAsync(new PostDeleteRequest()
    {
        PostId = Guid.NewGuid() // Non-existent ID
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.DeletePostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        NotFoundResult notFound = Assert.IsInstanceOfType<NotFoundResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task DeletePostAsync_WithGuestUser_ShouldReturnUnauthorizedAsync() => UsingComponentAsync(async (controller, serviceProvider, ct) =>
    {
        // Arrange - Clear authenticated user context to simulate guest
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
        PostDeleteRequest request = new()
        {
            PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        };
        // Act
        IActionResult result = await controller.DeletePostAsync(request, ct);
        // Assert
        Assert.IsNotNull(result);
        UnauthorizedResult unauthorized = Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task DeletePostAsync_WithUnauthorizedUser_ShouldReturnForbidAsync() => UsingComponentAsync(new PostDeleteRequest()
    {
        // Arrange - Get a post NOT owned by the current test user (GlobalTestUser)
        PostId = IntegrationTestDbLoader.PostOfUser2Id,
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.DeletePostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        ForbidResult forbid = Assert.IsInstanceOfType<ForbidResult>(result);

        // Verify post was NOT deleted
        CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbPost? stillExistsPost = await dbContextCheck.Set<CloudbbPost>()
            .FirstOrDefaultAsync(p => p.Id == IntegrationTestDbLoader.PostOfUser2Id, ct);
            
        Assert.IsNotNull(stillExistsPost, "Post should still exist after unauthorized delete attempt");
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task DeletePostAsync_WithNullRequest_ShouldReturnBadRequestAsync() => UsingComponentAsync(async (controller, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.DeletePostAsync(null!, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        Assert.AreEqual("Request body cannot be null", badRequest.Value);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task DeletePostAsync_WithCommentsAndVotes_ShouldDeleteEverythingAsync() => UsingComponentAsync(new PostDeleteRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.DeletePostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkResult ok = Assert.IsInstanceOfType<OkResult>(result);

        // Verify everything was cascade deleted
        CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
            
        CloudbbPost? deletedPost = await dbContextCheck.Set<CloudbbPost>()
            .FirstOrDefaultAsync(p => p.Id == IntegrationTestDbLoader.Post1OfUser1Id, ct);
        Assert.IsNull(deletedPost);

        CloudbbComment? deletedComment = await dbContextCheck.Set<CloudbbComment>()
            .FirstOrDefaultAsync(c => c.PostId == IntegrationTestDbLoader.Post1OfUser1Id, ct);
        Assert.IsNull(deletedComment);

        CloudbbPostVote? deletedVote = await dbContextCheck.Set<CloudbbPostVote>()
            .FirstOrDefaultAsync(v => v.PostId == IntegrationTestDbLoader.Post1OfUser1Id, ct);
        Assert.IsNull(deletedVote);
    }, TestContext.CancellationToken);
}