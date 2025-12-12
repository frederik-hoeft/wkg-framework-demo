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
public sealed class CommentsController_CreateCommentTests : ControllerBaseTest<CommentsController>
{
    public override TestContext TestContext { get; set; }

    protected async override ValueTask InitializeComponentAsync(CommentsController component, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await base.InitializeComponentAsync(component, serviceProvider, cancellationToken);
        await SetAuthenticatedUserContextAsync(component, serviceProvider, IntegrationTestDbLoader.TestUser1, cancellationToken);
    }

    [TestMethod]
    public Task CreateCommentAsync_WithValidData_ShouldCreateCommentAsync() => UsingComponentAsync(new CommentCreationRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        Content = "This is a test comment that should be saved to the database."
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.CreateCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        CommentCreationResponse response = Assert.IsInstanceOfType<CommentCreationResponse>(ok.Value);
        
        Assert.AreNotEqual(Guid.Empty, response.CommentId);

        // Verify comment was created in database
        CloudbbDbContext dbContext = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbComment? createdComment = await dbContext.Set<CloudbbComment>()
            .Include(c => c.User.IdentityUser)
            .FirstOrDefaultAsync(c => c.Id == response.CommentId, ct);
        
        Assert.IsNotNull(createdComment);
        Assert.AreEqual(IntegrationTestDbLoader.TestUser1.Username, createdComment.User.Username);
        Assert.AreEqual(request.PostId, createdComment.PostId);
        Assert.AreEqual(request.Content, createdComment.Content);
        Assert.IsTrue(createdComment.CreationTime <= DateTime.UtcNow);
        Assert.IsTrue(createdComment.CreationTime > DateTime.UtcNow.AddMinutes(-1));
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task CreateCommentAsync_WithNonExistentPost_ShouldReturnNotFoundAsync() => UsingComponentAsync(new CommentCreationRequest()
    {
        PostId = Guid.NewGuid(), // Non-existent post ID
        Content = "This comment targets a non-existent post."
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.CreateCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        NotFoundResult notFound = Assert.IsInstanceOfType<NotFoundResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task CreateCommentAsync_WithEmptyContent_ShouldReturnBadRequestAsync() => UsingComponentAsync(new CommentCreationRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        Content = ""
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.CreateCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task CreateCommentAsync_WithMaxLengthContent_ShouldCreateCommentAsync() => UsingComponentAsync(new CommentCreationRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        Content = new string('A', 1024) // Maximum allowed length
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.CreateCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        CommentCreationResponse response = Assert.IsInstanceOfType<CommentCreationResponse>(ok.Value);
        
        Assert.AreNotEqual(Guid.Empty, response.CommentId);

        // Verify comment was created in database with correct content
        CloudbbDbContext dbContext = serviceProvider.GetRequiredService<CloudbbDbContext>();
        CloudbbComment? createdComment = await dbContext.Set<CloudbbComment>()
            .FirstOrDefaultAsync(c => c.Id == response.CommentId, ct);
        
        Assert.IsNotNull(createdComment);
        Assert.AreEqual(1024, createdComment.Content.Length);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task CreateCommentAsync_WithOverLengthContent_ShouldReturnBadRequestAsync() => UsingComponentAsync(new CommentCreationRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        Content = new string('A', 1025) // Exceeds maximum allowed length
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.CreateCommentAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task CreateCommentAsync_WithGuestUser_ShouldReturnUnauthorizedAsync() => UsingComponentAsync(new CommentCreationRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        Content = "Guest comment attempt."
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Arrange - Clear authenticated user context to simulate guest
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
        
        // Act
        IActionResult result = await controller.CreateCommentAsync(request, ct);
        
        // Assert
        Assert.IsNotNull(result);
        UnauthorizedResult unauthorized = Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }, TestContext.CancellationToken);
}