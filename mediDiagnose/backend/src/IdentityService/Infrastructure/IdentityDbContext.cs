using IdentityService.Domain;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(account => account.Id);
            entity.HasIndex(account => account.Email).IsUnique();
            entity.HasIndex(account => account.Role)
                .IsUnique()
                .HasFilter("\"Role\" = 'Admin'")
                .HasDatabaseName("UX_UserAccounts_OneAdmin");
            entity.Property(account => account.Name).HasMaxLength(100).IsRequired();
            entity.Property(account => account.Email).HasMaxLength(200).IsRequired();
            entity.Property(account => account.PasswordHash).IsRequired();
            entity.Property(account => account.Role).HasMaxLength(20).IsRequired();
        });
    }
}
