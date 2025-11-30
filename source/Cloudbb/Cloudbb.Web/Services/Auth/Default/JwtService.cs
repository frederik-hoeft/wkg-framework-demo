using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cloudbb.Web.Services.Auth.Default;

internal sealed class JwtService(IConfiguration configuration) : IJwtService
{
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public string GenerateToken(IdentityUser user, IEnumerable<string> roles)
    {
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(configuration["Auth:Jwt:Key"]!));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
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
            expires: DateTime.UtcNow.AddMinutes(double.Parse(configuration["Auth:Jwt:ExpirationMinutes"]!)),
            signingCredentials: credentials);

        return _tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(configuration["Auth:Jwt:Key"]!));

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