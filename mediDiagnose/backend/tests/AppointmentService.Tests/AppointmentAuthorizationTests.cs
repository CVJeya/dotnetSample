using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AppointmentService.Application;
using Xunit;

namespace AppointmentService.Tests;

public class AppointmentAuthorizationTests
{
    [Fact]
    public async Task AppointmentRequest_RequiresActiveDirectoryPatientProfile()
    {
        using var factory = new AppointmentApiFactory();
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        factory.Directory.SetSchedule(doctorId, new DirectoryShift("Morning", new TimeOnly(9, 0), new TimeOnly(12, 0), 30));
        factory.Directory.RemovePatientProfile(patientId);
        using var client = factory.CreateAuthenticatedClient(patientId, "Patient");

        var response = await client.PostAsJsonAsync("/api/appointments", new
        {
            doctorId,
            startsAt = new DateTimeOffset(2026, 11, 23, 9, 0, 0, TimeSpan.Zero)
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AppointmentRequest_RequiresOffsetBearingTimestamp()
    {
        using var factory = new AppointmentApiFactory();
        using var client = factory.CreateAuthenticatedClient(Guid.NewGuid(), "Patient");

        var response = await client.PostAsJsonAsync("/api/appointments", new
        {
            doctorId = Guid.NewGuid(),
            startsAt = "2026-11-23T09:00:00"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RequestAppointment_DerivesPatientFromClaims()
    {
        using var factory = new AppointmentApiFactory();
        var doctorId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        factory.Directory.SetSchedule(doctorId, new DirectoryShift("Morning", new TimeOnly(9, 0), new TimeOnly(12, 0), 30));
        using var patientClient = factory.CreateAuthenticatedClient(patientId, "Patient");
        var response = await patientClient.PostAsJsonAsync("/api/appointments", new
        {
            doctorId,
            startsAt = new DateTimeOffset(2026, 11, 23, 9, 0, 0, TimeSpan.Zero),
            patientId = Guid.NewGuid()
        });
        var appointments = await patientClient.GetFromJsonAsync<JsonElement>("/api/appointments/mine");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(patientId, appointments[0].GetProperty("patientId").GetGuid());
    }

    [Fact]
    public async Task PatientCannotApproveOrReadAnotherPatientsAppointments()
    {
        using var factory = new AppointmentApiFactory();
        var doctorId = Guid.NewGuid();
        factory.Directory.SetSchedule(doctorId, new DirectoryShift("Morning", new TimeOnly(9, 0), new TimeOnly(12, 0), 30));
        using var firstPatientClient = factory.CreateAuthenticatedClient(Guid.NewGuid(), "Patient");
        using var secondPatientClient = factory.CreateAuthenticatedClient(Guid.NewGuid(), "Patient");
        var firstResponse = await firstPatientClient.PostAsJsonAsync("/api/appointments", new
        {
            doctorId,
            startsAt = new DateTimeOffset(2026, 11, 23, 9, 0, 0, TimeSpan.Zero)
        });
        using var firstPayload = JsonDocument.Parse(await firstResponse.Content.ReadAsStringAsync());
        var firstAppointmentId = firstPayload.RootElement.GetProperty("id").GetGuid();
        await secondPatientClient.PostAsJsonAsync("/api/appointments", new
        {
            doctorId,
            startsAt = new DateTimeOffset(2026, 11, 23, 9, 30, 0, TimeSpan.Zero)
        });

        var approveResponse = await firstPatientClient.PostAsync(
            $"/api/admin/appointments/{firstAppointmentId}/approve", null);
        var adminListResponse = await firstPatientClient.GetAsync("/api/admin/appointments?status=Pending");
        var firstPatientsAppointments = await firstPatientClient.GetFromJsonAsync<JsonElement>("/api/appointments/mine");

        Assert.Equal(HttpStatusCode.Forbidden, approveResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, adminListResponse.StatusCode);
        Assert.Equal(1, firstPatientsAppointments.GetArrayLength());
    }

    [Fact]
    public async Task DoctorAppointments_AreScopedToLinkedDirectoryProfile()
    {
        using var factory = new AppointmentApiFactory();
        var doctorUserId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        factory.Directory.LinkDoctor(doctorUserId, doctorId);
        factory.Directory.SetSchedule(doctorId, new DirectoryShift("Morning", new TimeOnly(9, 0), new TimeOnly(12, 0), 30));
        using var patientClient = factory.CreateAuthenticatedClient(Guid.NewGuid(), "Patient");
        var createResponse = await patientClient.PostAsJsonAsync("/api/appointments", new
        {
            doctorId,
            startsAt = new DateTimeOffset(2026, 11, 23, 9, 0, 0, TimeSpan.Zero)
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        using var doctorClient = factory.CreateAuthenticatedClient(doctorUserId, "Doctor");
        var appointments = await doctorClient.GetFromJsonAsync<JsonElement>("/api/doctors/me/appointments");

        Assert.Equal(1, appointments.GetArrayLength());
        Assert.Equal(doctorId, appointments[0].GetProperty("doctorId").GetGuid());
    }
}
