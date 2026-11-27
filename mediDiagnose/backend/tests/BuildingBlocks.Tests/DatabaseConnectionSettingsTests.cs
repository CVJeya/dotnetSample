using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;
using BuildingBlocks.Configuration;

namespace BuildingBlocks.Tests;

public class DatabaseConnectionSettingsTests
{
    [Fact]
    public void RequireDbUrl_WhenMissing_ThrowsAndNamesVariable()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () => DatabaseConnectionSettings.RequireDbUrl(configuration));

        Assert.Contains("DB_URL", exception.Message);
    }

    [Fact]
    public void RequireDbUrl_WhenBlank_ThrowsAndNamesVariable()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["DB_URL"] = "  " })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(
            () => DatabaseConnectionSettings.RequireDbUrl(configuration));

        Assert.Contains("DB_URL", exception.Message);
    }

    [Fact]
    public void RequireDbUrl_WhenNpgsqlConnectionString_ReturnsItUnchanged()
    {
        const string connectionString = "Host=db.example;Database=hospital;Username=app;Password=secret";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["DB_URL"] = connectionString })
            .Build();

        var result = DatabaseConnectionSettings.RequireDbUrl(configuration);

        Assert.Equal(connectionString, result);
    }

    [Fact]
    public void RequireDbUrl_WhenPostgresUri_DecodesCredentialsAndRetainsSslMode()
    {
        const string connectionUri = "postgresql://app-user:p%40ss%3Aword@db.example:5433/hospital?sslmode=require";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["DB_URL"] = connectionUri })
            .Build();

        var result = new NpgsqlConnectionStringBuilder(DatabaseConnectionSettings.RequireDbUrl(configuration));

        Assert.Equal("db.example", result.Host);
        Assert.Equal(5433, result.Port);
        Assert.Equal("hospital", result.Database);
        Assert.Equal("app-user", result.Username);
        Assert.Equal("p@ss:word", result.Password);
        Assert.Equal(SslMode.Require, result.SslMode);
    }
}
