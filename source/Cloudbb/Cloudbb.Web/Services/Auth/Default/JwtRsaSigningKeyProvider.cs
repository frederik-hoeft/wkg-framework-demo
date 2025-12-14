using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Cryptography;
using Wkg.Threading;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class JwtRsaSigningKeyProvider(IJwtRsaSigningKeyImportService signingKeyImportService) : IJwtSigningKeyProvider
{
    private readonly AsyncLock _asyncLock = new();
    // volatile to hint to the compiler that these fields may be accessed from multiple threads
    // avoids certain caching optimizations that could lead to stale reads
    // RSA instance implements IDisposable, so we need to keep a reference to it to dispose of it later
    private volatile RSA? _rsa;
    private volatile RsaSecurityKey? _key;
    private bool _disposed;

    public async ValueTask<SecurityKey> GetKeyAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        // lazy init with double-check locking
        RsaSecurityKey? key = _key;
        if (key is not null)
        {
            return key;
        }
        return await _asyncLock.RunTaskAsync(async ct =>
        {
            // re-sample in case re lost a race against another initialization
            key = _key;
            if (key is not null)
            {
                return key;
            }
            Debug.Assert(_rsa is null, "If _key is null, _ecdsa must also be null.");
            RSA rsa = await signingKeyImportService.ImportKeyAsync(ct);
            key = new RsaSecurityKey(rsa);

            _rsa = rsa;
            _key = key;
            return key;
        }, cancellationToken);
    }

    public void Dispose()
    {
        _disposed = true;
        // dispose the lock, any waiters will be cancelled, and anything touching the lock after dispose will get ObjectDisposedException
        _asyncLock.Dispose();
        _rsa?.Dispose();
    }
}
