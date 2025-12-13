using Blazored.LocalStorage;
using System.Text.Json;

namespace Cloudbb.Client.Services.TimeZone;

internal sealed class LocalStorageTimeZoneService
(
    ILocalStorageService localStorage,
    JsonSerializerOptions jsonOptions
) : ITimeZoneService
{
    private const string TIMEZONE_KEY = "user_timezone";

    public async Task<Models.TimeZone> GetTimeZoneAsync()
    {
        try
        {
            string? timeZoneJson = await localStorage.GetItemAsStringAsync(TIMEZONE_KEY);
            if (timeZoneJson is not null)
            {
                Models.TimeZone? timeZone = JsonSerializer.Deserialize<Models.TimeZone>(timeZoneJson, jsonOptions);
                if (timeZone is not null)
                {
                    return timeZone;
                }
            }
        }
        catch
        {
            // If deserialization fails, return default
        }
        
        // Default to UTC
        return new Models.TimeZone("UTC", TimeSpan.Zero);
    }

    public async Task SetTimeZoneAsync(Models.TimeZone timeZone)
    {
        string timeZoneJson = JsonSerializer.Serialize(timeZone, jsonOptions);
        await localStorage.SetItemAsStringAsync(TIMEZONE_KEY, timeZoneJson);
    }
}
