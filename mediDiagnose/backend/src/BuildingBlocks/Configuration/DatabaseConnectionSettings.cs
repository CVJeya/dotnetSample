using System.Globalization;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BuildingBlocks.Configuration;

public static class DatabaseConnectionSettings
{
    public static string RequireDbUrl(IConfiguration configuration)
    {
        var dbUrl = configuration["DB_URL"];
        if (string.IsNullOrWhiteSpace(dbUrl))
        {
            throw new InvalidOperationException("DB_URL must be configured with a PostgreSQL connection string.");
        }

        if (!dbUrl.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !dbUrl.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            _ = new NpgsqlConnectionStringBuilder(dbUrl);
            return dbUrl;
        }

        var uri = new Uri(dbUrl, UriKind.Absolute);
        var credentials = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
            Username = Uri.UnescapeDataString(credentials[0])
        };

        if (credentials.Length == 2)
        {
            builder.Password = Uri.UnescapeDataString(credentials[1]);
        }

        foreach (var parameter in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = parameter.Split('=', 2);
            var key = Uri.UnescapeDataString(pair[0]);
            var value = pair.Length == 2 ? Uri.UnescapeDataString(pair[1]) : string.Empty;

            if (string.Equals(key, "sslmode", StringComparison.OrdinalIgnoreCase))
            {
                if (!Enum.TryParse<SslMode>(value, ignoreCase: true, out var sslMode))
                {
                    throw new InvalidOperationException("DB_URL contains an unsupported sslmode value.");
                }

                builder.SslMode = sslMode;
            }
            else if (string.Equals(key, "connect_timeout", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var timeout))
                {
                    throw new InvalidOperationException("DB_URL contains an invalid connect_timeout value.");
                }

                builder.Timeout = timeout;
            }
            else if (string.Equals(key, "application_name", StringComparison.OrdinalIgnoreCase))
            {
                builder.ApplicationName = value;
            }
            else
            {
                throw new InvalidOperationException($"DB_URL contains an unsupported PostgreSQL URI option: {key}.");
            }
        }

        return builder.ConnectionString;
    }
}
