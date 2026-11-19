using IdentityService.Domain;
using IdentityService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application;

public sealed class AuthenticateUser(IdentityDbContext db)
{
    public async Task<UserAccount?> ExecuteAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var account = await db.UserAccounts.FirstOrDefaultAsync(
            candidate => candidate.Email == normalizedEmail && candidate.IsActive,
            cancellationToken);

        if (account is null || !BCrypt.Net.BCrypt.Verify(password, account.PasswordHash))
        {
            return null;
        }

        return account;
    }
}
