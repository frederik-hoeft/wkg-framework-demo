namespace Cloudbb.Web.Api.V1.Models.Version;

public sealed record GetVersionResponse(string VersionString, DateTime BuildDate, bool IsPreRelease);