using Microsoft.IdentityModel.Tokens;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class RsaSha256AlgorithmProvider : IJwtAlgorithmProvider
{
    public string GetAlgorithm() => SecurityAlgorithms.RsaSha256Signature;
}