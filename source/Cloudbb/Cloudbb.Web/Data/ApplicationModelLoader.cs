using Wkg.EntityFrameworkCore.Discovery.SourceGeneration;

namespace Cloudbb.Web.Data;

/// <summary>
/// Discovers the Entity Framework Core entity models defined in the Cloudbb.Web assembly during compilation and emits
/// a source-generated <see cref="IModelLoader"/> implementation for loading them into an EF Core <see cref="DbContext"/>.
/// </summary>
// see https://github.com/WKG-Software-GmbH/wkg-entity-framework-core/blob/main/docs/documentation.md#source-generator-discovery
// TODO: .NET 10 release not yet mirrored to GitHub.
[ModelLoader(AssemblyDiscoveryFailureBehavior = AssemblyDiscoveryFailureBehavior.Error, TargetAssemblies = ["Cloudbb.Web"])]
internal sealed partial class ApplicationModelLoader;