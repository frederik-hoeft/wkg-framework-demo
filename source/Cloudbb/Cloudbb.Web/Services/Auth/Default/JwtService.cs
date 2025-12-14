using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class JwtService(IConfiguration configuration, IJwtSigningKeyProvider credentialsFactory, IJwtAlgorithmProvider jwtAlgorithmProvider) : IJwtService
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public async ValueTask<string> GenerateTokenAsync(IdentityUser user, IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        SecurityKey key = await credentialsFactory.GetKeyAsync(cancellationToken);
        SigningCredentials credentials = new(key, jwtAlgorithmProvider.GetAlgorithm());

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            .. roles.Select(role => new Claim(ClaimTypes.Role, role)),
        ];

        JwtSecurityToken token = new(
            issuer: configuration["Auth:Jwt:Issuer"],
            audience: configuration["Auth:Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(TimeSpan.Parse(configuration["Auth:Jwt:TimeToLive"]!)),
            signingCredentials: credentials);

        return _tokenHandler.WriteToken(token);
    }

    public async ValueTask<ClaimsPrincipal> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        SecurityKey key = await credentialsFactory.GetKeyAsync(cancellationToken);
        TokenValidationParameters validationParameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = configuration["Auth:Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["Auth:Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        ClaimsPrincipal principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
        return principal;
    }
}