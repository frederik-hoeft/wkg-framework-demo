using Cloudbb.Web.Api.V1.Controllers;
using Cloudbb.Web.Api.V1.Models.Auth;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Cloudbb.Web.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Testing.Platform.Services;
using System.Security.Claims;

namespace Cloudbb.Web.Tests.Integration.Api.V1.Auth;

[TestClass]
public sealed class AuthController_RegisterTests : ControllerBaseTest<AuthController>
{
    public override TestContext TestContext { get; set; }

    [TestMethod]
    public Task RegisterAsync_WithValidData_ShouldSucceedAsync() => UsingComponentAsync(new RegisterRequest()
    {
        Email = "test@example.com",
        // max length is 32, so choose something safe
        Username = "MyTestUser",
        Password = "P@ssw0rdMyTestUser",
        ConfirmPassword = "P@ssw0rdMyTestUser",
    }, async (controller, request, serviceProvider, ct) =>
    {
        // Act
        IActionResult result = await controller.RegisterAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
        AuthResponse response = Assert.IsInstanceOfType<AuthResponse>(ok.Value);
        Assert.IsTrue(response.IsSuccess);
        Assert.IsNotNull(response.Token);
        Assert.IsNotNull(response.ExpiresAt);
        Assert.IsTrue(response.ExpiresAt > DateTime.UtcNow);

        // Verify token is valid
        IJwtService jwtService = serviceProvider.GetRequiredService<IJwtService>();
        ClaimsPrincipal validationResult = await jwtService.ValidateTokenAsync(response.Token, ct);
        Assert.IsNotNull(validationResult);

        // Verify user was created in database
        ApplicationDbContext dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        CloudbbUser? createdUser = await dbContext.Set<CloudbbUser>()
            .Include(u => u.IdentityUser)
            .FirstOrDefaultAsync(u => u.IdentityUser.Email == request.Email, ct);
        Assert.IsNotNull(createdUser);
        Assert.AreEqual(request.Username, createdUser.IdentityUser.UserName);
        Assert.AreEqual(request.Email, createdUser.IdentityUser.Email);

        // Verify user has default role
        UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        IList<string> roles = await userManager.GetRolesAsync(createdUser.IdentityUser);
        Assert.Contains("user", roles);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task RegisterAsync_WithDuplicateEmail_ShouldReturnBadRequestAsync() => UsingTransactionAsync(async (_, ct) =>
    {
        // Arrange
        const string DUPLICATE_EMAIL = "MyTestUser@example.com";

        // First registration should succeed
        await UsingComponentAsync(new RegisterRequest()
        {
            Email = DUPLICATE_EMAIL,
            Username = "MyTestUser1",
            Password = "P@ssw0rdMyTestUser1",
            ConfirmPassword = "P@ssw0rdMyTestUser1",
        }, async (controller, request, ct1) =>
        {
            IActionResult firstResult = await controller.RegisterAsync(request, ct1);
            Assert.IsInstanceOfType<OkObjectResult>(firstResult);
        }, ct);

        // Act - Second registration with same email should fail
        await UsingComponentAsync(new RegisterRequest()
        {
            Email = DUPLICATE_EMAIL, // Same email
            Username = "MyTestUser2",
            Password = "P@ssw0rdMyTestUser2",
            ConfirmPassword = "P@ssw0rdMyTestUser2",
        }, async (controller, request, ct1) =>
        {
            IActionResult result = await controller.RegisterAsync(request, ct1);

            // Assert
            Assert.IsNotNull(result);
            BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
            AuthResponse response = Assert.IsInstanceOfType<AuthResponse>(badRequest.Value);
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.Token);
            Assert.IsNotNull(response.Message);
            Assert.IsTrue(response.Message.Contains("already", StringComparison.OrdinalIgnoreCase));
        }, ct);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task RegisterAsync_WithDuplicateUsername_ShouldReturnBadRequestAsync() => UsingTransactionAsync(async (_, ct) =>
    {
        // Arrange
        string duplicateUsername = "MyTestUser";
        
        // Act - First registration should succeed
        await UsingComponentAsync(new RegisterRequest()
        {
            Email = "first_MyTestUser@example.com",
            Username = duplicateUsername,
            Password = "P@ssw0rdMyTestUser",
            ConfirmPassword = "P@ssw0rdMyTestUser",
        }, async (controller, request, ct1) =>
        {
            IActionResult firstResult = await controller.RegisterAsync(request, ct1);
            Assert.IsInstanceOfType<OkObjectResult>(firstResult);
        }, ct);

        // Act - Second registration with same username should fail
        await UsingComponentAsync(new RegisterRequest()
        {
            Email = "second_MyTestUser@example.com",
            Username = duplicateUsername, // Same username
            Password = "P@ssw0rdMyTestUser2",
            ConfirmPassword = "P@ssw0rdMyTestUser2",
        }, async (controller, request, ct1) =>
        {
            IActionResult result = await controller.RegisterAsync(request, ct1);

            // Assert
            Assert.IsNotNull(result);
            BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
            AuthResponse response = Assert.IsInstanceOfType<AuthResponse>(badRequest.Value);
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.Token);
            Assert.IsNotNull(response.Message);
            Assert.IsTrue(response.Message.Contains("already", StringComparison.OrdinalIgnoreCase));
        }, ct);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task RegisterAsync_WithMismatchedPasswords_ShouldReturnBadRequestAsync() => UsingComponentAsync(new RegisterRequest()
    {
        Email = "test@example.com",
        Username = "MyTestUser",
        Password = "P@ssw0rdMyTestUser",
        ConfirmPassword = "DifferentP@ssw0rdMyTestUser", // Different password
    }, async (controller, request, ct) =>
    {
        // Act
        IActionResult result = await controller.RegisterAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        AuthResponse response = Assert.IsInstanceOfType<AuthResponse>(badRequest.Value);
        Assert.IsFalse(response.IsSuccess);
        Assert.IsNull(response.Token);
        Assert.IsNotNull(response.Message);
        Assert.AreEqual("Invalid request data", response.Message);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task RegisterAsync_WithWeakPassword_ShouldReturnBadRequestAsync() => UsingComponentAsync(new RegisterRequest()
    {
        Email = "test@example.com",
        Username = "MyTestUser",
        Password = "weak", // Weak password that doesn't meet requirements
        ConfirmPassword = "weak",
    }, async (controller, request, ct) =>
    {
        // Act
        IActionResult result = await controller.RegisterAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        AuthResponse response = Assert.IsInstanceOfType<AuthResponse>(badRequest.Value);
        Assert.IsFalse(response.IsSuccess);
        Assert.IsNull(response.Token);
        Assert.IsNotNull(response.Message);
        // Should contain password validation error
        Assert.IsTrue(response.Message.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                        response.Message.Contains("length", StringComparison.OrdinalIgnoreCase));
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task RegisterAsync_WithInvalidEmail_ShouldReturnBadRequestAsync() => UsingComponentAsync(new RegisterRequest()
    {
        Email = "invalid-email-format", // Invalid email
        Username = "MyTestUser",
        Password = "P@ssw0rdMyTestUser",
        ConfirmPassword = "P@ssw0rdMyTestUser",
    }, async (controller, request, ct) =>
    {
        // Act
        IActionResult result = await controller.RegisterAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        AuthResponse response = Assert.IsInstanceOfType<AuthResponse>(badRequest.Value);
        Assert.IsFalse(response.IsSuccess);
        Assert.IsNull(response.Token);
        Assert.IsNotNull(response.Message);
        Assert.AreEqual("Invalid request data", response.Message);
    }, TestContext.CancellationToken);

    [TestMethod]
    public Task RegisterAsync_WithEmptyFields_ShouldReturnBadRequestAsync() => UsingComponentAsync(new RegisterRequest()
    {
        Email = "", // Empty email
        Username = "", // Empty username
        Password = "", // Empty password
        ConfirmPassword = "", // Empty confirm password
    }, async (controller, request, ct) =>
    {
        // Act
        IActionResult result = await controller.RegisterAsync(request, ct);

        // Assert
        Assert.IsNotNull(result);
        BadRequestObjectResult badRequest = Assert.IsInstanceOfType<BadRequestObjectResult>(result);
        AuthResponse response = Assert.IsInstanceOfType<AuthResponse>(badRequest.Value);
        Assert.IsFalse(response.IsSuccess);
        Assert.IsNull(response.Token);
        Assert.IsNotNull(response.Message);
        Assert.AreEqual("Invalid request data", response.Message);
    }, TestContext.CancellationToken);
}
