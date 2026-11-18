using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Models;

namespace TaskManagement.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.ToTable("Tasks");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.TaskTitle)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.TaskDescription)
                .HasMaxLength(2000);

            entity.Property(e => e.TaskStatus)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue(TaskStatusConstants.Pending);

            entity.Property(e => e.TaskRemarks)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("System");

            entity.Property(e => e.LastUpdatedBy)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue("System");

            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.LastUpdatedOn)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.TaskTitle).HasDatabaseName("IX_Tasks_TaskTitle");
            entity.HasIndex(e => e.TaskStatus).HasDatabaseName("IX_Tasks_TaskStatus");
        });
    }
}
