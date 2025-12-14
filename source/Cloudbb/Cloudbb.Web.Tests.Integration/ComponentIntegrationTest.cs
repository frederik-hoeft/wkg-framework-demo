using Wkg.AspNetCore.TestAdapters;

namespace Cloudbb.Web.Tests.Integration;

public abstract class ComponentIntegrationTest<TComponent> : ComponentTest<TComponent, IntegrationTestDbInitializer> where TComponent : class;