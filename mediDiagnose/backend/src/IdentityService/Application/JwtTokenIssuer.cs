using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Domain;
using Microsoft.IdentityModel.Tokens;

namespace IdentityService.Application;

public sealed class JwtTokenIssuer(IConfiguration configuration)
{
    public string Issue(UserAccount account)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt__Key is not set.");
        if (key.Length < 32)
        {
            throw new InvalidOperationException("Jwt__Key must be at least 32 characters.");
        }

        var issuer = configuration["Jwt:Issuer"] ?? "MediDiagnose";
        var audience = configuration["Jwt:Audience"] ?? "MediDiagnoseClient";
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim("sub", account.Id.ToString()),
            new Claim("role", account.Role),
            new Claim("name", account.Name),
            new Claim(ClaimTypes.Email, account.Email)
        };
        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
