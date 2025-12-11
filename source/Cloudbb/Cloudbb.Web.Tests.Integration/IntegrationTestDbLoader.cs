using Cloudbb.Web.Data;
using Wkg.AspNetCore.TestAdapters.Initialization;

namespace Cloudbb.Web.Tests.Integration;

public sealed class IntegrationTestDbLoader : AsyncTestDatabaseLoader<IntegrationTestDbLoader, ApplicationDbContext>, IAsyncTestDatabaseLoader<ApplicationDbContext>
{
    public ValueTask InitializeDatabaseAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        // Seed test data here
        return ValueTask.CompletedTask;
    }
}