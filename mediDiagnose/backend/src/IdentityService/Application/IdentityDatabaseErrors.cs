using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IdentityService.Application;

internal static class IdentityDatabaseErrors
{
    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
