using Cloudbb.Web.Api.V1.Controllers;
using Cloudbb.Web.Data;
using Cloudbb.Web.Data.Model;
using Cloudbb.Web.Services.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.TestAdapters;
using Wkg.AspNetCore.Transactions;

namespace Cloudbb.Web.Tests.Integration;

public abstract class ControllerBaseTest<TController> : TransactionalControllerTest<TController, CloudbbDbContext, IntegrationTestDbInitializer>
    where TController : DatabaseController<CloudbbDbContext>
{
    public abstract TestContext TestContext { get; set; }

    private protected static async ValueTask SetAuthenticatedUserContextAsync(PostsController controller, IServiceProvider serviceProvides, TestUser testUser, CancellationToken cancellationToken)
    {
        IJwtService jwt = serviceProvides.GetRequiredService<IJwtService>();
        ITransactionService<CloudbbDbContext> transaction = serviceProvides.GetRequiredService<ITransactionService<CloudbbDbContext>>();

        await transaction.Scoped.RunReadOnlyAsync(async (dbContext, ct) =>
        {
            CloudbbUser? cloudbbUser = await dbContext.Set<CloudbbUser>().FirstOrDefaultAsync(u => u.Id == testUser.UserId, ct)
                ?? throw new InvalidOperationException("Test user not found in database.");

            IJwtToken token = await jwt.GenerateTokenAsync(cloudbbUser, testUser.Roles, ct);
            string serializedToken = await token.SerializeAsync(ct);
            ClaimsPrincipal claimsPricipal = await jwt.ValidateTokenAsync(serializedToken, ct);
            controller.ControllerContext.HttpContext.User = claimsPricipal;
        }, cancellationToken);
    }
}
