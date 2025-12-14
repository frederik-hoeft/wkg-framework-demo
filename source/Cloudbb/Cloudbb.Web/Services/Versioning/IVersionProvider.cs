using Wkg.Versioning;

namespace Cloudbb.Web.Services.Versioning;

/// <summary>
/// Provides access to application deployment version information for diagnostic and monitoring purposes.
/// Abstracts version data retrieval to enable testing and different versioning strategies across environments.
/// </summary>
public interface IVersionProvider
{
    /// <summary>
    /// Retrieves comprehensive version information including semantic version, build metadata, and release classification.
    /// Used by version endpoints, health checks, and logging systems to identify the running application instance.
    /// </summary>
    /// <returns>Complete deployment version details including build timestamp and pre-release indicators.</returns>
    DeploymentVersionInfo GetVersionInfo();
}
