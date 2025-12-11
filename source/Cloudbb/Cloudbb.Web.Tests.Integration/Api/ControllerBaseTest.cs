using Cloudbb.Web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Wkg.AspNetCore.Transactions;

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
        UsingComponentAsync((controller, serviceProvider, ct1) => 
            serviceProvider.GetRequiredService<ITransactionService<ApplicationDbContext>>().Scoped.RunReadOnlyAsync(async (_, ct2) =>
            {
                ArgumentNullException.ThrowIfNull(unitTestTask);
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext!
                };
                await unitTestTask(controller, serviceProvider, ct2);
            },ct1)
        ,cancellationToken);
}
