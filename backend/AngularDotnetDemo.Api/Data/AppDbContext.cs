using AngularDotnetDemo.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AngularDotnetDemo.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Name).HasMaxLength(100);
            e.Property(u => u.Email).HasMaxLength(200);
        });
        b.Entity<Product>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(100);
            e.Property(p => p.Description).HasMaxLength(1000);
            e.Property(p => p.Price).HasPrecision(18, 2);
        });
    }
}
