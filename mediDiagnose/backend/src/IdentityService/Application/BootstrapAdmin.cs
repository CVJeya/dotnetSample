using System.ComponentModel.DataAnnotations;
using IdentityService.Domain;
using IdentityService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application;

public sealed class BootstrapAdmin(IdentityDbContext db)
{
    public async Task<UserAccount> RunAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email.Trim()))
        {
            throw new ArgumentException("A valid initial admin email is required.", nameof(email));
        }

        if (password.Length is < 8 or > 100)
        {
            throw new ArgumentException("The initial admin password must be between 8 and 100 characters.", nameof(password));
        }

        if (await db.UserAccounts.AnyAsync(account => account.Role == AccountRole.Admin, cancellationToken))
        {
            throw new InvalidOperationException("An Admin account already exists.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await db.UserAccounts.AnyAsync(account => account.Email == normalizedEmail, cancellationToken))
        {
            throw new IdentityConflictException("Email already registered.");
        }

        var account = new UserAccount
        {
            Name = "Hospital Administrator",
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = AccountRole.Admin,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        db.UserAccounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);
        return account;
    }
}
