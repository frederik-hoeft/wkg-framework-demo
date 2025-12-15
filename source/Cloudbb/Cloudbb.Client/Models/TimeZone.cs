namespace Cloudbb.Client.Models;

/// <summary>
/// Represents timezone information for accurate timestamp display to users.
/// </summary>
public sealed class TimeZone
{
    /// <summary>
    /// IANA timezone identifier (e.g., "Europe/Berlin").
    /// </summary>
    public string? IanaTimeZoneId { get; set; }

    /// <summary>
    /// Direct offset from UTC when IANA ID is unavailable (e.g., "01:00:00").
    /// </summary>
    public TimeSpan? TimeZoneOffset { get; set; }

    public TimeZone()
    {
        IanaTimeZoneId = "UTC";
        TimeZoneOffset = TimeSpan.Zero;
    }

    public TimeZone(string? ianaTimeZoneId, TimeSpan? timeZoneOffset = null)
    {
        IanaTimeZoneId = ianaTimeZoneId;
        TimeZoneOffset = timeZoneOffset;
    }
}
