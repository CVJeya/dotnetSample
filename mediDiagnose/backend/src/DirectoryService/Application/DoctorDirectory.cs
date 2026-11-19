using DirectoryService.Domain;
using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application;

public sealed class DoctorDirectory(DirectoryDbContext db)
{
    public Task<List<DoctorProfile>> GetPublicDoctorsAsync(CancellationToken cancellationToken = default) =>
        db.Doctors.AsNoTracking()
            .Where(doctor => doctor.IsActive)
            .OrderBy(doctor => doctor.Name)
            .ToListAsync(cancellationToken);

    public Task<DoctorProfile?> GetPublicDoctorAsync(Guid doctorId, CancellationToken cancellationToken = default) =>
        db.Doctors.AsNoTracking()
            .FirstOrDefaultAsync(doctor => doctor.Id == doctorId && doctor.IsActive, cancellationToken);

    public Task<DoctorProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Doctors.FirstOrDefaultAsync(doctor => doctor.UserId == userId && doctor.IsActive, cancellationToken);

    public async Task<DoctorProfile> CreateAsync(
        Guid userId,
        string name,
        string specialty,
        string? biography,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty || await db.Doctors.AnyAsync(doctor => doctor.UserId == userId, cancellationToken))
        {
            throw new DirectoryConflictException("A doctor profile already exists for this account.");
        }

        var doctor = new DoctorProfile
        {
            UserId = userId,
            Name = name.Trim(),
            Specialty = specialty.Trim(),
            Biography = biography?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        db.Doctors.Add(doctor);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (DirectoryDatabaseErrors.IsUniqueViolation(exception))
        {
            throw new DirectoryConflictException("A doctor profile already exists for this account.", exception);
        }

        return doctor;
    }

    public async Task<DoctorProfile?> UpdateAsync(
        Guid doctorId,
        string name,
        string specialty,
        string? biography,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var doctor = await db.Doctors.FirstOrDefaultAsync(profile => profile.Id == doctorId, cancellationToken);
        if (doctor is null)
        {
            return null;
        }

        doctor.Name = name.Trim();
        doctor.Specialty = specialty.Trim();
        doctor.Biography = biography?.Trim() ?? string.Empty;
        doctor.IsActive = isActive;
        await db.SaveChangesAsync(cancellationToken);
        return doctor;
    }

    public async Task<bool> DeactivateAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        var doctor = await db.Doctors.FirstOrDefaultAsync(profile => profile.Id == doctorId, cancellationToken);
        if (doctor is null)
        {
            return false;
        }

        doctor.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed class DirectoryConflictException(string message, Exception? innerException = null)
    : Exception(message, innerException);

internal static class DirectoryDatabaseErrors
{
    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is Npgsql.PostgresException { SqlState: Npgsql.PostgresErrorCodes.UniqueViolation };
}
