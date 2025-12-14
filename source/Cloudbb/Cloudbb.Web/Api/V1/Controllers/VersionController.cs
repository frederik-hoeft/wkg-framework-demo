using Cloudbb.Web.Api.V1.Models.Version;
using Cloudbb.Web.Services.Versioning;
using Microsoft.AspNetCore.Mvc;
using Wkg.Versioning;

namespace Cloudbb.Web.Api.V1.Controllers;

/// <summary>
/// Provides endpoints for retrieving application version and diagnostic information.
/// Essential for client compatibility verification, deployment tracking, and operational monitoring.
/// </summary>
public sealed partial class VersionController(IVersionProvider versionProvider) : ControllerBase
{
    public partial GetVersionResponse GetVersion()
    {
        DeploymentVersionInfo versionInfo = versionProvider.GetVersionInfo();
        GetVersionResponse response = new(versionInfo.VersionString, versionInfo.BuildDateUtc, versionInfo.IsPreRelease);
        return response;
    }
}