using Wkg.Versioning;

namespace Cloudbb.Web.Aot;

internal sealed class CloudbbWebAot() : DeploymentVersionInfo(CI_DEPLOYMENT__VERSION_PREFIX, CI_DEPLOYMENT__VERSION_SUFFIX, CI_DEPLOYMENT__DATETIME_UTC)
{
    private const string CI_DEPLOYMENT__VERSION_PREFIX = "0.0.0";
    private const string CI_DEPLOYMENT__VERSION_SUFFIX = "CI-INJECTED";
    private const string CI_DEPLOYMENT__DATETIME_UTC = "1970-01-01 00:00:00";

    /// <summary>
    /// Provides version information for the Wkg.EntityFrameworkCore framework.
    /// </summary>
    public static CloudbbWebAot VersionInfo { get; } = new CloudbbWebAot();
}
