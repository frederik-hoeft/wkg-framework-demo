using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class JwtSymmetricSigningKeyProvider(IConfiguration configuration) : IJwtSigningKeyProvider
{
    private bool _disposed;

    public ValueTask<SecurityKey> GetKeyAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        SecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Auth:Jwt:Key"]!));
        return ValueTask.FromResult(key);
    }

    // nothing to dispose, still mark as disposed for correctness and to detect use-after-dispose
    public void Dispose() => _disposed = true;
}
