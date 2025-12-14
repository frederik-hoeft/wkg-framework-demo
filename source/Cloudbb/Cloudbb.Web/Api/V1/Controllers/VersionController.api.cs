using Asp.Versioning;
using Cloudbb.Web.Api.V1.Models.Version;
using Microsoft.AspNetCore.Mvc;

namespace Cloudbb.Web.Api.V1.Controllers;

[ApiController]
[ApiVersion(API_V1)]
[Route("api/v{version:apiVersion}/version")]
public partial class VersionController
{
    [HttpGet]
    [ProducesResponseType<GetVersionResponse>(StatusCodes.Status200OK)]
    public partial GetVersionResponse GetVersion();
}
