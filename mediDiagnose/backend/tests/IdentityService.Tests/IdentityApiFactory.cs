using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IdentityService.Tests;

public sealed class IdentityApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"identity-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("DB_URL", "Host=localhost;Database=identity-tests;Username=test;Password=test");
        builder.UseSetting("Jwt:Key", new string('k', 48));
        builder.UseSetting("Jwt:Issuer", "IdentityTests");
        builder.UseSetting("Jwt:Audience", "IdentityTests");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<IdentityDbContext>>();
            services.RemoveAll<DbContextOptions<IdentityDbContext>>();
            services.AddDbContext<IdentityDbContext>(options => options.UseInMemoryDatabase(_databaseName));
        });
    }
}
