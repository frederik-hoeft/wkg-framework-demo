using Wkg.Versioning;

namespace Cloudbb.Web.Services.Versioning.Default;

internal sealed class CloudbbWebVersionProvider : IVersionProvider
{
    public DeploymentVersionInfo GetVersionInfo() => CloudbbWeb.VersionInfo;
}
