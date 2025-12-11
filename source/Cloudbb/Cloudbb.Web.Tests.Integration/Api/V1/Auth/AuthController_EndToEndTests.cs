using Cloudbb.Web.Api.V1.Controllers;
using Cloudbb.Web.Api.V1.Models.Auth;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Cloudbb.Web.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Cloudbb.Web.Tests.Integration.Api.V1.Auth;

[TestClass]
public sealed class AuthController_EndToEndTests : ControllerBaseTest<AuthController>
{
    public override TestContext TestContext { get; set; }

    [TestMethod]
    public Task AuthFlow_RegisterThenLogin_ShouldWorkEndToEndAsync()
    {
        // Arrange
        string email = "MyTestUser@example.com";
        string username = "MyTestUser";
        string password = "P@ssw0rdMyTestUser";

        RegisterRequest registerRequest = new()
        {
            Email = email,
            Username = username,
            Password = password,
            ConfirmPassword = password,
        };

        LoginRequest loginRequest = new()
        {
            Email = email,
            Password = password
        };

        return UsingControllerAsync(registerRequest, async (controller, serviceProvider, ct) =>
        {
            // Act 1 - Register new user
            IActionResult registerResult = await controller.RegisterAsync(registerRequest, ct);

            // Assert 1 - Registration should succeed
            Assert.IsNotNull(registerResult);
            OkObjectResult registerOk = Assert.IsInstanceOfType<OkObjectResult>(registerResult);
            AuthResponse registerResponse = Assert.IsInstanceOfType<AuthResponse>(registerOk.Value);
            Assert.IsTrue(registerResponse.IsSuccess);
            Assert.IsNotNull(registerResponse.Token);

            // Verify first token is valid
            IJwtService jwtService = serviceProvider.GetRequiredService<IJwtService>();
            ClaimsPrincipal firstTokenValidation = await jwtService.ValidateTokenAsync(registerResponse.Token, ct);
            Assert.IsNotNull(firstTokenValidation);

            // Act 2 - Login with the same credentials
            IActionResult loginResult = await controller.LoginAsync(loginRequest, ct);

            // Assert 2 - Login should succeed
            Assert.IsNotNull(loginResult);
            OkObjectResult loginOk = Assert.IsInstanceOfType<OkObjectResult>(loginResult);
            AuthResponse loginResponse = Assert.IsInstanceOfType<AuthResponse>(loginOk.Value);
            Assert.IsTrue(loginResponse.IsSuccess);
            Assert.IsNotNull(loginResponse.Token);

            // Verify second token is valid
            ClaimsPrincipal secondTokenValidation = await jwtService.ValidateTokenAsync(loginResponse.Token, ct);
            Assert.IsNotNull(secondTokenValidation);

            // Both tokens should have the same user claims
            string? firstUserId = firstTokenValidation.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string? secondUserId = secondTokenValidation.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Assert.IsNotNull(firstUserId);
            Assert.IsNotNull(secondUserId);
            Assert.AreEqual(firstUserId, secondUserId);

            string? firstName = firstTokenValidation.FindFirst(ClaimTypes.Name)?.Value;
            string? secondName = secondTokenValidation.FindFirst(ClaimTypes.Name)?.Value;
            Assert.AreEqual(firstName, secondName);
            Assert.AreEqual(username, firstName);

            string? firstEmail = firstTokenValidation.FindFirst(ClaimTypes.Email)?.Value;
            string? secondEmail = secondTokenValidation.FindFirst(ClaimTypes.Email)?.Value;
            Assert.AreEqual(firstEmail, secondEmail);
            Assert.AreEqual(email, firstEmail);

            // Both tokens should have user role
            bool firstHasUserRole = firstTokenValidation.IsInRole("user");
            bool secondHasUserRole = secondTokenValidation.IsInRole("user");
            Assert.IsTrue(firstHasUserRole);
            Assert.IsTrue(secondHasUserRole);
        }, TestContext.CancellationToken);
    }

    [TestMethod]
    public Task AuthFlow_RegisterLogoutLogin_ShouldWorkAsync()
    {
        // Arrange
        string email = "MyTestUser@example.com";
        string username = "MyTestUser";
        string password = "P@ssw0rdMyTestUser";

        RegisterRequest registerRequest = new()
        {
            Email = email,
            Username = username,
            Password = password,
            ConfirmPassword = password,
        };

        LoginRequest loginRequest = new()
        {
            Email = email,
            Password = password
        };

        return UsingControllerAsync(registerRequest, async (controller, serviceProvider, ct) =>
        {
            // Act 1 - Register
            IActionResult registerResult = await controller.RegisterAsync(registerRequest, ct);
            Assert.IsInstanceOfType<OkObjectResult>(registerResult);

            // Act 2 - Logout
            IActionResult logoutResult = await controller.LogoutAsync(ct);
            Assert.IsNotNull(logoutResult);
            OkResult logoutOk = Assert.IsInstanceOfType<OkResult>(logoutResult);
            Assert.AreEqual(200, logoutOk.StatusCode);

            // Act 3 - Login again after logout
            IActionResult loginResult = await controller.LoginAsync(loginRequest, ct);

            // Assert - Login should still work after logout
            Assert.IsNotNull(loginResult);
            OkObjectResult loginOk = Assert.IsInstanceOfType<OkObjectResult>(loginResult);
            AuthResponse loginResponse = Assert.IsInstanceOfType<AuthResponse>(loginOk.Value);
            Assert.IsTrue(loginResponse.IsSuccess);
            Assert.IsNotNull(loginResponse.Token);

            // Verify token is valid
            IJwtService jwtService = serviceProvider.GetRequiredService<IJwtService>();
            ClaimsPrincipal tokenValidation = await jwtService.ValidateTokenAsync(loginResponse.Token, ct);
            Assert.IsNotNull(tokenValidation);
        }, TestContext.CancellationToken);
    }

    [TestMethod]
    public Task AuthFlow_MultipleRegistrations_ShouldCreateSeparateUsersAsync()
    {
        // Arrange
        RegisterRequest request1 = new()
        {
            Email = "TestUser1@example.com",
            Username = "TestUser1",
            Password = "P@ssw0rdTestUser1",
            ConfirmPassword = "P@ssw0rdTestUser1",
        };

        RegisterRequest request2 = new()
        {
            Email = "TestUser2@example.com", 
            Username = "TestUser2",
            Password = "P@ssw0rdTestUser2",
            ConfirmPassword = "P@ssw0rdTestUser2",
        };

        return UsingControllerAsync(request1, async (controller, serviceProvider, ct) =>
        {
            // Act - Register two different users
            IActionResult result1 = await controller.RegisterAsync(request1, ct);
            IActionResult result2 = await controller.RegisterAsync(request2, ct);

            // Assert - Both registrations should succeed
            OkObjectResult ok1 = Assert.IsInstanceOfType<OkObjectResult>(result1);
            OkObjectResult ok2 = Assert.IsInstanceOfType<OkObjectResult>(result2);

            AuthResponse response1 = Assert.IsInstanceOfType<AuthResponse>(ok1.Value);
            AuthResponse response2 = Assert.IsInstanceOfType<AuthResponse>(ok2.Value);

            Assert.IsTrue(response1.IsSuccess);
            Assert.IsTrue(response2.IsSuccess);
            Assert.IsNotNull(response1.Token);
            Assert.IsNotNull(response2.Token);

            // Tokens should be different
            Assert.AreNotEqual(response1.Token, response2.Token);

            // Verify both users exist in database
            ApplicationDbContext dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            CloudbbUser? user1 = await dbContext.Set<CloudbbUser>()
                .Include(u => u.IdentityUser)
                .FirstOrDefaultAsync(u => u.IdentityUser.Email == request1.Email, ct);
            CloudbbUser? user2 = await dbContext.Set<CloudbbUser>()
                .Include(u => u.IdentityUser)
                .FirstOrDefaultAsync(u => u.IdentityUser.Email == request2.Email, ct);

            Assert.IsNotNull(user1);
            Assert.IsNotNull(user2);
            Assert.AreNotEqual(user1.Id, user2.Id);
            Assert.AreEqual(request1.Username, user1.IdentityUser.UserName);
            Assert.AreEqual(request2.Username, user2.IdentityUser.UserName);
        }, TestContext.CancellationToken);
    }
}