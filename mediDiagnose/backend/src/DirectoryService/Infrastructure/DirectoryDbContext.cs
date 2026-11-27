using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure;

public sealed class DirectoryDbContext(DbContextOptions<DirectoryDbContext> options) : DbContext(options)
{
    public DbSet<DoctorProfile> Doctors => Set<DoctorProfile>();
    public DbSet<PatientProfile> Patients => Set<PatientProfile>();
    public DbSet<DoctorShift> DoctorShifts => Set<DoctorShift>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DoctorProfile>(entity =>
        {
            entity.HasKey(doctor => doctor.Id);
            entity.HasIndex(doctor => doctor.UserId).IsUnique();
            entity.Property(doctor => doctor.Name).HasMaxLength(100).IsRequired();
            entity.Property(doctor => doctor.Specialty).HasMaxLength(120).IsRequired();
            entity.Property(doctor => doctor.Biography).HasMaxLength(1000).IsRequired();
        });

        modelBuilder.Entity<PatientProfile>(entity =>
        {
            entity.HasKey(patient => patient.UserId);
            entity.Property(patient => patient.Name).HasMaxLength(100).IsRequired();
            entity.Property(patient => patient.Email).HasMaxLength(200).IsRequired();
            entity.Property(patient => patient.PhoneNumber).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<DoctorShift>(entity =>
        {
            entity.HasKey(shift => shift.Id);
            entity.HasIndex(shift => new { shift.DoctorId, shift.DayOfWeek, shift.Shift }).IsUnique();
            entity.Property(shift => shift.Shift).HasMaxLength(10).IsRequired();
            entity.Property(shift => shift.StartTime).HasColumnType("time without time zone");
            entity.Property(shift => shift.EndTime).HasColumnType("time without time zone");
            entity.HasOne<DoctorProfile>()
                .WithMany()
                .HasForeignKey(shift => shift.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
