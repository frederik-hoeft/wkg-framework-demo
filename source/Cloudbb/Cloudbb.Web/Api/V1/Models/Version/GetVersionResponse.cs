namespace Cloudbb.Web.Api.V1.Models.Version;

/// <summary>
/// Response model containing application version information for diagnostic and compatibility purposes.
/// Enables clients to verify API compatibility and track deployment versions in production environments.
/// </summary>
/// <param name="VersionString">Semantic version identifier (e.g., "1.0.0", "2.1.0-beta.3") following SemVer specification.</param>
/// <param name="BuildDate">UTC timestamp indicating when this application build was compiled, useful for deployment tracking.</param>
/// <param name="IsPreRelease">Indicates whether this is a pre-release version (beta, alpha, RC) requiring additional validation before production use.</param>
public sealed record GetVersionResponse(string VersionString, DateTime BuildDate, bool IsPreRelease);