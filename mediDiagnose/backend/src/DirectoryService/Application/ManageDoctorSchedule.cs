using DirectoryService.Domain;
using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Application;

public sealed class ManageDoctorSchedule(DirectoryDbContext db)
{
    public async Task<IReadOnlyList<DoctorShift>?> ReplaceAsync(
        Guid doctorUserId,
        IReadOnlyCollection<DoctorShiftInput> shifts,
        CancellationToken cancellationToken = default)
    {
        var doctor = await db.Doctors.FirstOrDefaultAsync(
            profile => profile.UserId == doctorUserId && profile.IsActive,
            cancellationToken);
        if (doctor is null)
        {
            return null;
        }

        Validate(shifts);
        var existing = await db.DoctorShifts
            .Where(shift => shift.DoctorId == doctor.Id)
            .ToListAsync(cancellationToken);
        db.DoctorShifts.RemoveRange(existing);

        var replacement = shifts.Select(shift => new DoctorShift
        {
            DoctorId = doctor.Id,
            DayOfWeek = shift.DayOfWeek,
            Shift = shift.Shift,
            StartTime = shift.StartTime,
            EndTime = shift.EndTime,
            AppointmentDurationMinutes = shift.AppointmentDurationMinutes
        }).ToList();
        db.DoctorShifts.AddRange(replacement);
        await db.SaveChangesAsync(cancellationToken);
        return replacement;
    }

    public Task<List<DoctorShift>> GetForDateAsync(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        CancellationToken cancellationToken = default) =>
        db.DoctorShifts.AsNoTracking()
            .Where(shift => shift.DoctorId == doctorId && shift.DayOfWeek == (int)dayOfWeek)
            .OrderBy(shift => shift.StartTime)
            .ToListAsync(cancellationToken);

    private static void Validate(IReadOnlyCollection<DoctorShiftInput> shifts)
    {
        if (shifts.Count > 14)
        {
            throw new ScheduleValidationException("A doctor can define at most two shifts per weekday.");
        }

        var seen = new HashSet<(int DayOfWeek, string Shift)>();
        foreach (var shift in shifts)
        {
            if (shift.DayOfWeek is < 0 or > 6)
            {
                throw new ScheduleValidationException("DayOfWeek must be between 0 and 6.");
            }

            var normalizedShift = shift.Shift.Trim();
            if (!string.Equals(normalizedShift, "Morning", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalizedShift, "Evening", StringComparison.OrdinalIgnoreCase))
            {
                throw new ScheduleValidationException("Shift must be Morning or Evening.");
            }

            if (shift.StartTime >= shift.EndTime)
            {
                throw new ScheduleValidationException("Shift end time must be after its start time.");
            }

            if (shift.AppointmentDurationMinutes <= 0 ||
                shift.AppointmentDurationMinutes > (shift.EndTime - shift.StartTime).TotalMinutes)
            {
                throw new ScheduleValidationException("Appointment duration must fit within the shift.");
            }

            var key = (shift.DayOfWeek, normalizedShift.ToUpperInvariant());
            if (!seen.Add(key))
            {
                throw new ScheduleValidationException("A weekday cannot contain the same shift more than once.");
            }
        }
    }
}

public sealed record DoctorShiftInput(
    int DayOfWeek,
    string Shift,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int AppointmentDurationMinutes);

public sealed class ScheduleValidationException(string message) : Exception(message);
