using Cloudbb.Web.Services.Versioning;
using Wkg.Versioning;

namespace Cloudbb.Web.Aot.Services.Versioning.Default;

internal sealed class CloudbbWebAotVersionProvider : IVersionProvider
{
    public DeploymentVersionInfo GetVersionInfo() => CloudbbWebAot.VersionInfo;
}
