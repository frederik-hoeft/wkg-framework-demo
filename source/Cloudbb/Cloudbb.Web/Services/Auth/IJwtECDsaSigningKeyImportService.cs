using System.Security.Cryptography;

namespace Cloudbb.Web.Services.Auth;

public interface IJwtECDsaSigningKeyImportService
{
    ValueTask<ECDsa> ImportKeyAsync(CancellationToken cancellationToken = default);
}
