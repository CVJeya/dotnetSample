using BuildingBlocks.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace AppointmentService.Infrastructure;

public sealed class AppointmentsDbContextFactory : IDesignTimeDbContextFactory<AppointmentsDbContext>
{
    public AppointmentsDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        var connectionString = DatabaseConnectionSettings.RequireDbUrl(configuration);
        var options = new DbContextOptionsBuilder<AppointmentsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppointmentsDbContext(options);
    }
}
