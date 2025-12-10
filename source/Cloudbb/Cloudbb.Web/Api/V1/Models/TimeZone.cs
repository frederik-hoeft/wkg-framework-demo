using System.ComponentModel;

namespace Cloudbb.Web.Api.V1.Models;

public readonly record struct TimeZone
(
    [DefaultValue("UTC")] string? IanaTimeZoneId = null,
    [DefaultValue("00:00:00")] TimeSpan? TimeZoneOffset = null
)
{
    internal readonly string IanaId => IanaTimeZoneId ?? "UTC";

    internal readonly TimeSpan Offset => TimeZoneOffset ?? TimeSpan.Zero;

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
