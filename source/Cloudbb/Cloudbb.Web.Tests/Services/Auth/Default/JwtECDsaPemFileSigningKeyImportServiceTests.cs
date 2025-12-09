using Cloudbb.Web.Services.Auth.Default;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace Cloudbb.Web.Tests.Services.Auth.Default;

[TestClass]
public sealed class JwtECDsaPemFileSigningKeyImportServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration = new();
    private readonly JwtECDsaPemFileSigningKeyImportService _service;
    private readonly DirectoryInfo _testKeyDirectory;

    // Sample ECDSA P-256 private key for testing (in PEM format)
    private const string SAMPLE_ECDSA_PRIVATE_KEY_PEM = """
        -----BEGIN PRIVATE KEY-----
        MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQg1gxUSCeEidoNMfAT
        o4iN9vCXWml3jrceXnjD+xehVaqhRANCAAS0dSAoyjq6zwozvHGaAYOR9oTLXvPd
        fJOmRfn4Iony/ysiZjKQi5RNuHAbNyUTrbTq03Qn8pYjsVw5uqbFQHSr
        -----END PRIVATE KEY-----
        """;

    public TestContext TestContext { get; set; }

    public JwtECDsaPemFileSigningKeyImportServiceTests()
    {
        _service = new JwtECDsaPemFileSigningKeyImportService(_mockConfiguration.Object);
        string testKeyDirectory = Path.Combine(Path.GetTempPath(), "JwtECDsaTests", Guid.NewGuid().ToString());
        _testKeyDirectory = Directory.CreateDirectory(testKeyDirectory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_testKeyDirectory.Exists)
        {
            _testKeyDirectory.Delete(recursive: true);
        }
    }

    [TestMethod]
    public async Task ImportKeyAsync_WithValidPemFile_ReturnsECDsaKeyAsync()
    {
        // Arrange
        string pemFilePath = Path.Combine(_testKeyDirectory.FullName, "test-key.pem");
        await File.WriteAllTextAsync(pemFilePath, SAMPLE_ECDSA_PRIVATE_KEY_PEM, Encoding.UTF8, TestContext.CancellationToken);
        
        _mockConfiguration.Setup(x => x["Auth:Jwt:ECDsaPrivateKeyFile"])
            .Returns(pemFilePath);

        // Act
        using ECDsa ecdsa = await _service.ImportKeyAsync(TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(ecdsa);
        
        // Verify we can use the key for signing/verification
        byte[] testData = "test data"u8.ToArray();
        byte[] signature = ecdsa.SignData(testData, HashAlgorithmName.SHA256);
        bool isValid = ecdsa.VerifyData(testData, signature, HashAlgorithmName.SHA256);
        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public async Task ImportKeyAsync_WithNullConfiguration_ThrowsInvalidOperationExceptionAsync()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["Auth:Jwt:ECDsaPrivateKeyFile"])
            .Returns((string?)null);

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => _service.ImportKeyAsync(TestContext.CancellationToken).AsTask());
        
        Assert.AreEqual("ECDSA private key file path is not configured.", exception.Message);
    }

    [TestMethod]
    public async Task ImportKeyAsync_WithEmptyConfiguration_ThrowsInvalidOperationExceptionAsync()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["Auth:Jwt:ECDsaPrivateKeyFile"])
            .Returns(string.Empty);

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await _service.ImportKeyAsync(TestContext.CancellationToken));
        
        Assert.AreEqual("ECDSA private key file path is not configured.", exception.Message);
    }

    [TestMethod]
    public async Task ImportKeyAsync_WithWhitespaceConfiguration_ThrowsInvalidOperationExceptionAsync()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["Auth:Jwt:ECDsaPrivateKeyFile"])
            .Returns("   \t\r\n   ");

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await _service.ImportKeyAsync(TestContext.CancellationToken));
        
        Assert.AreEqual("ECDSA private key file path is not configured.", exception.Message);
    }

    [TestMethod]
    public async Task ImportKeyAsync_WithNonExistentFile_ThrowsFileNotFoundExceptionAsync()
    {
        // Arrange
        string nonExistentPath = Path.Combine(_testKeyDirectory.FullName, "non-existent.pem");
        _mockConfiguration.Setup(x => x["Auth:Jwt:ECDsaPrivateKeyFile"])
            .Returns(nonExistentPath);

        // Act & Assert
        FileNotFoundException exception = await Assert.ThrowsExactlyAsync<FileNotFoundException>(
            async () => await _service.ImportKeyAsync(TestContext.CancellationToken));
        
        Assert.AreEqual("ECDSA private key file not found.", exception.Message);
        Assert.AreEqual(nonExistentPath, exception.FileName);
    }

    [TestMethod]
    public async Task ImportKeyAsync_WithInvalidPemContent_ThrowsCryptographicExceptionAsync()
    {
        // Arrange
        string pemFilePath = Path.Combine(_testKeyDirectory.FullName, "invalid-key.pem");
        await File.WriteAllTextAsync(pemFilePath, "invalid pem content", Encoding.UTF8, TestContext.CancellationToken);
        
        _mockConfiguration.Setup(x => x["Auth:Jwt:ECDsaPrivateKeyFile"])
            .Returns(pemFilePath);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            () => _service.ImportKeyAsync(TestContext.CancellationToken).AsTask());
    }

    [TestMethod]
    public async Task ImportKeyAsync_WithRsaKeyInsteadOfEcdsa_ThrowsCryptographicExceptionAsync()
    {
        // Arrange - Use an RSA key instead of ECDSA
        string rsaPrivateKey = """
            -----BEGIN PRIVATE KEY-----
            MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQDG0wLWbvWj8aCg
            HEBpJFj7R8u/7A+8Ng8a8LPY8z8w5Dw8z8w8w8w8w8w8w8w8w8w8w8w8w8w8w8
            EAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
            -----END PRIVATE KEY-----
            """;
        
        string pemFilePath = Path.Combine(_testKeyDirectory.FullName, "rsa-key.pem");
        await File.WriteAllTextAsync(pemFilePath, rsaPrivateKey, Encoding.UTF8, TestContext.CancellationToken);
        
        _mockConfiguration.Setup(x => x["Auth:Jwt:ECDsaPrivateKeyFile"])
            .Returns(pemFilePath);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<CryptographicException>(
            async () => await _service.ImportKeyAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task ImportKeyAsync_WithCancellationToken_RespectsTokenAsync()
    {
        // Arrange
        string pemFilePath = Path.Combine(_testKeyDirectory.FullName, "test-key.pem");
        await File.WriteAllTextAsync(pemFilePath, SAMPLE_ECDSA_PRIVATE_KEY_PEM, Encoding.UTF8, TestContext.CancellationToken);
        
        _mockConfiguration.Setup(x => x["Auth:Jwt:ECDsaPrivateKeyFile"])
            .Returns(pemFilePath);

        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<TaskCanceledException>(
            async () => await _service.ImportKeyAsync(cts.Token));
    }    

    [TestMethod]
    public async Task ImportKeyAsync_WithDifferentPemFormats_HandlesCorrectlyAsync()
    {
        // Test with different line endings and whitespace
        string pemWithDifferentFormatting = SAMPLE_ECDSA_PRIVATE_KEY_PEM
            .Replace("\n", "\r\n")
            .Insert(0, "\r\n\r\n")
            + "\r\n\r\n";
        
        // Arrange
        string pemFilePath = Path.Combine(_testKeyDirectory.FullName, "formatted-key.pem");
        await File.WriteAllTextAsync(pemFilePath, pemWithDifferentFormatting, Encoding.UTF8, TestContext.CancellationToken);
        
        _mockConfiguration.Setup(x => x["Auth:Jwt:ECDsaPrivateKeyFile"])
            .Returns(pemFilePath);

        // Act
        using ECDsa ecdsa = await _service.ImportKeyAsync(TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(ecdsa);
        
        // Verify functionality
        byte[] testData = "test data"u8.ToArray();
        byte[] signature = ecdsa.SignData(testData, HashAlgorithmName.SHA256);
        bool isValid = ecdsa.VerifyData(testData, signature, HashAlgorithmName.SHA256);
        Assert.IsTrue(isValid);
    }

    [TestMethod]
    public async Task ImportKeyAsync_MultipleCalls_ReturnsNewInstancesAsync()
    {
        // Arrange
        string pemFilePath = Path.Combine(_testKeyDirectory.FullName, "test-key.pem");
        await File.WriteAllTextAsync(pemFilePath, SAMPLE_ECDSA_PRIVATE_KEY_PEM, Encoding.UTF8, TestContext.CancellationToken);
        
        _mockConfiguration.Setup(x => x["Auth:Jwt:ECDsaPrivateKeyFile"])
            .Returns(pemFilePath);

        // Act
        using ECDsa ecdsa1 = await _service.ImportKeyAsync(TestContext.CancellationToken);
        using ECDsa ecdsa2 = await _service.ImportKeyAsync(TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(ecdsa1);
        Assert.IsNotNull(ecdsa2);
        Assert.AreNotSame(ecdsa1, ecdsa2);
        
        // Both should be functional
        byte[] testData = "test data"u8.ToArray();
        byte[] signature1 = ecdsa1.SignData(testData, HashAlgorithmName.SHA256);
        byte[] signature2 = ecdsa2.SignData(testData, HashAlgorithmName.SHA256);
        
        Assert.IsTrue(ecdsa1.VerifyData(testData, signature1, HashAlgorithmName.SHA256));
        Assert.IsTrue(ecdsa2.VerifyData(testData, signature2, HashAlgorithmName.SHA256));
        
        // Cross-validation should also work (same private key)
        Assert.IsTrue(ecdsa1.VerifyData(testData, signature2, HashAlgorithmName.SHA256));
        Assert.IsTrue(ecdsa2.VerifyData(testData, signature1, HashAlgorithmName.SHA256));
    }
}