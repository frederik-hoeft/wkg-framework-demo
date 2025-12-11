using Cloudbb.Web.Api.V1.Controllers;
using Cloudbb.Web.Api.V1.Models.Auth;
using Cloudbb.Web.Services.Auth;
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
    public Task RegisterAsync_ShouldSucceed()
    {
        // Arrange
        Guid seed = Guid.CreateVersion7();
        RegisterRequest request = new()
        {
            Email = $"{seed}@example.com",
            // max length is 32, so choose something safe
            Username = $"user_{seed:N}"[..16],
            Password = $"P@ssw0rd{seed}",
            ConfirmPassword = $"P@ssw0rd{seed}",
        };

        return UsingControllerAsync(request, async (controller, serviceProvider, ct) =>
        {
            // Act
            IActionResult result = await controller.RegisterAsync(request, ct);

            // Assert
            Assert.IsNotNull(result);
            OkObjectResult ok = Assert.IsInstanceOfType<OkObjectResult>(result);
            AuthResponse response = Assert.IsInstanceOfType<AuthResponse>(ok.Value);
            Assert.IsTrue(response.IsSuccess);
            Assert.IsNotNull(response.Token);
            // the generated token should be valid
            IJwtService jwtService = serviceProvider.GetRequiredService<IJwtService>();
            ClaimsPrincipal validationResult = await jwtService.ValidateTokenAsync(response.Token, ct);
            Assert.IsNotNull(validationResult);
        }, TestContext.CancellationToken);
    }
}
