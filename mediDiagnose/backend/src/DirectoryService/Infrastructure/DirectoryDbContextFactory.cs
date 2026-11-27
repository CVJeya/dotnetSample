using BuildingBlocks.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DirectoryService.Infrastructure;

public sealed class DirectoryDbContextFactory : IDesignTimeDbContextFactory<DirectoryDbContext>
{
    public DirectoryDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        var connectionString = DatabaseConnectionSettings.RequireDbUrl(configuration);
        var options = new DbContextOptionsBuilder<DirectoryDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new DirectoryDbContext(options);
    }
}
