using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class JwtRsaPemFileSigningKeyImportService(IConfiguration configuration) : IJwtRsaSigningKeyImportService
{
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Ownership is transferred to caller.")]
    public async ValueTask<RSA> ImportKeyAsync(CancellationToken cancellationToken = default)
    {
        string? pemKeyFile = configuration["Auth:Jwt:RsaPrivateKeyFile"];
        if (string.IsNullOrWhiteSpace(pemKeyFile))
        {
            throw new InvalidOperationException("RSA private key file path is not configured.");
        }
        FileInfo finfo = new(pemKeyFile);
        if (!finfo.Exists)
        {
            throw new FileNotFoundException("RSA private key file not found.", pemKeyFile);
        }
        string pemContents = await File.ReadAllTextAsync(finfo.FullName, Encoding.UTF8, cancellationToken);
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(pemContents);
        return rsa;
    }
}
