using Microsoft.IdentityModel.Tokens;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class JwtEcdsaSha256AlgorithmProvider : IJwtAlgorithmProvider
{
    public string GetAlgorithm() => SecurityAlgorithms.EcdsaSha256Signature;
}