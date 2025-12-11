using Cloudbb.Web.Data;
using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.TestAdapters;

namespace Cloudbb.Web.Tests.Integration;

[TestClass]
[SuppressMessage("Design", "MSTEST0016:Test class should have test method", Justification = "Database initializer class")]
public sealed class DatabaseInitializer : DbContextTest<ApplicationDbContext, IntegrationTestDbInitializer>
{
    [AssemblyInitialize]
    public static Task InitializeDbAsync(TestContext testContext)
    {
        ArgumentNullException.ThrowIfNull(testContext);
        return UsingDbContextAsync((dbContext, ct) => Task.CompletedTask, testContext.CancellationToken);
    }
}
