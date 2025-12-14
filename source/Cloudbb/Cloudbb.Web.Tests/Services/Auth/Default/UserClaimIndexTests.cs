using Cloudbb.Web.Services.Auth.Default;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace Cloudbb.Web.Tests.Services.Auth.Default;

[TestClass]
public sealed class UserClaimIndexTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
    private readonly Mock<HttpContext> _mockHttpContext = new();
    private readonly UserClaimIndex _userClaimIndex;

    public UserClaimIndexTests()
    {
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);
        _userClaimIndex = new UserClaimIndex(_mockHttpContextAccessor.Object);
    }

    [TestMethod]
    public void IsAuthenticated_WithAuthenticatedUser_ReturnsTrue()
    {
        // Arrange
        ClaimsIdentity identity = new("test", ClaimTypes.Name, ClaimTypes.Role);
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool isAuthenticated = _userClaimIndex.IsAuthenticated;

        // Assert
        Assert.IsTrue(isAuthenticated);
    }

    [TestMethod]
    public void IsAuthenticated_WithUnauthenticatedUser_ReturnsFalse()
    {
        // Arrange
        ClaimsIdentity identity = new(); // No authentication type means not authenticated
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool isAuthenticated = _userClaimIndex.IsAuthenticated;

        // Assert
        Assert.IsFalse(isAuthenticated);
    }

    [TestMethod]
    public void IsAuthenticated_WithNullHttpContext_ReturnsFalse()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        bool isAuthenticated = _userClaimIndex.IsAuthenticated;

        // Assert
        Assert.IsFalse(isAuthenticated);
    }

    [TestMethod]
    public void IsAuthenticated_WithNullIdentity_ReturnsFalse()
    {
        // Arrange
        ClaimsPrincipal principal = new();
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool isAuthenticated = _userClaimIndex.IsAuthenticated;

        // Assert
        Assert.IsFalse(isAuthenticated);
    }

    [TestMethod]
    public void TryGetUserId_WithValidUserIdClaim_ReturnsTrueAndCorrectId()
    {
        // Arrange
        Guid expectedUserId = Guid.NewGuid();
        Claim[] claims = [new(ClaimTypes.NameIdentifier, expectedUserId.ToString())];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool result = _userClaimIndex.TryGetUserId(out Guid actualUserId);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(expectedUserId, actualUserId);
    }

    [TestMethod]
    public void TryGetUserId_WithInvalidUserIdClaim_ReturnsFalseAndEmptyGuid()
    {
        // Arrange
        Claim[] claims = [new(ClaimTypes.NameIdentifier, "not-a-guid")];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool result = _userClaimIndex.TryGetUserId(out Guid actualUserId);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(Guid.Empty, actualUserId);
    }

    [TestMethod]
    public void TryGetUserId_WithNoUserIdClaim_ReturnsFalseAndEmptyGuid()
    {
        // Arrange
        Claim[] claims = [new(ClaimTypes.Name, "testuser")];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool result = _userClaimIndex.TryGetUserId(out Guid actualUserId);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(Guid.Empty, actualUserId);
    }

    [TestMethod]
    public void TryGetUserId_WithUnauthenticatedUser_ReturnsFalseAndEmptyGuid()
    {
        // Arrange
        ClaimsIdentity identity = new(); // Unauthenticated
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool result = _userClaimIndex.TryGetUserId(out Guid actualUserId);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(Guid.Empty, actualUserId);
    }

    [TestMethod]
    public void GetUserId_WithValidUserIdClaim_ReturnsCorrectId()
    {
        // Arrange
        Guid expectedUserId = Guid.NewGuid();
        Claim[] claims = [new(ClaimTypes.NameIdentifier, expectedUserId.ToString())];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        Guid actualUserId = _userClaimIndex.GetUserId();

        // Assert
        Assert.AreEqual(expectedUserId, actualUserId);
    }

    [TestMethod]
    public void GetUserId_WithInvalidUserIdClaim_ThrowsInvalidOperationException()
    {
        // Arrange
        Claim[] claims = [new(ClaimTypes.NameIdentifier, "not-a-guid")];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => _userClaimIndex.GetUserId());
    }

    [TestMethod]
    public void GetUserId_WithNoUserIdClaim_ThrowsInvalidOperationException()
    {
        // Arrange
        Claim[] claims = [new(ClaimTypes.Name, "testuser")];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => _userClaimIndex.GetUserId());
    }

    [TestMethod]
    public void TryGetUsername_WithValidUsernameClaim_ReturnsTrueAndCorrectUsername()
    {
        // Arrange
        string expectedUsername = "testuser";
        Claim[] claims = [new(ClaimTypes.Name, expectedUsername)];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool result = _userClaimIndex.TryGetUsername(out string? actualUsername);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(expectedUsername, actualUsername);
    }

    [TestMethod]
    public void TryGetUsername_WithNoUsernameClaim_ReturnsFalseAndNull()
    {
        // Arrange
        Claim[] claims = [new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool result = _userClaimIndex.TryGetUsername(out string? actualUsername);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(actualUsername);
    }

    [TestMethod]
    public void TryGetUsername_WithUnauthenticatedUser_ReturnsFalseAndNull()
    {
        // Arrange
        ClaimsIdentity identity = new(); // Unauthenticated
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool result = _userClaimIndex.TryGetUsername(out string? actualUsername);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(actualUsername);
    }

    [TestMethod]
    public void GetUsername_WithValidUsernameClaim_ReturnsCorrectUsername()
    {
        // Arrange
        string expectedUsername = "testuser";
        Claim[] claims = [new(ClaimTypes.Name, expectedUsername)];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        string actualUsername = _userClaimIndex.GetUsername();

        // Assert
        Assert.AreEqual(expectedUsername, actualUsername);
    }

    [TestMethod]
    public void GetUsername_WithNoUsernameClaim_ThrowsInvalidOperationException()
    {
        // Arrange
        Claim[] claims = [new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(_userClaimIndex.GetUsername);
    }

    [TestMethod]
    public void GetUsername_WithEmptyUsernameClaim_ThrowsInvalidOperationException()
    {
        // Arrange
        Claim[] claims = [new(ClaimTypes.Name, "")];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(_userClaimIndex.GetUsername);
    }

    [TestMethod]
    public void TryGetClaim_WithExistingClaim_ReturnsTrueAndCorrectValue()
    {
        // Arrange
        string claimType = "custom:claim";
        string expectedValue = "custom-value";
        Claim[] claims = [new(claimType, expectedValue)];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool result = _userClaimIndex.TryGetClaim(claimType, out string? actualValue);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(expectedValue, actualValue);
    }

    [TestMethod]
    public void TryGetClaim_WithNonExistingClaim_ReturnsFalseAndNull()
    {
        // Arrange
        Claim[] claims = [new(ClaimTypes.Name, "testuser")];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool result = _userClaimIndex.TryGetClaim("non:existing", out string? actualValue);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(actualValue);
    }

    [TestMethod]
    public void TryGetClaim_CachesClaimsOnFirstAccess()
    {
        // Arrange
        Claim[] claims = 
        [
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "testuser"),
            new("custom:claim", "custom-value")
        ];
        ClaimsIdentity identity = new(claims, "test");
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act - First access should build cache
        bool result1 = _userClaimIndex.TryGetClaim(ClaimTypes.Name, out string? value1);
        
        // Modify the mock to return different claims
        Claim[] differentClaims = [new(ClaimTypes.Name, "different-user")];
        ClaimsIdentity differentIdentity = new(differentClaims, "test");
        ClaimsPrincipal differentPrincipal = new(differentIdentity);
        _mockHttpContext.Setup(x => x.User).Returns(differentPrincipal);
        
        // Second access should use cached values
        bool result2 = _userClaimIndex.TryGetClaim(ClaimTypes.Name, out string? value2);
        bool result3 = _userClaimIndex.TryGetClaim("custom:claim", out string? value3);

        // Assert
        Assert.IsTrue(result1);
        Assert.IsTrue(result2);
        Assert.IsTrue(result3);
        Assert.AreEqual("testuser", value1);
        Assert.AreEqual("testuser", value2); // Should be cached value, not "different-user"
        Assert.AreEqual("custom-value", value3);
    }

    [TestMethod]
    public void TryGetClaim_WithUnauthenticatedUser_ReturnsFalseAndNull()
    {
        // Arrange
        ClaimsIdentity identity = new(); // Unauthenticated
        ClaimsPrincipal principal = new(identity);
        _mockHttpContext.Setup(x => x.User).Returns(principal);

        // Act
        bool result = _userClaimIndex.TryGetClaim(ClaimTypes.Name, out string? actualValue);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(actualValue);
    }

    [TestMethod]
    public void TryGetClaim_WithNullHttpContext_ReturnsFalseAndNull()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        bool result = _userClaimIndex.TryGetClaim(ClaimTypes.Name, out string? actualValue);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(actualValue);
    }

    [TestMethod]
    public void TryGetClaim_WithNullClaims_ReturnsFalseAndNull()
    {
        // Arrange
        Mock<ClaimsPrincipal> mockPrincipal = new();
        Mock<ClaimsIdentity> mockIdentity = new();
        
        mockIdentity.Setup(x => x.IsAuthenticated).Returns(true);
        mockIdentity.Setup(x => x.Claims).Returns([]);
        mockPrincipal.Setup(x => x.Identity).Returns(mockIdentity.Object);
        mockPrincipal.Setup(x => x.Claims).Returns([]);
        
        _mockHttpContext.Setup(x => x.User).Returns(mockPrincipal.Object);

        // Act
        bool result = _userClaimIndex.TryGetClaim(ClaimTypes.Name, out string? actualValue);

        // Assert
        Assert.IsFalse(result);
        Assert.IsNull(actualValue);
    }
}