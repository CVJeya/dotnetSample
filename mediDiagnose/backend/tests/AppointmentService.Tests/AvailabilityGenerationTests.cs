using AppointmentService.Application;
using AppointmentService.Domain;
using Xunit;

namespace AppointmentService.Tests;

public class AvailabilityGenerationTests
{
    [Fact]
    public void GeneratedSlots_EndNoLaterThanShiftClose()
    {
        var date = new DateOnly(2026, 11, 23);
        var shift = new DoctorShiftSchedule(
            "Morning",
            new TimeOnly(10, 0),
            new TimeOnly(11, 0),
            40);

        var slots = GenerateAvailability.Generate(date, shift, TimeZoneInfo.Utc);

        var slot = Assert.Single(slots);
        Assert.Equal(new DateTimeOffset(2026, 11, 23, 10, 0, 0, TimeSpan.Zero), slot.StartsAt);
        Assert.Equal(new DateTimeOffset(2026, 11, 23, 10, 40, 0, TimeSpan.Zero), slot.EndsAt);
    }
}
