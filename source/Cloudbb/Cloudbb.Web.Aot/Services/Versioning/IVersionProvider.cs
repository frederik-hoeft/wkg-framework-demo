using Wkg.Versioning;

namespace Cloudbb.Web.Services.Versioning;

public interface IVersionProvider
{
    DeploymentVersionInfo GetVersionInfo();
}
