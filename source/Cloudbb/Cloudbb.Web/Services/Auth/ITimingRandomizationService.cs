namespace Cloudbb.Web.Services.Auth;

/// <summary>
/// Service for introducing random delays to mitigate timing attacks.
/// </summary>
public interface ITimingRandomizationService
{
    /// <summary>
    /// Introduces a random delay up to the specified maximum duration.
    /// </summary>
    /// <param name="maxDelay">The maximum delay duration.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous delay operation.</returns>
    Task DelayAsync(TimeSpan maxDelay, CancellationToken cancellationToken = default);
}