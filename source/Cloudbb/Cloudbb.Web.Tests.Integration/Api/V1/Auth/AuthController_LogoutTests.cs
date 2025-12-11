using Cloudbb.Web.Api.V1.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Cloudbb.Web.Tests.Integration.Api.V1.Auth;

[TestClass]
public sealed class AuthController_LogoutTests : ControllerBaseTest<AuthController>
{
    public override TestContext TestContext { get; set; }

    [TestMethod]
    public Task LogoutAsync_ShouldSucceedAsync()
    {
        return UsingControllerAsync(async (controller, serviceProvider, ct) =>
        {
            // Act
            IActionResult result = await controller.LogoutAsync(ct);

            // Assert
            Assert.IsNotNull(result);
            OkResult ok = Assert.IsInstanceOfType<OkResult>(result);
            Assert.AreEqual(200, ok.StatusCode);
        }, TestContext.CancellationToken);
    }
}