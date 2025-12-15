using Cloudbb.Client.Models;

namespace Cloudbb.Client.Services.TimeZone;

/// <summary>
/// Service for managing user's timezone preference stored in browser local storage.
/// </summary>
public interface ITimeZoneService
{
    /// <summary>
    /// Gets the user's timezone preference.
    /// </summary>
    Task<Models.TimeZone> GetTimeZoneAsync();

    /// <summary>
    /// Sets the user's timezone preference.
    /// </summary>
    Task SetTimeZoneAsync(Models.TimeZone timeZone);
}
