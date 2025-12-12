using Cloudbb.Web.Api.V1.Controllers;
using Cloudbb.Web.Api.V1.Models.Posts;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Cloudbb.Web.Tests.Integration.Api.V1.Posts;

[TestClass]
public sealed class PostsController_CreatePostTests : ControllerBaseTest<PostsController>
{
    public override TestContext TestContext { get; set; }

    protected async override ValueTask InitializeComponentAsync(PostsController component, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await base.InitializeComponentAsync(component, serviceProvider, cancellationToken);
        await SetAuthenticatedUserContextAsync(component, serviceProvider, IntegrationTestDbLoader.TestUser1, cancellationToken);
    }

    [TestMethod]
    public Task CreatePostAsync_WithValidData_ShouldCreatePostAsync() => UsingComponentAsync(new PostCreationRequest()
    {
        Title = "Test Post Title",
        Content = "This is a test post content that should be saved to the database."
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.CreatePostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        PostCreationResponse response = Assert.IsInstanceOfType<PostCreationResponse>(ok.Value);
        
        Assert.AreNotEqual(Guid.Empty, response.PostId);

        // Verify post was created in database
        ApplicationDbContext dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        CloudbbPost? createdPost = await dbContext.Set<CloudbbPost>()
            .Include(p => p.Revisions)
            .Include(p => p.User.IdentityUser)
            .FirstOrDefaultAsync(p => p.Id == response.PostId, ct);
        
        Assert.IsNotNull(createdPost);
        Assert.AreEqual(IntegrationTestDbLoader.TestUser1.Username, createdPost.User.Username);
        Assert.IsNotNull(createdPost.Revisions);
        Assert.IsGreaterThanOrEqualTo(lowerBound: 1, createdPost.Revisions.Count);
        
        CloudbbPostRevision latestRevision = createdPost.Revisions.OrderByDescending(r => r.CreationTime).First();
        Assert.AreEqual(request.Title, latestRevision.Title);
        Assert.AreEqual(request.Content, latestRevision.Content);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task CreatePostAsync_WithGuestUser_ShouldReturnUnauthorizedAsync() => UsingComponentAsync(new PostCreationRequest()
    {
        Title = "Guest Post Title",
        Content = "This is a post attempt by a guest user."
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Arrange - Set unauthenticated user context
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
        // Act
        IActionResult result = await controller.CreatePostAsync(request, ct);
        // Assert
        Assert.IsNotNull(result);
        UnauthorizedResult unauthorized = Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task CreatePostAsync_WithEmptyTitle_ShouldFailValidationAsync() => UsingComponentAsync(new PostCreationRequest()
    {
        Title = string.Empty,
        Content = "Valid content"
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.CreatePostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        // Should fail validation due to empty title
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task CreatePostAsync_WithNullTitle_ShouldFailValidationAsync() => UsingComponentAsync(new PostCreationRequest()
    {
        Title = null!,
        Content = "Valid content"
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.CreatePostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        // Should fail validation due to empty title
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task CreatePostAsync_WithEmptyContent_ShouldFailValidationAsync() => UsingComponentAsync(new PostCreationRequest()
    {
        Title = "Valid Title",
        Content = string.Empty
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.CreatePostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        // Should fail validation due to empty content
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task CreatePostAsync_WithLongContent_ShouldCreatePostSuccessfullyAsync() => UsingComponentAsync(new PostCreationRequest()
    {
        Title = "Post with Long Content",
        Content = new string('A', 2000) // 2000 character content to test longer posts
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.CreatePostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        PostCreationResponse response = Assert.IsInstanceOfType<PostCreationResponse>(ok.Value);
        
        Assert.AreNotEqual(Guid.Empty, response.PostId);

        // Verify content was saved correctly
        ApplicationDbContext dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        CloudbbPost? createdPost = await dbContext.Set<CloudbbPost>()
            .Include(p => p.Revisions)
            .FirstOrDefaultAsync(p => p.Id == response.PostId, ct);
        
        Assert.IsNotNull(createdPost);
        CloudbbPostRevision latestRevision = createdPost.Revisions.OrderByDescending(r => r.CreationTime).First();
        Assert.AreEqual(request.Content.Length, latestRevision.Content.Length);
    }, TestContext.CancellationToken);
}