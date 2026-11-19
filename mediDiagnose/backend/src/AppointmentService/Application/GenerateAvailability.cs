using AppointmentService.Domain;

namespace AppointmentService.Application;

public static class GenerateAvailability
{
    public static IReadOnlyList<AppointmentSlot> Generate(
        DateOnly date,
        DoctorShiftSchedule shift,
        TimeZoneInfo hospitalTimeZone)
    {
        if (shift.EndTime <= shift.StartTime || shift.AppointmentDurationMinutes <= 0)
        {
            throw new ArgumentException("The doctor shift is invalid.", nameof(shift));
        }

        var shiftStart = date.ToDateTime(shift.StartTime, DateTimeKind.Unspecified);
        var shiftEnd = date.ToDateTime(shift.EndTime, DateTimeKind.Unspecified);
        var duration = TimeSpan.FromMinutes(shift.AppointmentDurationMinutes);
        var slots = new List<AppointmentSlot>();

        for (var start = shiftStart; start + duration <= shiftEnd; start += duration)
        {
            var end = start + duration;
            if (!TryGetOffset(start, hospitalTimeZone, out var startOffset)
                || !TryGetOffset(end, hospitalTimeZone, out var endOffset)
                || startOffset != endOffset)
            {
                continue;
            }

            slots.Add(new AppointmentSlot(
                new DateTimeOffset(start, startOffset),
                new DateTimeOffset(end, endOffset)));
        }

        return slots;
    }

    private static bool TryGetOffset(DateTime localTime, TimeZoneInfo timeZone, out TimeSpan offset)
    {
        if (timeZone.IsInvalidTime(localTime) || timeZone.IsAmbiguousTime(localTime))
        {
            offset = default;
            return false;
        }

        offset = timeZone.GetUtcOffset(localTime);
        return true;
    }
}
