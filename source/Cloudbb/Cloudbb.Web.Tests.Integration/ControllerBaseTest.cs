using Cloudbb.Web.Data;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.TestAdapters;

namespace Cloudbb.Web.Tests.Integration;

public abstract class ControllerBaseTest<TController> : TransactionalControllerTest<TController, ApplicationDbContext, IntegrationTestDbInitializer>
    where TController : DatabaseController<ApplicationDbContext>
{
    public abstract TestContext TestContext { get; set; }
}
