using Cloudbb.Web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wkg.AspNetCore.Configuration;
using Wkg.AspNetCore.Delegates;
using Wkg.AspNetCore.ErrorHandling;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.TestAdapters.Initialization;
using Wkg.AspNetCore.TestAdapters.Initialization.Extensions;

namespace Cloudbb.Web.Tests.Integration;

public sealed class IntegrationTestDbInitializer : IAsyncDITestInitializer
{
    public static async ValueTask ConfigureAsync(IServiceCollection services, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(services);
        // apply appsettings.Testing.json configuration
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        IConfiguration configuration = await services.ConfigureUsingAsync<Startup>(cancellationToken: cancellationToken);
        services.MockDatabaseTransactions<CloudbbDbContext>();
        // since we're not running in a full WebApplication context, we need to register the configuration and some required services manually
        services.AddSingleton(configuration);
        services.AddLogging(logging => logging.AddConsole());
        services.AddScoped<IHttpContextAccessor, HttpContextAccessor>(serviceProvider => new HttpContextAccessor()
        {
            HttpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            }
        });
        services.AddSingleton<IErrorSentry, NoopErrorSentry>();
    }

    public static async ValueTask InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        await using CloudbbDbContext context = serviceProvider.GetRequiredService<CloudbbDbContext>();
        await context.Database.EnsureDeletedAsync(cancellationToken);
        await context.Database.MigrateAsync(cancellationToken);
        await serviceProvider.InitializeTestDatabaseAsync<IntegrationTestDbLoader>(cancellationToken);
    }

    private sealed class NoopErrorSentry : IErrorSentry
    {
        public ApiProxyException AfterHandled(Exception e) => throw e;

        public IActionResult Watch(RequestAction<IActionResult> action) => action();

        public void Watch(RequestAction action) => action();

        public TResult Watch<TResult>(RequestAction<TResult> action) => action();

        public Task<IActionResult> WatchAsync(RequestTask<IActionResult> task) => task();

        public Task WatchAsync(RequestTask task) => task();

        public Task<TResult> WatchAsync<TResult>(RequestTask<TResult> task) => task();
    }
}
