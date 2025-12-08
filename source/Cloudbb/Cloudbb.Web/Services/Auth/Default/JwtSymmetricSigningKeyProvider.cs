using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class JwtSymmetricSigningKeyProvider(IConfiguration configuration) : IJwtSigningKeyProvider
{
    public ValueTask<SecurityKey> GetKeyAsync(CancellationToken cancellationToken = default)
    {
        SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Auth:Jwt:Key"]!));
        return ValueTask.FromResult(key);
    }

    // nothing to dispose
    public void Dispose() { }
}
