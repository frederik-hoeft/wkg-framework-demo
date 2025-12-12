using Cloudbb.Web.Api.V1.Controllers;
using Cloudbb.Web.Api.V1.Models;
using Cloudbb.Web.Api.V1.Models.Posts;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cloudbb.Web.Tests.Integration.Api.V1.Posts;

[TestClass]
public sealed class PostsController_GetPostTests : ControllerBaseTest<PostsController>
{
    public override TestContext TestContext { get; set; }

    protected async override ValueTask InitializeComponentAsync(PostsController component, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await base.InitializeComponentAsync(component, serviceProvider, cancellationToken);
        await SetAuthenticatedUserContextAsync(component, serviceProvider, IntegrationTestDbLoader.TestUser1, cancellationToken);
    }

    [TestMethod]
    public Task GetPostAsync_WithValidPostId_ShouldReturnPostAsync() => UsingComponentAsync(new PostReadRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        TimeZone = new TimeZone { IanaTimeZoneId = "UTC" },
        IncludeComments = true
    }, async (controller, request, serviceProvider, ct1) =>
    {
        // Act
        IActionResult result = await controller.GetPostAsync(request, ct1);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        PostReadResponse response = Assert.IsInstanceOfType<PostReadResponse>(ok.Value);
            
        Assert.AreEqual(IntegrationTestDbLoader.Post1OfUser1Id, response.PostId);
        Assert.AreEqual(IntegrationTestDbLoader.TestUser1.UserId, response.UserId);
        Assert.AreEqual(IntegrationTestDbLoader.TestUser1.Username, response.Username);
        Assert.IsNotNull(response.Title);
        Assert.IsNotNull(response.Content);
        Assert.IsGreaterThanOrEqualTo(lowerBound: 1, response.Revisions);
        Assert.IsNotNull(response.Comments); // Comments included
        // should be able to edit own post
        Assert.IsTrue(response.CanEdit);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task GetPostAsync_WithCommentsDisabled_ShouldReturnPostWithoutCommentsAsync() => UsingComponentAsync(new PostReadRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        TimeZone = new TimeZone { IanaTimeZoneId = "UTC" },
        IncludeComments = false
    }, async (controller, request, serviceProvider, ct1) =>
    {
        // Act
        IActionResult result = await controller.GetPostAsync(request, ct1);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        PostReadResponse response = Assert.IsInstanceOfType<PostReadResponse>(ok.Value);
            
        Assert.AreEqual(IntegrationTestDbLoader.Post1OfUser1Id, response.PostId);
        Assert.IsNull(response.Comments); // Comments not included
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task GetPostAsync_WithNonExistentId_ShouldReturnNotFoundAsync() => UsingComponentAsync(new PostReadRequest()
    {
        PostId = Guid.NewGuid(), // Non-existent ID
        TimeZone = new TimeZone { IanaTimeZoneId = "UTC" },
        IncludeComments = false
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.GetPostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        NotFoundResult notFound = Assert.IsInstanceOfType<NotFoundResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task GetPostAsync_WithGuestUser_ShouldReturnUnauthorizedAsync() => UsingComponentAsync(new PostReadRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        TimeZone = new TimeZone { IanaTimeZoneId = "UTC" },
        IncludeComments = false
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Arrange - Set unauthenticated user context
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
        // Act
        IActionResult result = await controller.GetPostAsync(request, ct);
        // Assert
        Assert.IsNotNull(result);
        UnauthorizedResult unauthorized = Assert.IsInstanceOfType<UnauthorizedResult>(result);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task GetPostAsync_WithUserVote_ShouldReturnUserVoteStatusAsync() => UsingComponentAsync(new PostReadRequest()
    {
        PostId = IntegrationTestDbLoader.Post1OfUser1Id,
        TimeZone = new TimeZone { IanaTimeZoneId = "UTC" },
        IncludeComments = false
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Arrange - user 2 has upvoted user 1's post, need to authenticate as user 2
        await SetAuthenticatedUserContextAsync(controller, serviceProvider, IntegrationTestDbLoader.TestUser2, ct);

        // Act
        IActionResult result = await controller.GetPostAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        PostReadResponse response = Assert.IsInstanceOfType<PostReadResponse>(ok.Value);
            
        // Should have vote information
        Assert.IsGreaterThanOrEqualTo(lowerBound: 1, response.VoteScore);
        Assert.AreEqual(VoteType.Upvote, response.UserVote);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task GetPostAsync_WithInvalidRequest_ShouldReturnBadRequestAsync() => UsingComponentAsync(async (controller, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.GetPostAsync(null!, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        Assert.AreEqual("Request body cannot be null", badRequest.Value);
    }, TestContext.CancellationToken);
}