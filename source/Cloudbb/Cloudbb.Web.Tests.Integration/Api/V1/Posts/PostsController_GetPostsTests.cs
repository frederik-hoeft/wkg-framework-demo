using Cloudbb.Web.Api.V1.Controllers;
using Cloudbb.Web.Api.V1.Models;
using Cloudbb.Web.Api.V1.Models.Posts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Cloudbb.Web.Tests.Integration.Api.V1.Posts;

[TestClass]
public sealed class PostsController_GetPostsTests : ControllerBaseTest<PostsController>
{
    public override TestContext TestContext { get; set; }

    protected async override ValueTask InitializeComponentAsync(PostsController component, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        await base.InitializeComponentAsync(component, serviceProvider, cancellationToken);
        await SetAuthenticatedUserContextAsync(component, serviceProvider, IntegrationTestDbLoader.TestUser1, cancellationToken);
    }

    [TestMethod]
    public Task GetPostsAsync_WithDefaultPaging_ShouldReturnPostsAsync() => UsingComponentAsync(new PostListRequest()
    {
        TimeZone = new TimeZone { IanaTimeZoneId = "UTC" },
        PageNumber = 1,
        PageSize = 20,
        SortMode = PostListSortMode.Score,
        SortOrder = SortOrder.Descending
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.GetPostsAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        PostListResponse response = Assert.IsInstanceOfType<PostListResponse>(ok.Value);
        
        // Should return all test posts (3 from loader)
        Assert.IsGreaterThanOrEqualTo(lowerBound: 3, response.Posts.Count);
        
        // Verify ordering by score (descending by default)
        for (int i = 0; i < response.Posts.Count - 1; i++)
        {
            Assert.IsGreaterThanOrEqualTo(lowerBound: response.Posts[i + 1].VoteScore, response.Posts[i].VoteScore);
        }
        
        // Verify post structure
        PostListResponseEntry firstPost = response.Posts.First();
        Assert.AreNotEqual(Guid.Empty, firstPost.PostId);
        Assert.AreNotEqual(Guid.Empty, firstPost.UserId);
        Assert.IsNotNull(firstPost.Title);
        Assert.IsNotNull(firstPost.ContentPreview);
        Assert.IsGreaterThanOrEqualTo(lowerBound: 1, firstPost.Revisions);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task GetPostsAsync_WithActivitySort_ShouldReturnPostsByActivityAsync() => UsingComponentAsync(new PostListRequest()
    {
        TimeZone = new TimeZone { IanaTimeZoneId = "UTC" },
        SortMode = PostListSortMode.Activity,
        SortOrder = SortOrder.Descending
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.GetPostsAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        PostListResponse response = Assert.IsInstanceOfType<PostListResponse>(ok.Value);
        
        Assert.IsGreaterThanOrEqualTo(lowerBound: 3, response.Posts.Count);
        
        // Verify ordering by activity (creation time descending)
        for (int i = 0; i < response.Posts.Count - 1; i++)
        {
            Assert.IsGreaterThanOrEqualTo(lowerBound: response.Posts[i + 1].LastModified, value: response.Posts[i].LastModified);
        }
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task GetPostsAsync_WithPagination_ShouldReturnCorrectPageAsync() => UsingComponentAsync(new PostListRequest()
    {
        TimeZone = new TimeZone { IanaTimeZoneId = "UTC" },
        PageNumber = 1,
        PageSize = 2
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.GetPostsAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        PostListResponse response = Assert.IsInstanceOfType<PostListResponse>(ok.Value);
        
        // Should return at most 2 posts due to page size
        Assert.IsLessThanOrEqualTo(upperBound: 2, response.Posts.Count);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task GetPostsAsync_WithInvalidRequest_ShouldReturnBadRequestAsync() => UsingComponentAsync(async (controller, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.GetPostsAsync(null!, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        Assert.AreEqual("Request body cannot be null", badRequest.Value);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task GetPostsAsync_WithGuestUser_ShouldReturnUnauthorizedAsync() => UsingComponentAsync(new PostListRequest()
    {
        TimeZone = new TimeZone { IanaTimeZoneId = "UTC" },
        PageNumber = 1,
        PageSize = 20
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Arrange - Set unauthenticated user context
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
        // Act
        IActionResult result = await controller.GetPostsAsync(request, ct);
        // Assert
        Assert.IsNotNull(result);
        UnauthorizedResult unauthorized = Assert.IsInstanceOfType<UnauthorizedResult>(result);
        Assert.AreEqual(401, unauthorized.StatusCode);
    }, TestContext.CancellationToken);
}