using DirectoryService.Domain;
using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application;

public sealed class PatientDirectory(DirectoryDbContext db)
{
    public Task<List<PatientProfile>> GetActivePatientsAsync(CancellationToken cancellationToken = default) =>
        db.Patients.AsNoTracking()
            .Where(patient => patient.IsActive)
            .OrderBy(patient => patient.Name)
            .ToListAsync(cancellationToken);

    public Task<PatientProfile?> GetAsync(Guid userId, CancellationToken cancellationToken = default) =>
        db.Patients.AsNoTracking()
            .FirstOrDefaultAsync(patient => patient.UserId == userId, cancellationToken);

    public async Task<PatientProfile?> SaveOwnProfileAsync(
        Guid userId,
        string name,
        string email,
        string? phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var patient = await db.Patients.FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
        if (patient is { IsActive: false })
        {
            return null;
        }

        if (patient is null)
        {
            patient = new PatientProfile { UserId = userId, CreatedAt = DateTime.UtcNow };
            db.Patients.Add(patient);
        }

        patient.Name = name.Trim();
        patient.Email = email.Trim().ToLowerInvariant();
        patient.PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        patient.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return patient;
    }

    public async Task<PatientProfile?> UpdateAsync(
        Guid userId,
        string name,
        string email,
        string? phoneNumber,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var patient = await db.Patients.FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
        if (patient is null)
        {
            return null;
        }

        patient.Name = name.Trim();
        patient.Email = email.Trim().ToLowerInvariant();
        patient.PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        patient.IsActive = isActive;
        patient.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return patient;
    }

    public async Task<bool> DeactivateAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var patient = await db.Patients.FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
        if (patient is null)
        {
            return false;
        }

        patient.IsActive = false;
        patient.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
