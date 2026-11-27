using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DirectoryService.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace DirectoryService.Tests;

public sealed class DirectoryApiFactory : WebApplicationFactory<Program>
{
    public const string JwtKey = "directory-test-secret-key-that-is-at-least-32-characters";
    public const string InternalApiKey = "directory-test-internal-api-key-32chars";
    private readonly string _databaseName = $"directory-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("DB_URL", "Host=localhost;Database=directory-tests;Username=test;Password=test");
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:Issuer", "MediDiagnose");
        builder.UseSetting("Jwt:Audience", "MediDiagnoseClient");
        builder.UseSetting("Hospital:TimeZone", "Asia/Kolkata");
        builder.UseSetting("InternalServices:ApiKey", InternalApiKey);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<DirectoryDbContext>>();
            services.RemoveAll<DbContextOptions<DirectoryDbContext>>();
            services.AddDbContext<DirectoryDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }

    public static string CreateToken(Guid userId, string role)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            "MediDiagnose",
            "MediDiagnoseClient",
            [
                new Claim("sub", userId.ToString()),
                new Claim("role", role),
                new Claim("name", "Test User")
            ],
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
