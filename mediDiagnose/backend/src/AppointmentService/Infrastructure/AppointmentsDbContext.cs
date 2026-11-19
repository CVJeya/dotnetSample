using AppointmentService.Domain;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Infrastructure;

public sealed class AppointmentsDbContext(DbContextOptions<AppointmentsDbContext> options) : DbContext(options)
{
    public DbSet<AppointmentRequest> AppointmentRequests => Set<AppointmentRequest>();
    public DbSet<ShiftCapacity> ShiftCapacities => Set<ShiftCapacity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppointmentRequest>(entity =>
        {
            entity.ToTable("AppointmentRequests");
            entity.HasKey(request => request.Id);
            entity.HasIndex(request => new { request.DoctorId, request.StartsAtUtc })
                .IsUnique()
                .HasFilter("\"Status\" IN ('Pending', 'Approved')")
                .HasDatabaseName("UX_AppointmentRequests_ActiveSlot");
            entity.HasIndex(request => new { request.PatientId, request.StartsAtUtc });
            entity.HasIndex(request => new { request.DoctorId, request.LocalDate, request.Shift, request.Status });
            entity.Property(request => request.Shift).HasMaxLength(10).IsRequired();
            entity.Property(request => request.Status).HasMaxLength(20).IsRequired();
            entity.Property(request => request.StartsAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(request => request.EndsAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(request => request.CreatedAtUtc).HasColumnType("timestamp with time zone");
            entity.Property(request => request.DecidedAtUtc).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<ShiftCapacity>(entity =>
        {
            entity.ToTable("ShiftCapacities", table =>
                table.HasCheckConstraint(
                    "CK_ShiftCapacities_ReservedCount",
                    $"\"ReservedCount\" >= 0 AND \"ReservedCount\" <= {ShiftCapacity.MaximumReservations}"));
            entity.HasKey(capacity => new { capacity.DoctorId, capacity.LocalDate, capacity.Shift });
            entity.Property(capacity => capacity.Shift).HasMaxLength(10).IsRequired();
        });
    }
}
