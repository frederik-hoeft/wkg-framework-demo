using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Security.Cryptography;
using Wkg.Threading;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class JwtECDsaSigningKeyProvider(IJwtECDsaSigningKeyImportService signingKeyImportService) : IJwtSigningKeyProvider
{
    private readonly AsyncLock _asyncLock = new();
    // volatile to hint to the compiler that these fields may be accessed from multiple threads
    // avoids certain caching optimizations that could lead to stale reads
    // ECDsa instance implements IDisposable, so we need to keep a reference to it to dispose of it later
    private volatile ECDsa? _ecdsa;
    private volatile ECDsaSecurityKey? _key;

    public async ValueTask<SecurityKey> GetKeyAsync(CancellationToken cancellationToken = default)
    {
        // lazy init with double-check locking
        ECDsaSecurityKey? key = _key;
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
            Debug.Assert(_ecdsa is null, "If _key is null, _ecdsa must also be null.");
            ECDsa ecdsa = await signingKeyImportService.ImportKeyAsync(ct);
            key = new ECDsaSecurityKey(ecdsa);

            _ecdsa = ecdsa;
            _key = key;
            return key;
        }, cancellationToken);
    }

    public void Dispose()
    {
        // dispose the lock, any waiters will be cancelled, and anything touching the lock after dispose will get ObjectDisposedException
        _asyncLock.Dispose();
        _ecdsa?.Dispose();
    }
}
