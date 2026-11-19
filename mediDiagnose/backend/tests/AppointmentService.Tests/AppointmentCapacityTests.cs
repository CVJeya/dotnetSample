using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AppointmentService.Application;
using AppointmentService.Domain;
using AppointmentService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AppointmentService.Tests;

public class AppointmentCapacityTests
{
    [Fact]
    public async Task AvailabilityForUnknownDoctor_ReturnsNotFound()
    {
        using var factory = new AppointmentApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/availability/doctors/{Guid.NewGuid()}?date=2026-11-23&shift=Morning");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ReservesPendingRequestsAndCapsEachShiftAtTwenty()
    {
        using var factory = new AppointmentApiFactory();
        var doctorId = Guid.NewGuid();
        factory.Directory.SetSchedule(doctorId, new DirectoryShift("Morning", new TimeOnly(9, 0), new TimeOnly(15, 0), 15));
        using var client = factory.CreateAuthenticatedClient(Guid.NewGuid(), "Patient");
        var slots = GenerateAvailability.Generate(
            AppointmentApiFactory.TestDate,
            new DoctorShiftSchedule("Morning", new TimeOnly(9, 0), new TimeOnly(15, 0), 15),
            TimeZoneInfo.Utc);

        for (var index = 0; index < 20; index++)
        {
            var response = await RequestAppointmentAsync(client, doctorId, slots[index].StartsAt);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        using var publicClient = factory.CreateClient();
        var fullAvailability = await publicClient.GetFromJsonAsync<JsonElement>(
            $"/api/availability/doctors/{doctorId}?date=2026-11-23&shift=Morning");
        var fullShiftResponse = await RequestAppointmentAsync(client, doctorId, slots[20].StartsAt);
        Assert.Equal(0, fullAvailability.GetArrayLength());
        Assert.Equal(HttpStatusCode.Conflict, fullShiftResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppointmentsDbContext>();
        Assert.Equal(20, await db.ShiftCapacities.Select(capacity => capacity.ReservedCount).SingleAsync());
        Assert.Equal(20, await db.AppointmentRequests.CountAsync(request => request.Status == AppointmentStatus.Pending));
    }

    [Fact]
    public async Task ConcurrentRequestsForFinalCapacity_NeverExceedsTwenty()
    {
        using var factory = new AppointmentApiFactory();
        var doctorId = Guid.NewGuid();
        factory.Directory.SetSchedule(doctorId, new DirectoryShift("Morning", new TimeOnly(9, 0), new TimeOnly(15, 0), 15));
        using var client = factory.CreateAuthenticatedClient(Guid.NewGuid(), "Patient");
        var slots = GenerateAvailability.Generate(
            AppointmentApiFactory.TestDate,
            new DoctorShiftSchedule("Morning", new TimeOnly(9, 0), new TimeOnly(15, 0), 15),
            TimeZoneInfo.Utc);

        for (var index = 0; index < 19; index++)
        {
            Assert.Equal(HttpStatusCode.Created,
                (await RequestAppointmentAsync(client, doctorId, slots[index].StartsAt)).StatusCode);
        }

        var responses = await Task.WhenAll(
            RequestAppointmentAsync(client, doctorId, slots[19].StartsAt),
            RequestAppointmentAsync(client, doctorId, slots[20].StartsAt));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppointmentsDbContext>();
        Assert.Equal(20, await db.ShiftCapacities.Select(capacity => capacity.ReservedCount).SingleAsync());
    }

    [Fact]
    public async Task ConcurrentRequestsForSameSlot_OnlyOneSucceeds()
    {
        using var factory = new AppointmentApiFactory();
        var doctorId = Guid.NewGuid();
        factory.Directory.SetSchedule(doctorId, new DirectoryShift("Morning", new TimeOnly(9, 0), new TimeOnly(15, 0), 15));
        using var client = factory.CreateAuthenticatedClient(Guid.NewGuid(), "Patient");
        var startsAt = GenerateAvailability.Generate(
            AppointmentApiFactory.TestDate,
            new DoctorShiftSchedule("Morning", new TimeOnly(9, 0), new TimeOnly(15, 0), 15),
            TimeZoneInfo.Utc)[0].StartsAt;

        var responses = await Task.WhenAll(
            RequestAppointmentAsync(client, doctorId, startsAt),
            RequestAppointmentAsync(client, doctorId, startsAt));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Conflict);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppointmentsDbContext>();
        Assert.Equal(1, await db.ShiftCapacities.Select(capacity => capacity.ReservedCount).SingleAsync());
    }

    [Fact]
    public async Task DeclinedRequest_ReleasesCapacityAndSlot()
    {
        using var factory = new AppointmentApiFactory();
        var doctorId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        factory.Directory.SetSchedule(doctorId, new DirectoryShift("Morning", new TimeOnly(9, 0), new TimeOnly(15, 0), 15));
        using var patientClient = factory.CreateAuthenticatedClient(Guid.NewGuid(), "Patient");
        using var adminClient = factory.CreateAuthenticatedClient(adminId, "Admin");
        var startsAt = GenerateAvailability.Generate(
            AppointmentApiFactory.TestDate,
            new DoctorShiftSchedule("Morning", new TimeOnly(9, 0), new TimeOnly(15, 0), 15),
            TimeZoneInfo.Utc)[0].StartsAt;
        var createResponse = await RequestAppointmentAsync(patientClient, doctorId, startsAt);
        using var created = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var appointmentId = created.RootElement.GetProperty("id").GetGuid();

        var declineResponse = await adminClient.PostAsync($"/api/admin/appointments/{appointmentId}/decline", null);
        var retryResponse = await RequestAppointmentAsync(patientClient, doctorId, startsAt);

        Assert.Equal(HttpStatusCode.OK, declineResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, retryResponse.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppointmentsDbContext>();
        Assert.Equal(1, await db.ShiftCapacities.Select(capacity => capacity.ReservedCount).SingleAsync());
        var declined = await db.AppointmentRequests.FirstAsync(request => request.Status == AppointmentStatus.Declined);
        Assert.Equal(adminId, declined.DecidedByUserId);
        Assert.NotNull(declined.DecidedAtUtc);
    }

    [Fact]
    public async Task ScheduleEdit_PreservesExistingAppointments()
    {
        using var factory = new AppointmentApiFactory();
        var doctorId = Guid.NewGuid();
        var originalShift = new DirectoryShift("Morning", new TimeOnly(9, 0), new TimeOnly(12, 0), 30);
        factory.Directory.SetSchedule(doctorId, originalShift);
        using var client = factory.CreateAuthenticatedClient(Guid.NewGuid(), "Patient");
        var originalStart = new DateTimeOffset(2026, 11, 23, 9, 0, 0, TimeSpan.Zero);
        var createResponse = await RequestAppointmentAsync(client, doctorId, originalStart);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        factory.Directory.SetSchedule(doctorId, new DirectoryShift("Morning", new TimeOnly(10, 0), new TimeOnly(12, 0), 30));
        using var publicClient = factory.CreateClient();
        var availability = await publicClient.GetFromJsonAsync<JsonElement>(
            $"/api/availability/doctors/{doctorId}?date=2026-11-23&shift=Morning");

        Assert.DoesNotContain(availability.EnumerateArray(),
            slot => slot.GetProperty("startsAt").GetDateTimeOffset() == originalStart);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppointmentsDbContext>();
        Assert.Equal(originalStart.UtcDateTime,
            (await db.AppointmentRequests.SingleAsync()).StartsAtUtc);
    }

    private static Task<HttpResponseMessage> RequestAppointmentAsync(
        HttpClient client,
        Guid doctorId,
        DateTimeOffset startsAt) =>
        client.PostAsJsonAsync("/api/appointments", new { doctorId, startsAt });
}
