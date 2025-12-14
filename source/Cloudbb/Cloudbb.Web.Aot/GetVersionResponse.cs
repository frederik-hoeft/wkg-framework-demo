namespace Cloudbb.Web.Aot;

internal sealed record GetVersionResponse(string VersionString, DateTime BuildDate, bool IsPreRelease);