using Cloudbb.Web.Services.Auth.Default;

namespace Cloudbb.Web.Tests.Services.Auth.Default;

[TestClass]
public sealed class CsprngTimingRandomizationServiceTests
{
    private readonly CsprngTimingRandomizationService _service = new();

    public TestContext TestContext { get; set; }

    [TestMethod]
    public async Task DelayAsync_WithPositiveDelay_CompletesBeforeTimeoutAsync()
    {
        using CancellationTokenSource cts = new(TimeSpan.FromSeconds(5));
        await _service.DelayAsync(TimeSpan.FromMilliseconds(10), cts.Token);
    }

    [TestMethod]
    public Task DelayAsync_WithNonPositiveDelay_ThrowsAsync() => 
        Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => _service.DelayAsync(TimeSpan.Zero, TestContext.CancellationToken));

    [TestMethod]
    public async Task DelayAsync_WithCancelledToken_ThrowsOperationCanceledExceptionAsync()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => _service.DelayAsync(TimeSpan.FromMilliseconds(10), cts.Token));
    }
}
