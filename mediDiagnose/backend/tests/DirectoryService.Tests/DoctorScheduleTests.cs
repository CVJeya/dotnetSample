using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace DirectoryService.Tests;

public class DoctorScheduleTests
{
    [Fact]
    public async Task DoctorCanEditOwnScheduleAndDirectoryReturnsConfiguredTimeZone()
    {
        using var factory = new DirectoryApiFactory();
        using var client = factory.CreateClient();
        var doctorUserId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(Guid.NewGuid(), "Admin"));
        var createResponse = await client.PostAsJsonAsync("/api/admin/doctors", new
        {
            userId = doctorUserId,
            name = "Dr. Schedule",
            specialty = "Family Medicine"
        });
        using var doctorPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var doctorId = doctorPayload.RootElement.GetProperty("id").GetGuid();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(doctorUserId, "Doctor"));
        var updateResponse = await client.PutAsJsonAsync("/api/doctors/me/shifts", new
        {
            shifts = new[]
            {
                new { dayOfWeek = 1, shift = "Morning", startTime = "09:00", endTime = "12:00", appointmentDurationMinutes = 30 }
            }
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deniedResponse = await client.GetAsync($"/internal/doctors/{doctorId}/shifts?date=2026-11-23");
        Assert.Equal(HttpStatusCode.Unauthorized, deniedResponse.StatusCode);

        client.DefaultRequestHeaders.Add("X-Internal-Service-Key", DirectoryApiFactory.InternalApiKey);
        var response = await client.GetAsync($"/internal/doctors/{doctorId}/shifts?date=2026-11-23");
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Asia/Kolkata", payload.RootElement.GetProperty("hospitalTimeZone").GetString());
        Assert.Equal(1, payload.RootElement.GetProperty("shifts").GetArrayLength());
        Assert.Equal("Morning", payload.RootElement.GetProperty("shifts")[0].GetProperty("shift").GetString());
    }

    [Fact]
    public async Task InternalDoctorLookup_RequiresServiceKeyAndResolvesIdentityUser()
    {
        using var factory = new DirectoryApiFactory();
        using var client = factory.CreateClient();
        var doctorUserId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(Guid.NewGuid(), "Admin"));
        var createResponse = await client.PostAsJsonAsync("/api/admin/doctors", new
        {
            userId = doctorUserId,
            name = "Dr. Internal",
            specialty = "Family Medicine"
        });
        using var doctorPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var doctorId = doctorPayload.RootElement.GetProperty("id").GetGuid();
        client.DefaultRequestHeaders.Authorization = null;

        var deniedResponse = await client.GetAsync($"/internal/doctors/by-user/{doctorUserId}");
        Assert.Equal(HttpStatusCode.Unauthorized, deniedResponse.StatusCode);

        client.DefaultRequestHeaders.Add("X-Internal-Service-Key", DirectoryApiFactory.InternalApiKey);
        var response = await client.GetAsync($"/internal/doctors/by-user/{doctorUserId}");
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(doctorId, payload.RootElement.GetProperty("doctorId").GetGuid());
    }

    [Fact]
    public async Task DoctorSchedule_WhenShiftEndsBeforeItStarts_ReturnsBadRequest()
    {
        using var factory = new DirectoryApiFactory();
        using var client = factory.CreateClient();
        var doctorUserId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(Guid.NewGuid(), "Admin"));
        await client.PostAsJsonAsync("/api/admin/doctors", new
        {
            userId = doctorUserId,
            name = "Dr. Invalid Schedule",
            specialty = "Family Medicine"
        });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(doctorUserId, "Doctor"));
        var response = await client.PutAsJsonAsync("/api/doctors/me/shifts", new
        {
            shifts = new[]
            {
                new { dayOfWeek = 1, shift = "Morning", startTime = "12:00", endTime = "09:00", appointmentDurationMinutes = 30 }
            }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DoctorWithoutLinkedProfile_CannotModifyAnotherDoctorsSchedule()
    {
        using var factory = new DirectoryApiFactory();
        using var client = factory.CreateClient();
        var doctorUserId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(Guid.NewGuid(), "Admin"));
        await client.PostAsJsonAsync("/api/admin/doctors", new
        {
            userId = doctorUserId,
            name = "Dr. Linked",
            specialty = "Family Medicine"
        });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(Guid.NewGuid(), "Doctor"));
        var response = await client.PutAsJsonAsync("/api/doctors/me/shifts", new
        {
            shifts = new[]
            {
                new { dayOfWeek = 1, shift = "Morning", startTime = "09:00", endTime = "12:00", appointmentDurationMinutes = 30 }
            }
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
