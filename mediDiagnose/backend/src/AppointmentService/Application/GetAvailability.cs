using AppointmentService.Domain;
using AppointmentService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application;

public sealed class GetAvailability(
    AppointmentsDbContext db,
    IDirectoryClient directory,
    TimeZoneInfo hospitalTimeZone)
{
    public async Task<IReadOnlyList<AppointmentSlot>?> GetAsync(
        Guid doctorId,
        DateOnly date,
        string shiftName,
        CancellationToken cancellationToken = default)
    {
        var schedule = await directory.GetScheduleAsync(doctorId, date, cancellationToken);
        if (schedule is null)
        {
            return null;
        }

        if (!string.Equals(schedule.HospitalTimeZone, hospitalTimeZone.Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Directory and Appointments services use different hospital time zones.");
        }

        var shift = schedule.Shifts.FirstOrDefault(item =>
            string.Equals(item.Shift, shiftName, StringComparison.Ordinal));
        if (shift is null)
        {
            return [];
        }

        var capacity = await db.ShiftCapacities.AsNoTracking()
            .Where(item => item.DoctorId == doctorId && item.LocalDate == date && item.Shift == shiftName)
            .Select(item => (int?)item.ReservedCount)
            .SingleOrDefaultAsync(cancellationToken);
        if (capacity >= RequestAppointment.ShiftCapacityLimit)
        {
            return [];
        }

        var reservedStarts = await db.AppointmentRequests.AsNoTracking()
            .Where(item => item.DoctorId == doctorId
                && item.LocalDate == date
                && item.Shift == shiftName
                && item.Status != AppointmentStatus.Declined)
            .Select(item => item.StartsAtUtc)
            .ToListAsync(cancellationToken);
        var reservedUtc = reservedStarts.ToHashSet();

        return GenerateAvailability.Generate(date, new DoctorShiftSchedule(
                shift.Shift,
                shift.StartTime,
                shift.EndTime,
                shift.AppointmentDurationMinutes),
            hospitalTimeZone)
            .Where(slot => !reservedUtc.Contains(slot.StartsAt.UtcDateTime))
            .ToArray();
    }
}
