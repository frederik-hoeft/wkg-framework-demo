using Asp.Versioning;
using Cloudbb.Web.Api.V1.Models.Version;
using Microsoft.AspNetCore.Mvc;

namespace Cloudbb.Web.Api.V1.Controllers;

[ApiController]
[ApiVersion(API_V1)]
[Route("api/v{version:apiVersion}/version")]
public partial class VersionController
{
    /// <summary>
    /// Retrieves the current application version information including build date and pre-release status.
    /// Used for diagnostic purposes, client compatibility checks, and operational monitoring.
    /// </summary>
    /// <returns>Version details including semantic version string, UTC build timestamp, and release status.</returns>
    [HttpGet]
    [ProducesResponseType<GetVersionResponse>(StatusCodes.Status200OK)]
    public partial GetVersionResponse GetVersion();
}
