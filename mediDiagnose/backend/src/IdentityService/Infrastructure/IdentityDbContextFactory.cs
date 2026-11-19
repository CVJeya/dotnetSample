using BuildingBlocks.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace IdentityService.Infrastructure;

public sealed class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        var connectionString = DatabaseConnectionSettings.RequireDbUrl(configuration);
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new IdentityDbContext(options);
    }
}
