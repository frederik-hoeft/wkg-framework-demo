using System.Security.Cryptography;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class CsprngTimingRandomizationService : ITimingRandomizationService
{
    public Task DelayAsync(TimeSpan maxDelay, CancellationToken cancellationToken = default)
    {
        int maxMilliseconds = (int)maxDelay.TotalMilliseconds;
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxMilliseconds, nameof(maxDelay));
        int delayMilliseconds = RandomNumberGenerator.GetInt32((int)maxDelay.TotalMilliseconds);
        return Task.Delay(delayMilliseconds, cancellationToken);
    }
}