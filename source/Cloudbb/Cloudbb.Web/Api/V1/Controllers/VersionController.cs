using Cloudbb.Web.Api.V1.Models.Version;
using Cloudbb.Web.Services.Versioning;
using Microsoft.AspNetCore.Mvc;
using Wkg.Versioning;

namespace Cloudbb.Web.Api.V1.Controllers;

/// <summary>
/// Provides endpoints for managing posts.
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