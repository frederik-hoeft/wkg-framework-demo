using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class JwtECDsaPemFileSigningKeyImportService(IConfiguration configuration) : IJwtECDsaSigningKeyImportService
{
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownership is transferred to caller.")]
    public async ValueTask<ECDsa> ImportKeyAsync(CancellationToken cancellationToken = default)
    {
        string? pemKeyFile = configuration["Auth:Jwt:ECDsaPrivateKeyFile"];
        if (string.IsNullOrWhiteSpace(pemKeyFile))
        {
            throw new InvalidOperationException("ECDSA private key file path is not configured.");
        }
        FileInfo finfo = new(pemKeyFile);
        if (!finfo.Exists)
        {
            throw new FileNotFoundException("ECDSA private key file not found.", pemKeyFile);
        }
        string pemContents = await File.ReadAllTextAsync(finfo.FullName, Encoding.UTF8, cancellationToken);
        ECDsa ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(pemContents);
        return ecdsa;
    }
}
