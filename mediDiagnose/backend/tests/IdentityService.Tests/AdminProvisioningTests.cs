using System.Net;
using System.Net.Http.Json;
using System.Text;
using IdentityService.Application;
using IdentityService.Domain;
using IdentityService.Infrastructure;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IdentityService.Tests;

public class AdminProvisioningTests
{
    [Fact]
    public async Task DoctorAccountEndpoint_WithoutAdminAuthentication_ReturnsUnauthorized()
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/admin/doctor-accounts", new
        {
            name = "Doctor",
            email = $"doctor-{Guid.NewGuid():N}@example.com",
            password = "doctor-password-123"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DoctorAccountEndpoint_WithAdminAuthentication_CreatesDoctorAccount()
    {
        using var factory = new IdentityApiFactory();
        var adminEmail = $"admin-{Guid.NewGuid():N}@example.com";
        const string adminPassword = "admin-password-123";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            await new BootstrapAdmin(db).RunAsync(adminEmail, adminPassword);
        }

        using var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = adminEmail, password = adminPassword });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        using var loginPayload = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", loginPayload.RootElement.GetProperty("accessToken").GetString());

        var response = await client.PostAsJsonAsync("/api/admin/doctor-accounts", new
        {
            name = "New Doctor",
            email = $"doctor-{Guid.NewGuid():N}@example.com",
            password = "doctor-password-123"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var responsePayload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Doctor", responsePayload.RootElement.GetProperty("role").GetString());
    }

    [Fact]
    public async Task DoctorAccountEndpoint_WithPatientAuthentication_ReturnsForbidden()
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();
        var registration = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Patient",
            email = $"patient-{Guid.NewGuid():N}@example.com",
            password = "patient-password-123"
        });
        using var registrationPayload = JsonDocument.Parse(await registration.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer", registrationPayload.RootElement.GetProperty("accessToken").GetString());

        var response = await client.PostAsJsonAsync("/api/admin/doctor-accounts", new
        {
            name = "Unauthorized Doctor",
            email = $"doctor-{Guid.NewGuid():N}@example.com",
            password = "doctor-password-123"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BootstrapAdmin_WhenNoAdminExists_CreatesAdminAccount()
    {
        using var factory = new IdentityApiFactory();
        var email = $"bootstrap-{Guid.NewGuid():N}@example.com";
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var account = await new BootstrapAdmin(db).RunAsync(email, "admin-password-123");

        Assert.Equal(AccountRole.Admin, account.Role);
        Assert.Equal(email, account.Email);
        Assert.NotEqual("admin-password-123", account.PasswordHash);
    }

    [Fact]
    public async Task BootstrapAdmin_WhenPasswordIsTooShort_RejectsCredentials()
    {
        using var factory = new IdentityApiFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        await Assert.ThrowsAsync<ArgumentException>(
            () => new BootstrapAdmin(db).RunAsync("admin@example.com", "short"));
    }

    [Fact]
    public async Task BootstrapAdmin_WhenAdminAlreadyExists_RefusesToCreateAnother()
    {
        using var factory = new IdentityApiFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        db.UserAccounts.Add(new UserAccount
        {
            Id = Guid.NewGuid(),
            Name = "Existing Admin",
            Email = $"existing-admin-{Guid.NewGuid():N}@example.com",
            PasswordHash = "already-hashed",
            Role = AccountRole.Admin,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });
        await db.SaveChangesAsync();

        var bootstrap = new BootstrapAdmin(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => bootstrap.RunAsync("another-admin@example.com", "admin-password-123"));
    }
}
