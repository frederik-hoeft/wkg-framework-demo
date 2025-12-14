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
    public Task LoginAsync_WithValidCredentials_ShouldSucceedAsync() => UsingComponentAsync(new LoginRequest()
    {
        Email = "MyTestUser@example.com",
        Password = "P@ssw0rdMyTestUser"
    }, async (controller, request, serviceProvider, ct) =>
    {
        // First create a user to login with
        UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        CloudbbDbContext dbContext = serviceProvider.GetRequiredService<CloudbbDbContext>();
            
        IdentityUser identityUser = new()
        {
            UserName = "MyTestUser",
            Email = request.Email
        };
        IdentityResult createResult = await userManager.CreateAsync(identityUser, request.Password);
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

    [TestMethod]
    public Task LoginAsync_WithInvalidEmail_ShouldReturnUnauthorizedAsync() => UsingComponentAsync(new LoginRequest()
    {
        Email = "nonexistent@example.com",
        Password = "P@ssw0rdMyTestUser"
    }, async (controller, request, serviceProvider, ct) =>
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

    [TestMethod]
    public Task LoginAsync_WithInvalidPassword_ShouldReturnUnauthorizedAsync() => UsingComponentAsync(new LoginRequest()
    {
        Email = "test@example.com",
        Password = "WrongP@ssMyTestUser"
    }, async (controller, request, serviceProvider, ct) =>
    {
        // First create a user
        UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        CloudbbDbContext dbContext = serviceProvider.GetRequiredService<CloudbbDbContext>();
            
        IdentityUser identityUser = new()
        {
            UserName = "MyTestUser",
            Email = request.Email
        };
        IdentityResult createResult = await userManager.CreateAsync(identityUser, "P@ssw0rdMyTestUser");
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

    [TestMethod]
    public Task LoginAsync_WithInvalidModel_ShouldReturnBadRequestAsync() => UsingComponentAsync(new LoginRequest()
    {
        Email = "", // Invalid empty email
        Password = "somepassword"
    }, async (controller, request, serviceProvider, ct) =>
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