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
public sealed class AuthController_LoginTests : ControllerBaseTest<AuthController>
{
    public override TestContext TestContext { get; set; }

    [TestMethod]
    public Task LoginAsync_WithValidCredentials_ShouldSucceedAsync()
    {
        // Arrange
        string email = "MyTestUser@example.com";
        string password = "P@ssw0rdMyTestUser";

        LoginRequest request = new()
        {
            Email = email,
            Password = password
        };

        return UsingControllerAsync(request, async (controller, serviceProvider, ct) =>
        {
            // First create a user to login with
            UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            ApplicationDbContext dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            
            IdentityUser identityUser = new()
            {
                UserName = "MyTestUser",
                Email = email
            };
            IdentityResult createResult = await userManager.CreateAsync(identityUser, password);
            Assert.IsTrue(createResult.Succeeded, string.Join("; ", createResult.Errors.Select(e => e.Description)));
            
            await userManager.AddToRoleAsync(identityUser, "user");
            
            CloudbbUser user = new(identityUser);
            dbContext.Add(user);
            await dbContext.SaveChangesAsync(ct);

            // Act
            IActionResult result = await controller.LoginAsync(request, ct);

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
        }, TestContext.CancellationToken);
    }

    [TestMethod]
    public Task LoginAsync_WithInvalidEmail_ShouldReturnUnauthorizedAsync()
    {
        // Arrange
        LoginRequest request = new()
        {
            Email = "nonexistent@example.com",
            Password = "P@ssw0rdMyTestUser"
        };

        return UsingControllerAsync(request, async (controller, serviceProvider, ct) =>
        {
            // Act
            IActionResult result = await controller.LoginAsync(request, ct);

            // Assert
            Assert.IsNotNull(result);
            UnauthorizedObjectResult unauthorized = Assert.IsInstanceOfType<UnauthorizedObjectResult>(result);
            AuthResponse response = Assert.IsInstanceOfType<AuthResponse>(unauthorized.Value);
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.Token);
            Assert.IsNotNull(response.Message);
            Assert.AreEqual("Invalid email or password", response.Message);
        }, TestContext.CancellationToken);
    }

    [TestMethod]
    public Task LoginAsync_WithInvalidPassword_ShouldReturnUnauthorizedAsync()
    {
        // Arrange
        string email = "test@example.com";
        string correctPassword = "P@ssw0rdMyTestUser";
        string wrongPassword = "WrongP@ssMyTestUser";

        LoginRequest request = new()
        {
            Email = email,
            Password = wrongPassword
        };

        return UsingControllerAsync(request, async (controller, serviceProvider, ct) =>
        {
            // First create a user
            UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            ApplicationDbContext dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            
            IdentityUser identityUser = new()
            {
                UserName = "MyTestUser",
                Email = email
            };
            IdentityResult createResult = await userManager.CreateAsync(identityUser, correctPassword);
            Assert.IsTrue(createResult.Succeeded);
            
            await userManager.AddToRoleAsync(identityUser, "user");
            
            CloudbbUser user = new(identityUser);
            dbContext.Add(user);
            await dbContext.SaveChangesAsync(ct);

            // Act
            IActionResult result = await controller.LoginAsync(request, ct);

            // Assert
            Assert.IsNotNull(result);
            UnauthorizedObjectResult unauthorized = Assert.IsInstanceOfType<UnauthorizedObjectResult>(result);
            AuthResponse response = Assert.IsInstanceOfType<AuthResponse>(unauthorized.Value);
            Assert.IsFalse(response.IsSuccess);
            Assert.IsNull(response.Token);
            Assert.IsNotNull(response.Message);
            Assert.AreEqual("Invalid email or password", response.Message);
        }, TestContext.CancellationToken);
    }

    [TestMethod]
    public Task LoginAsync_WithInvalidModel_ShouldReturnBadRequestAsync()
    {
        // Arrange
        LoginRequest request = new()
        {
            Email = "", // Invalid empty email
            Password = "somepassword"
        };

        return UsingControllerAsync(request, async (controller, serviceProvider, ct) =>
        {
            // Act
            IActionResult result = await controller.LoginAsync(request, ct);

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
}