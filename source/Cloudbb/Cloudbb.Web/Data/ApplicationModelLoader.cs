using Wkg.EntityFrameworkCore.Discovery.SourceGeneration;

namespace Cloudbb.Web.Data;

[ModelLoader(AssemblyDiscoveryFailureBehavior = AssemblyDiscoveryFailureBehavior.Error, TargetAssemblies = ["Cloudbb.Web"])]
internal sealed partial class ApplicationModelLoader;
