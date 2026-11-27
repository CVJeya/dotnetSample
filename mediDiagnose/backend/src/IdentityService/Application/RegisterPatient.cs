using IdentityService.Domain;
using IdentityService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application;

public sealed class RegisterPatient(IdentityDbContext db)
{
    public async Task<UserAccount> ExecuteAsync(
        string name,
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await db.UserAccounts.AnyAsync(account => account.Email == normalizedEmail, cancellationToken))
        {
            throw new IdentityConflictException("Email already registered.");
        }

        var account = new UserAccount
        {
            Name = name.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = AccountRole.Patient,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        db.UserAccounts.Add(account);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IdentityDatabaseErrors.IsUniqueViolation(exception))
        {
            throw new IdentityConflictException("Email already registered.", exception);
        }

        return account;
    }
}

public sealed class IdentityConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);
