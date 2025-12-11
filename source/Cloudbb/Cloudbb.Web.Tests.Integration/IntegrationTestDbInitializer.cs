using Cloudbb.Web.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Wkg.AspNetCore.Configuration;
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
        services.MockDatabaseTransactions<ApplicationDbContext>();
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
    }

    public static async ValueTask InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        await using ApplicationDbContext context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
        await serviceProvider.InitializeTestDatabaseAsync<IntegrationTestDbLoader>(cancellationToken);
    }
}
