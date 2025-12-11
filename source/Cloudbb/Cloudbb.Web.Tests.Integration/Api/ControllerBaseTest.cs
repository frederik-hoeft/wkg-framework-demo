using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Cloudbb.Web.Tests.Integration.Api;

public abstract class ControllerBaseTest<TController> : ComponentIntegrationTest<TController> where TController : ControllerBase
{
    public abstract TestContext TestContext { get; set; }

    protected static Task UsingControllerAsync<TRequest>(TRequest request, Func<TController, IServiceProvider, CancellationToken, Task> unitTestTask, CancellationToken cancellationToken = default)
        where TRequest : class => UsingControllerAsync(async (controller, serviceProvider, ct) =>
    {
        ArgumentNullException.ThrowIfNull(unitTestTask);

        controller.TryValidateModel(request);
        await unitTestTask(controller, serviceProvider, ct);
    }, cancellationToken);

    protected static Task UsingControllerAsync(Func<TController, IServiceProvider, CancellationToken, Task> unitTestTask, CancellationToken cancellationToken = default) => 
        UsingComponentAsync(async (controller, serviceProvider, ct) =>
    {
        ArgumentNullException.ThrowIfNull(unitTestTask);

        Mock<HttpContext> httpContextMock = new();
        httpContextMock.SetupGet(c => c.RequestServices).Returns(serviceProvider);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContextMock.Object,
        };

        await unitTestTask(controller, serviceProvider, ct);
    }, cancellationToken);
}
