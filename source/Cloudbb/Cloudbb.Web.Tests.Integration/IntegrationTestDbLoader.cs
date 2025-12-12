using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.TestAdapters.Initialization;

namespace Cloudbb.Web.Tests.Integration;

public sealed class IntegrationTestDbLoader : AsyncTestDatabaseLoader<IntegrationTestDbLoader, ApplicationDbContext>, IAsyncTestDatabaseLoader<ApplicationDbContext>
{
    public async ValueTask InitializeDatabaseAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        // create dummy user
        // First create a user
        UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        IdentityUser identityUser = new()
        {
            UserName = "GlobalTestUser",
            Email = "global-test-user@example.com"
        };
        IdentityResult createResult = await userManager.CreateAsync(identityUser, "P@ssw0rdGlobalTestUser");
        Assert.IsTrue(createResult.Succeeded);
        await userManager.AddToRoleAsync(identityUser, "user");
        CloudbbUser user = new(identityUser);
        dbContext.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}