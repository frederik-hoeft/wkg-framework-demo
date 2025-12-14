using Microsoft.IdentityModel.Tokens;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class JwtHmacSha256AlgorithmProvider : IJwtAlgorithmProvider
{
    public string GetAlgorithm() => SecurityAlgorithms.HmacSha256;
}
