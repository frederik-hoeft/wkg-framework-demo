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
public sealed class PostsController_EditPostTests : ControllerBaseTest<PostsController>
{
    public override TestContext TestContext { get; set; }

    protected async override ValueTask InitializeComponentAsync(PostsController component, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await base.InitializeComponentAsync(component, serviceProvider, cancellationToken);
        await SetAuthenticatedUserContextAsync(component, serviceProvider, IntegrationTestDbLoader.TestUser1, cancellationToken);
    }

    [TestMethod]
    public Task EditPostAsync_WithValidData_ShouldEditPostAsync() => UsingComponentAsync(new PostEditRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        Title = "Updated Post Title",
        Content = "This is the updated content for the post."
    }, async (controller, request, serviceProvider, ct1) =>
    {
        // Act
        IActionResult result = await controller.EditPostAsync(request, ct1);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        PostEditResponse response = Assert.IsInstanceOfType<PostEditResponse>(ok.Value);
            
        Assert.AreEqual(IntegrationTestDbLoader.Post1OfUser1Id, response.PostId);

        // Verify new revision was created
        CloudbbDbContext dbContextCheck = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbPost? updatedPost = await dbContextCheck.Set<CloudbbPost>()
            .Include(p => p.Revisions)
            .FirstOrDefaultAsync(p => p.Id == IntegrationTestDbLoader.Post1OfUser1Id, ct1);
            
        Assert.IsNotNull(updatedPost);
        Assert.IsGreaterThanOrEqualTo(lowerBound: 2, updatedPost.Revisions.Count); // Original + new revision
            
        CloudbbPostRevision latestRevision = updatedPost.Revisions.OrderByDescending(r => r.CreationTime).First();
        Assert.AreEqual(request.Title, latestRevision.Title);
        Assert.AreEqual(request.Content, latestRevision.Content);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task EditPostAsync_WithNonExistentPost_ShouldReturnNotFoundAsync() => UsingComponentAsync(new PostEditRequest()
    {
        PostId = Guid.NewGuid(), // Non-existent ID
        Title = "Updated Title",
        Content = "Updated content"
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.EditPostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        NotFoundResult notFound = Assert.IsInstanceOfType<NotFoundResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task EditPostAsync_WithGuestUser_ShouldReturnUnauthorizedAsync() => UsingComponentAsync(async (controller, serviceProvider, ct) =>
    {
        // Arrange - Set unauthenticated user context
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
        PostEditRequest request = new()
        {
            PostId = IntegrationTestDbLoader.Post1OfUser1Id,
            Title = "Guest Edit Attempt",
            Content = "This edit should fail due to lack of authentication"
        };
        // Act
        IActionResult result = await controller.EditPostAsync(request, ct);
        // Assert
        Assert.IsNotNull(result);
        UnauthorizedResult unauthorized = Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task EditPostAsync_WithUnauthorizedUser_ShouldReturnForbidAsync() => UsingComponentAsync(new PostEditRequest()
    {
        PostId = IntegrationTestDbLoader.PostOfUser2Id,
        Title = "Unauthorized Edit",
        Content = "This edit should fail due to permissions"
    }, async (controller, request, ct) =>
    {
        // Act
        IActionResult result = await controller.EditPostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        ForbidResult forbid = Assert.IsInstanceOfType<ForbidResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task EditPostAsync_WithEmptyTitle_ShouldFailValidationAsync() => UsingComponentAsync(new PostEditRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        Title = string.Empty, // Invalid empty title
        Content = "Valid content"
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.EditPostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task EditPostAsync_WithNullRequest_ShouldReturnBadRequestAsync() => UsingComponentAsync(async (controller, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.EditPostAsync(null!, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        Assert.AreEqual("Request body cannot be null", badRequest.Value);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task EditPostAsync_MultipleEdits_ShouldCreateMultipleRevisionsAsync() => UsingTransactionAsync(async (dbContext, ct) =>
    {
        CloudbbPost? post = await dbContext.Set<CloudbbPost>()
            .Include(p => p.Revisions)
            .FirstOrDefaultAsync(p => p.Id == IntegrationTestDbLoader.Post1OfUser1Id, ct);
        Assert.IsNotNull(post);
        
        int initialRevisionCount = post.Revisions.Count;

        // First edit
        await UsingComponentAsync(new PostEditRequest()
        {
            PostId = post.Id,
            Title = "First Edit Title",
            Content = "First edit content"
        }, async (controller, request, serviceProvider, ct1) =>
        {
            // Act
            IActionResult result = await controller.EditPostAsync(request, ct1);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<OkObjectResult>(result);
        }, ct);
        // Second edit
        await UsingComponentAsync(new PostEditRequest()
        {
            PostId = post.Id,
            Title = "Second Edit Title", 
            Content = "Second edit content"
        }, async (controller, request, serviceProvider, ct1) =>
        {
            // Act
            IActionResult result = await controller.EditPostAsync(request, ct1);

            // Assert
            Assert.IsNotNull(result);
            OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);

            // Verify multiple revisions were created
            // use the controller itself to get the latest revision
            IActionResult postReadResult = await controller.GetPostAsync(new PostReadRequest() { PostId = post.Id, TimeZone = new TimeZone() }, ct1);
            Assert.IsNotNull(postReadResult);
            OkObjectResult postReadOk = Assert.IsInstanceOfType<OkObjectResult>(postReadResult);
            PostReadResponse postReadResponse = Assert.IsInstanceOfType<PostReadResponse>(postReadOk.Value);
            Assert.AreEqual(initialRevisionCount + 2, postReadResponse.Revisions);
            // Verify latest revision content
            Assert.AreEqual("Second Edit Title", postReadResponse.Title);
            Assert.AreEqual("Second edit content", postReadResponse.Content);
        }, ct);
    }, TestContext.CancellationToken);
}