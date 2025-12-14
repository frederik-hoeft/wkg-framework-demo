using Cloudbb.Web.Services.Auth.Default;
using Microsoft.IdentityModel.Tokens;

namespace Cloudbb.Web.Tests.Services.Auth.Default;

[TestClass]
public sealed class JwtEcdsaSha256AlgorithmProviderTests
{
    private readonly JwtEcdsaSha256AlgorithmProvider _provider = new();

    [TestMethod]
    public void GetAlgorithm_ReturnsEcdsaSha256Signature()
    {
        // Act
        string algorithm = _provider.GetAlgorithm();

        // Assert
        Assert.AreEqual(SecurityAlgorithms.EcdsaSha256Signature, algorithm);
    }

    [TestMethod]
    public void GetAlgorithm_MultipleCalls_ReturnsSameValue()
    {
        // Act
        string algorithm1 = _provider.GetAlgorithm();
        string algorithm2 = _provider.GetAlgorithm();

        // Assert
        Assert.AreEqual(algorithm1, algorithm2);
        Assert.AreEqual(SecurityAlgorithms.EcdsaSha256Signature, algorithm1);
        Assert.AreEqual(SecurityAlgorithms.EcdsaSha256Signature, algorithm2);
    }

    [TestMethod]
    public void GetAlgorithm_ReturnsConstantValue()
    {
        // Arrange
        JwtEcdsaSha256AlgorithmProvider provider1 = new();
        JwtEcdsaSha256AlgorithmProvider provider2 = new();

        // Act
        string algorithm1 = provider1.GetAlgorithm();
        string algorithm2 = provider2.GetAlgorithm();

        // Assert
        Assert.AreEqual(algorithm1, algorithm2);
        Assert.AreEqual(SecurityAlgorithms.EcdsaSha256Signature, algorithm1);
        Assert.AreEqual(SecurityAlgorithms.EcdsaSha256Signature, algorithm2);
    }
}