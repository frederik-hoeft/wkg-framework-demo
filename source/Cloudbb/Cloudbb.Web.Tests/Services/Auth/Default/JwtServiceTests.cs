using Cloudbb.Web.Data.Model;
using Cloudbb.Web.Services.Auth;
using Cloudbb.Web.Services.Auth.Default;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cloudbb.Web.Tests.Services.Auth.Default;

[TestClass]
public sealed class JwtServiceTests
{
    private readonly Mock<IConfiguration> _mockConfiguration = new();
    private readonly Mock<IJwtSigningKeyProvider> _mockSigningKeyProvider = new();
    private readonly Mock<IJwtAlgorithmProvider> _mockAlgorithmProvider = new();
    private readonly JwtService _jwtService;
    private readonly SymmetricSecurityKey _testKey = new(Encoding.UTF8.GetBytes("test-key-that-is-long-enough-for-hmac-sha256"));
    private readonly CloudbbUser _testUser;

    public TestContext TestContext { get; set; }

    public JwtServiceTests()
    {
        SetupMockConfiguration();
        SetupMockDependencies();
        _jwtService = new JwtService(_mockConfiguration.Object, _mockSigningKeyProvider.Object, _mockAlgorithmProvider.Object);

        // Create test user with Identity user
        IdentityUser identityUser = new()
        {
            Id = "test-id",
            UserName = "testuser",
            Email = "test@example.com"
        };
        _testUser = new CloudbbUser(identityUser)
        {
            Id = new Guid("12345678-1234-1234-1234-123456789012") // Set a specific GUID for testing
        };
    }

    [TestMethod]
    public async Task GenerateTokenAsync_WithValidUser_ReturnsJwtTokenAsync()
    {
        // Arrange
        string[] roles = ["User", "Admin"];

        // Act
        IJwtToken token = await _jwtService.GenerateTokenAsync(_testUser, roles, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(token);
        Assert.IsNotNull(token.Token);
        Assert.AreEqual("test-issuer", token.Token.Issuer);
        Assert.AreEqual("test-audience", token.Token.Audiences.First());
        
        // Verify claims
        Claim[] claims = [.. token.Token.Claims];
        Assert.IsTrue(claims.Any(c => c.Type == ClaimTypes.NameIdentifier && c.Value == "12345678-1234-1234-1234-123456789012"));
        Assert.IsTrue(claims.Any(c => c.Type == ClaimTypes.Name && c.Value == "testuser"));
        Assert.IsTrue(claims.Any(c => c.Type == ClaimTypes.Email && c.Value == "test@example.com"));
        Assert.IsTrue(claims.Any(c => c.Type == JwtRegisteredClaimNames.Jti));
        Assert.IsTrue(claims.Any(c => c.Type == JwtRegisteredClaimNames.Iat));
        Assert.IsTrue(claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "User"));
        Assert.IsTrue(claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin"));
    }

    [TestMethod]
    public async Task GenerateTokenAsync_WithEmptyRoles_ReturnsTokenWithoutRoleClaimsAsync()
    {
        // Arrange
        string[] roles = [];

        // Act
        IJwtToken token = await _jwtService.GenerateTokenAsync(_testUser, roles, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(token);
        Claim[] roleClaims = [.. token.Token.Claims.Where(c => c.Type == ClaimTypes.Role)];
        Assert.IsEmpty(roleClaims);
    }

    [TestMethod]
    public async Task GenerateTokenAsync_WithMultipleRoles_ReturnsTokenWithAllRoleClaimsAsync()
    {
        // Arrange
        string[] roles = ["User", "Admin", "Moderator"];

        // Act
        IJwtToken token = await _jwtService.GenerateTokenAsync(_testUser, roles, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(token);
        Claim[] roleClaims = [.. token.Token.Claims.Where(c => c.Type == ClaimTypes.Role)];
        Assert.HasCount(3, roleClaims);
        Assert.IsTrue(roleClaims.Any(c => c.Value == "User"));
        Assert.IsTrue(roleClaims.Any(c => c.Value == "Admin"));
        Assert.IsTrue(roleClaims.Any(c => c.Value == "Moderator"));
    }

    [TestMethod]
    public async Task GenerateTokenAsync_WithCancellationToken_PassesToSigningKeyProviderAsync()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        string[] roles = ["User"];

        // Act
        IJwtToken token = await _jwtService.GenerateTokenAsync(_testUser, roles, cts.Token);

        // Assert
        _mockSigningKeyProvider.Verify(x => x.GetKeyAsync(cts.Token), Times.Once);
    }

    [TestMethod]
    public async Task ValidateTokenAsync_WithValidToken_ReturnsClaimsPrincipalAsync()
    {
        // Arrange
        IJwtToken generatedToken = await _jwtService.GenerateTokenAsync(_testUser, ["User"], TestContext.CancellationToken);
        string tokenString = await generatedToken.SerializeAsync(TestContext.CancellationToken);

        // Act
        ClaimsPrincipal principal = await _jwtService.ValidateTokenAsync(tokenString, TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(principal?.Identity);
        Assert.IsTrue(principal.Identity.IsAuthenticated);
        Assert.AreEqual("12345678-1234-1234-1234-123456789012", principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.AreEqual("testuser", principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.AreEqual("test@example.com", principal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.AreEqual("User", principal.FindFirst(ClaimTypes.Role)?.Value);
    }

    [TestMethod]
    public async Task ValidateTokenAsync_WithInvalidToken_ThrowsSecurityTokenExceptionAsync()
    {
        // Arrange
        string invalidToken = "invalid.token.string";

        // Act & Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(
            async () => await _jwtService.ValidateTokenAsync(invalidToken, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task ValidateTokenAsync_WithExpiredToken_ThrowsSecurityTokenValidationExceptionAsync()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["Auth:Jwt:TimeToLive"]).Returns("-00:01:00"); // Expired 1 minute ago
        _mockConfiguration.Setup(x => x["Auth:Jwt:ClockSkew"]).Returns("00:00:00");   // No clock skew for testing
        JwtService serviceWithExpiredTime = new(_mockConfiguration.Object, _mockSigningKeyProvider.Object, _mockAlgorithmProvider.Object);
        
        IJwtToken expiredToken = await serviceWithExpiredTime.GenerateTokenAsync(_testUser, ["User"], TestContext.CancellationToken);
        string tokenString = await expiredToken.SerializeAsync(TestContext.CancellationToken);

        // Reset configuration for validation
        _mockConfiguration.Setup(x => x["Auth:Jwt:TimeToLive"]).Returns("01:00:00");

        // Act & Assert
        await Assert.ThrowsExactlyAsync<SecurityTokenExpiredException>(
            async () => await _jwtService.ValidateTokenAsync(tokenString, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task ValidateTokenAsync_WithWrongIssuer_ThrowsSecurityTokenInvalidIssuerExceptionAsync()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["Auth:Jwt:Issuer"]).Returns("wrong-issuer");
        JwtService serviceWithWrongIssuer = new(_mockConfiguration.Object, _mockSigningKeyProvider.Object, _mockAlgorithmProvider.Object);
        
        IJwtToken token = await serviceWithWrongIssuer.GenerateTokenAsync(_testUser, ["User"], TestContext.CancellationToken);
        string tokenString = await token.SerializeAsync(TestContext.CancellationToken);

        // Reset issuer for validation
        _mockConfiguration.Setup(x => x["Auth:Jwt:Issuer"]).Returns("test-issuer");

        // Act & Assert
        await Assert.ThrowsExactlyAsync<SecurityTokenInvalidIssuerException>(
            async () => await _jwtService.ValidateTokenAsync(tokenString, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task ValidateTokenAsync_WithWrongAudience_ThrowsSecurityTokenInvalidAudienceExceptionAsync()
    {
        // Arrange
        _mockConfiguration.Setup(x => x["Auth:Jwt:Audience"]).Returns("wrong-audience");
        JwtService serviceWithWrongAudience = new(_mockConfiguration.Object, _mockSigningKeyProvider.Object, _mockAlgorithmProvider.Object);
        
        IJwtToken token = await serviceWithWrongAudience.GenerateTokenAsync(_testUser, ["User"], TestContext.CancellationToken);
        string tokenString = await token.SerializeAsync(TestContext.CancellationToken);

        // Reset audience for validation
        _mockConfiguration.Setup(x => x["Auth:Jwt:Audience"]).Returns("test-audience");

        // Act & Assert
        await Assert.ThrowsExactlyAsync<SecurityTokenInvalidAudienceException>(
            async () => await _jwtService.ValidateTokenAsync(tokenString, TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task ValidateTokenAsync_WithCancellationToken_PassesToSigningKeyProviderAsync()
    {
        // Arrange
        using CancellationTokenSource cts = new();
        IJwtToken token = await _jwtService.GenerateTokenAsync(_testUser, ["User"], TestContext.CancellationToken);
        string tokenString = await token.SerializeAsync(TestContext.CancellationToken);

        // Reset the mock to verify the call with the specific token
        _mockSigningKeyProvider.Reset();
        _mockSigningKeyProvider.Setup(x => x.GetKeyAsync(It.IsAny<CancellationToken>())).ReturnsAsync(_testKey);

        // Act
        ClaimsPrincipal principal = await _jwtService.ValidateTokenAsync(tokenString, cts.Token);

        // Assert
        _mockSigningKeyProvider.Verify(x => x.GetKeyAsync(cts.Token), Times.Once);
    }

    [TestMethod]
    public async Task JwtToken_SerializeAsync_ReturnsValidJwtStringAsync()
    {
        // Arrange
        IJwtToken token = await _jwtService.GenerateTokenAsync(_testUser, ["User"], TestContext.CancellationToken);

        // Act
        string serializedToken = await token.SerializeAsync(TestContext.CancellationToken);

        // Assert
        Assert.IsNotNull(serializedToken);
        Assert.Contains(".", serializedToken);
        
        // Verify it's a valid JWT format (3 parts separated by dots)
        string[] parts = serializedToken.Split('.');
        Assert.HasCount(3, parts);
    }

    private void SetupMockConfiguration()
    {
        _mockConfiguration.Setup(x => x["Auth:Jwt:Issuer"]).Returns("test-issuer");
        _mockConfiguration.Setup(x => x["Auth:Jwt:Audience"]).Returns("test-audience");
        _mockConfiguration.Setup(x => x["Auth:Jwt:TimeToLive"]).Returns("01:00:00");
        _mockConfiguration.Setup(x => x["Auth:Jwt:Key"]).Returns("test-key-that-is-long-enough-for-hmac-sha256");
        _mockConfiguration.Setup(x => x["Auth:Jwt:ClockSkew"]).Returns("00:05:00");
    }

    private void SetupMockDependencies()
    {
        _mockSigningKeyProvider.Setup(x => x.GetKeyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testKey);
        
        _mockAlgorithmProvider.Setup(x => x.GetAlgorithm())
            .Returns(SecurityAlgorithms.HmacSha256Signature);
    }
}