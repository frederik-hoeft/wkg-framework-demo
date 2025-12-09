using Cloudbb.Web.Services.Auth.Default;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Text;

namespace Cloudbb.Web.Tests.Services.Auth.Default;

[TestClass]
public sealed class JwtSymmetricSigningKeyProviderTests : IDisposable
{
    private readonly Mock<IConfiguration> _mockConfiguration = new();
    private readonly JwtSymmetricSigningKeyProvider _provider;

    public TestContext TestContext { get; set; }

    public JwtSymmetricSigningKeyProviderTests()
    {
        _mockConfiguration.Setup(x => x["Auth:Jwt:Key"]).Returns("test-signing-key-for-jwt-tokens");
        _provider = new JwtSymmetricSigningKeyProvider(_mockConfiguration.Object);
    }

    [TestMethod]
    public async Task GetKeyAsync_WithValidConfiguration_ReturnsSymmetricSecurityKeyAsync()
    {
        // Act
        SecurityKey key = await _provider.GetKeyAsync(TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(key);
        Assert.IsInstanceOfType<SymmetricSecurityKey>(key);
        
        SymmetricSecurityKey symmetricKey = (SymmetricSecurityKey)key;
        byte[] expectedKeyBytes = Encoding.UTF8.GetBytes("test-signing-key-for-jwt-tokens");
        CollectionAssert.AreEqual(expectedKeyBytes, symmetricKey.Key);
    }

    [TestMethod]
    public async Task GetKeyAsync_WithDifferentKeys_ReturnsDifferentKeysAsync()
    {
        // Arrange
        Mock<IConfiguration> mockConfig1 = new();
        mockConfig1.Setup(x => x["Auth:Jwt:Key"]).Returns("first-key");
        using JwtSymmetricSigningKeyProvider provider1 = new(mockConfig1.Object);

        Mock<IConfiguration> mockConfig2 = new();
        mockConfig2.Setup(x => x["Auth:Jwt:Key"]).Returns("second-key");
        using JwtSymmetricSigningKeyProvider provider2 = new(mockConfig2.Object);

        // Act
        SecurityKey key1 = await provider1.GetKeyAsync(TestContext.CancellationToken);
        SecurityKey key2 = await provider2.GetKeyAsync(TestContext.CancellationToken);

        // Assert
        Assert.IsInstanceOfType<SymmetricSecurityKey>(key1);
        Assert.IsInstanceOfType<SymmetricSecurityKey>(key2);
        
        SymmetricSecurityKey symmetricKey1 = (SymmetricSecurityKey)key1;
        SymmetricSecurityKey symmetricKey2 = (SymmetricSecurityKey)key2;
        
        CollectionAssert.AreNotEqual(symmetricKey1.Key, symmetricKey2.Key);
    }

    [TestMethod]
    public async Task GetKeyAsync_MultipleCalls_ReturnsSameKeyAsync()
    {
        // Act
        SecurityKey key1 = await _provider.GetKeyAsync(TestContext.CancellationToken);
        SecurityKey key2 = await _provider.GetKeyAsync(TestContext.CancellationToken);

        // Assert
        Assert.IsInstanceOfType<SymmetricSecurityKey>(key1);
        Assert.IsInstanceOfType<SymmetricSecurityKey>(key2);
        
        SymmetricSecurityKey symmetricKey1 = (SymmetricSecurityKey)key1;
        SymmetricSecurityKey symmetricKey2 = (SymmetricSecurityKey)key2;
        
        CollectionAssert.AreEqual(symmetricKey1.Key, symmetricKey2.Key);
    }

    [TestMethod]
    public async Task GetKeyAsync_WithEmptyKey_ThrowsArgumentExceptionAsync()
    {
        // Arrange
        Mock<IConfiguration> mockConfig = new();
        mockConfig.Setup(x => x["Auth:Jwt:Key"]).Returns("");
        using JwtSymmetricSigningKeyProvider provider = new(mockConfig.Object);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await provider.GetKeyAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task GetKeyAsync_WithNullKey_ThrowsArgumentNullExceptionAsync()
    {
        // Arrange
        Mock<IConfiguration> mockConfig = new();
        mockConfig.Setup(x => x["Auth:Jwt:Key"]).Returns((string?)null);
        using JwtSymmetricSigningKeyProvider provider = new(mockConfig.Object);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await provider.GetKeyAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task GetKeyAsync_WithUnicodeCharacters_HandlesCorrectlyAsync()
    {
        // Arrange
        string unicodeKey = "key-with-unicode-\u00E4\u00F6\u00FC-characters";
        Mock<IConfiguration> mockConfig = new();
        mockConfig.Setup(x => x["Auth:Jwt:Key"]).Returns(unicodeKey);
        using JwtSymmetricSigningKeyProvider provider = new(mockConfig.Object);

        // Act
        SecurityKey key = await provider.GetKeyAsync(TestContext.CancellationToken);

        // Assert
        Assert.IsInstanceOfType<SymmetricSecurityKey>(key);
        SymmetricSecurityKey symmetricKey = (SymmetricSecurityKey)key;
        byte[] expectedBytes = Encoding.UTF8.GetBytes(unicodeKey);
        CollectionAssert.AreEqual(expectedBytes, symmetricKey.Key);
    }

    [TestMethod]
    public async Task GetKeyAsync_WithCancellationToken_IgnoresCancellationAsync()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        // Act - Should not throw despite cancelled token since operation is synchronous
        SecurityKey key = await _provider.GetKeyAsync(cts.Token);

        // Assert
        Assert.IsNotNull(key);
        Assert.IsInstanceOfType<SymmetricSecurityKey>(key);
    }

    [TestMethod]
    public void MultiDispose_DoesNotThrow()
    {
        // Act & Assert - Should not throw
        _provider.Dispose();
        
        // Multiple dispose calls should also not throw
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

    public void Dispose() => _provider.Dispose();
}