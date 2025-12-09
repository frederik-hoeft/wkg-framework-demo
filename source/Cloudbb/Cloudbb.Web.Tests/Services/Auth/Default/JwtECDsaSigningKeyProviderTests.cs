using Cloudbb.Web.Services.Auth;
using Cloudbb.Web.Services.Auth.Default;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Security.Cryptography;

namespace Cloudbb.Web.Tests.Services.Auth.Default;

[TestClass]
public sealed class JwtECDsaSigningKeyProviderTests : IDisposable
{
    private readonly Mock<IJwtECDsaSigningKeyImportService> _mockImportService = new();
    private readonly JwtECDsaSigningKeyProvider _provider;

    public TestContext TestContext { get; set; }

    public JwtECDsaSigningKeyProviderTests()
    {
        _provider = new JwtECDsaSigningKeyProvider(_mockImportService.Object);
    }

    [TestMethod]
    public async Task GetKeyAsync_FirstCall_ImportsAndReturnsECDsaSecurityKeyAsync()
    {
        // Arrange
        using ECDsa ecdsa = ECDsa.Create();
        _mockImportService.Setup(x => x.ImportKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ecdsa);

        // Act
        SecurityKey key = await _provider.GetKeyAsync(TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(key);
        Assert.IsInstanceOfType<ECDsaSecurityKey>(key);
        
        ECDsaSecurityKey ecdsaKey = (ECDsaSecurityKey)key;
        Assert.AreEqual(ecdsa, ecdsaKey.ECDsa);
        
        _mockImportService.Verify(x => x.ImportKeyAsync(TestContext.CancellationToken), Times.Once);
    }

    [TestMethod]
    public async Task GetKeyAsync_SubsequentCalls_ReturnsCachedKeyWithoutImportingAsync()
    {
        // Arrange
        using ECDsa ecdsa = ECDsa.Create();
        _mockImportService.Setup(x => x.ImportKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ecdsa);

        // Act
        SecurityKey key1 = await _provider.GetKeyAsync(TestContext.CancellationToken);
        SecurityKey key2 = await _provider.GetKeyAsync(TestContext.CancellationToken);
        SecurityKey key3 = await _provider.GetKeyAsync(TestContext.CancellationToken);

        // Assert
        Assert.AreSame(key1, key2);
        Assert.AreSame(key2, key3);
        
        // Import service should only be called once
        _mockImportService.Verify(x => x.ImportKeyAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetKeyAsync_ConcurrentCalls_OnlyImportsOnceAsync()
    {
        // Arrange
        using ECDsa ecdsa = ECDsa.Create();
        _mockImportService.Setup(x => x.ImportKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ecdsa);

        // Act - Make concurrent calls
        Task<SecurityKey>[] tasks = 
        [
            _provider.GetKeyAsync(TestContext.CancellationToken).AsTask(),
            _provider.GetKeyAsync(TestContext.CancellationToken).AsTask(),
            _provider.GetKeyAsync(TestContext.CancellationToken).AsTask(),
            _provider.GetKeyAsync(TestContext.CancellationToken).AsTask(),
            _provider.GetKeyAsync(TestContext.CancellationToken).AsTask()
        ];

        SecurityKey[] keys = await Task.WhenAll(tasks);

        // Assert
        Assert.HasCount(5, keys);
        
        // All keys should be the same instance (cached)
        for (int i = 1; i < keys.Length; i++)
        {
            Assert.AreSame(keys[0], keys[i]);
        }
        
        // Import service should only be called once despite concurrent access
        _mockImportService.Verify(x => x.ImportKeyAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetKeyAsync_WithCancellationToken_PassesToImportServiceAsync()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        using ECDsa ecdsa = ECDsa.Create();
        _mockImportService.Setup(x => x.ImportKeyAsync(cts.Token))
            .ReturnsAsync(ecdsa);

        // Act
        SecurityKey key = await _provider.GetKeyAsync(cts.Token);

        // Assert
        Assert.IsNotNull(key);
        _mockImportService.Verify(x => x.ImportKeyAsync(cts.Token), Times.Once);
    }

    [TestMethod]
    public async Task GetKeyAsync_WhenImportServiceThrows_PropagatesExceptionAsync()
    {
        // Arrange
        InvalidOperationException expectedException = new("Import failed");
        _mockImportService.Setup(x => x.ImportKeyAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        InvalidOperationException thrownException = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await _provider.GetKeyAsync(TestContext.CancellationToken));
        
        Assert.AreSame(expectedException, thrownException);
    }

    [TestMethod]
    public async Task GetKeyAsync_WhenImportServiceThrowsOnFirstCall_AllowsRetryAsync()
    {
        // Arrange
        ECDsa mockEcdsa = ECDsa.Create();
        _mockImportService.SetupSequence(x => x.ImportKeyAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("First call fails"))
            .ReturnsAsync(mockEcdsa);
        using ECDsa _ = mockEcdsa; // Ensure disposal

        // Act & Assert
        // First call should throw
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await _provider.GetKeyAsync(TestContext.CancellationToken));

        // Second call should succeed
        SecurityKey key = await _provider.GetKeyAsync(TestContext.CancellationToken);
        Assert.IsNotNull(key);
        Assert.IsInstanceOfType<ECDsaSecurityKey>(key);
        
        _mockImportService.Verify(x => x.ImportKeyAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task GetKeyAsync_WithCancelledToken_ThrowsOperationCanceledExceptionAsync()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();
        
        _mockImportService.Setup(x => x.ImportKeyAsync(It.IsAny<CancellationToken>()))
            .Throws<OperationCanceledException>();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            async () => await _provider.GetKeyAsync(cts.Token));
    }

    [TestMethod]
    public void Dispose_DisposesECDsaKey()
    {
        // Arrange
        using ECDsa ecdsa = ECDsa.Create();
        _mockImportService.Setup(x => x.ImportKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ecdsa);

        // Act & Assert - Should not throw
        _provider.Dispose();
        
        // Multiple dispose calls should not throw
        _provider.Dispose();
    }

    [TestMethod]
    public async Task GetKeyAsync_AfterDispose_ThrowsObjectDisposedExceptionAsync()
    {
        // Arrange
        _provider.Dispose();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ObjectDisposedException>(
            async () => await _provider.GetKeyAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task GetKeyAsync_DisposeWhileWaiting_CompletesSuccessfullyAsync()
    {
        // Arrange
        TaskCompletionSource<ECDsa> tcs = new();
        _mockImportService.Setup(x => x.ImportKeyAsync(It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<ECDsa>(tcs.Task));

        // Act
        Task<SecurityKey> getKeyTask = _provider.GetKeyAsync(TestContext.CancellationToken).AsTask();
        
        // Dispose while the import is still pending
        _provider.Dispose();
        
        // Complete the import 
        using ECDsa ecdsa = ECDsa.Create();
        tcs.SetResult(ecdsa);

        // Assert - The task completes successfully even after disposal
        SecurityKey result = await getKeyTask;
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<ECDsaSecurityKey>(result);
    }

    public void Dispose() => _provider.Dispose();
}