using Cloudbb.Web.Configuration.Swagger.Attributes;
using System.ComponentModel;

namespace Cloudbb.Web.Api.V1.Models;

/// <summary>
/// Represents timezone information for accurate timestamp display to users.
/// Supports both IANA timezone identifiers and direct offset specification,
/// enabling proper localization of dates across different geographical regions.
/// </summary>
public sealed class TimeZone
{
    /// <summary>
    /// IANA timezone identifier.
    /// </summary>
    [ExampleValue("Europe/Berlin")]
    [DefaultValue("UTC")]
    public string? IanaTimeZoneId { get; set; }

    /// <summary>
    /// Direct offset from UTC when IANA ID is unavailable
    /// </summary>
    [ExampleValue("01:00:00")]
    [DefaultValue("00:00:00")]
    public TimeSpan? TimeZoneOffset { get; set; }

    internal string IanaId => IanaTimeZoneId ?? "UTC";

    internal TimeSpan Offset => TimeZoneOffset ?? TimeSpan.Zero;

    internal TimeZoneInfo GetTimeZoneInfo()
    {
        if (!string.IsNullOrEmpty(IanaTimeZoneId))
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(IanaTimeZoneId);
            }
            catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException) { }
        }
        // normalize offset
        TimeSpan offset = TimeSpan.Zero;
        if (TimeZoneOffset is { } value)
        {
            offset = new TimeSpan(value.Hours, value.Minutes, 0);
        }
        if (offset == TimeSpan.Zero)
        {
            return TimeZoneInfo.Utc;
        }
        return TimeZoneInfo.CreateCustomTimeZone("Custom", offset, "Custom", "Custom");
    }
}
