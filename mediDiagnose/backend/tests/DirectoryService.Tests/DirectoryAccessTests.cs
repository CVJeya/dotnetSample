using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DirectoryService.Domain;
using DirectoryService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DirectoryService.Tests;

public class DirectoryAccessTests
{
    [Fact]
    public async Task PublicDoctorBrowsing_DoesNotExposePatientContactInformation()
    {
        using var factory = new DirectoryApiFactory();
        using var client = factory.CreateClient();
        var adminId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(adminId, "Admin"));
        var doctorResponse = await client.PostAsJsonAsync("/api/admin/doctors", new
        {
            userId = Guid.NewGuid(),
            name = "Dr. Public",
            specialty = "Cardiology",
            biography = "Accepts morning and evening appointments."
        });
        Assert.Equal(HttpStatusCode.Created, doctorResponse.StatusCode);

        var privatePhone = "555-0101";
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(Guid.NewGuid(), "Patient"));
        var patientResponse = await client.PutAsJsonAsync("/api/patients/me", new
        {
            name = "Private Patient",
            email = "private.patient@example.com",
            phoneNumber = privatePhone
        });
        Assert.Equal(HttpStatusCode.OK, patientResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = null;
        var response = await client.GetAsync("/api/doctors");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Dr. Public", content);
        Assert.DoesNotContain("private.patient@example.com", content);
        Assert.DoesNotContain(privatePhone, content);
    }

    [Fact]
    public async Task AdminProfileRoutes_RejectUnauthenticatedAndPatientUsers()
    {
        using var factory = new DirectoryApiFactory();
        using var client = factory.CreateClient();

        var anonymousResponse = await client.PostAsJsonAsync("/api/admin/doctors", new
        {
            userId = Guid.NewGuid(),
            name = "Unauthorized Doctor",
            specialty = "Pediatrics"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(Guid.NewGuid(), "Patient"));
        var patientResponse = await client.PostAsJsonAsync("/api/admin/doctors", new
        {
            userId = Guid.NewGuid(),
            name = "Unauthorized Doctor",
            specialty = "Pediatrics"
        });

        Assert.Equal(HttpStatusCode.Forbidden, patientResponse.StatusCode);
    }

    [Fact]
    public async Task AdminCannotCreatePatientProfilesWithoutIdentityAccounts()
    {
        using var factory = new DirectoryApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(Guid.NewGuid(), "Admin"));

        var response = await client.PostAsJsonAsync("/api/admin/patients", new
        {
            userId = Guid.NewGuid(),
            name = "Unlinked Patient",
            email = "unlinked.patient@example.com",
            phoneNumber = "555-0104"
        });

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task AdminDeactivation_KeepsDoctorProfileRecord()
    {
        using var factory = new DirectoryApiFactory();
        using var client = factory.CreateClient();
        var adminId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(adminId, "Admin"));
        var createResponse = await client.PostAsJsonAsync("/api/admin/doctors", new
        {
            userId = Guid.NewGuid(),
            name = "Dr. Inactive",
            specialty = "Neurology"
        });
        using var createPayload = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var doctorId = createPayload.RootElement.GetProperty("id").GetGuid();

        var deleteResponse = await client.DeleteAsync($"/api/admin/doctors/{doctorId}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DirectoryDbContext>();
        var profile = await db.Doctors.FindAsync(doctorId);
        Assert.NotNull(profile);
        Assert.False(profile.IsActive);
    }

    [Fact]
    public async Task PatientMe_UsesAuthenticatedUserIdInsteadOfSubmittedUserId()
    {
        using var factory = new DirectoryApiFactory();
        using var client = factory.CreateClient();
        var patientId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(patientId, "Patient"));

        var updateResponse = await client.PutAsJsonAsync("/api/patients/me", new
        {
            userId = Guid.NewGuid(),
            name = "My Profile",
            email = "patient@example.com",
            phoneNumber = "555-0102"
        });
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var response = await client.GetAsync("/api/patients/me");
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(patientId, payload.RootElement.GetProperty("userId").GetGuid());
        Assert.Equal("My Profile", payload.RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task InternalPatientProfile_RequiresServiceApiKey()
    {
        using var factory = new DirectoryApiFactory();
        using var client = factory.CreateClient();
        var patientId = Guid.NewGuid();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", DirectoryApiFactory.CreateToken(patientId, "Patient"));
        var profile = await client.PutAsJsonAsync("/api/patients/me", new
        {
            name = "Internal Patient",
            email = "internal.patient@example.com",
            phoneNumber = "555-0103"
        });
        Assert.Equal(HttpStatusCode.OK, profile.StatusCode);
        client.DefaultRequestHeaders.Authorization = null;

        var deniedResponse = await client.GetAsync($"/internal/patients/{patientId}");
        Assert.Equal(HttpStatusCode.Unauthorized, deniedResponse.StatusCode);

        client.DefaultRequestHeaders.Add("X-Internal-Service-Key", "incorrect-internal-service-key");
        var invalidKeyResponse = await client.GetAsync($"/internal/patients/{patientId}");
        Assert.Equal(HttpStatusCode.Unauthorized, invalidKeyResponse.StatusCode);

        client.DefaultRequestHeaders.Remove("X-Internal-Service-Key");
        client.DefaultRequestHeaders.Add("X-Internal-Service-Key", DirectoryApiFactory.InternalApiKey);
        var allowedResponse = await client.GetAsync($"/internal/patients/{patientId}");
        using var payload = JsonDocument.Parse(await allowedResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, allowedResponse.StatusCode);
        Assert.Equal("Internal Patient", payload.RootElement.GetProperty("name").GetString());
    }
}
