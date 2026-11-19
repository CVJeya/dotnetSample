using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using IdentityService.Infrastructure;
using Xunit;

namespace IdentityService.Tests;

public class RegistrationTests
{
    [Fact]
    public async Task RegisterPatient_WhenRoleIsSubmitted_CreatesPatientAndHashesPassword()
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();
        var password = "patient-password-123";
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Test Patient",
            email = $"patient-{Guid.NewGuid():N}@example.com",
            password,
            role = "Admin"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Patient", payload.RootElement.GetProperty("role").GetString());
        Assert.True(Guid.TryParse(payload.RootElement.GetProperty("userId").GetString(), out var userId));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var account = await db.UserAccounts.FindAsync(userId);
        Assert.NotNull(account);
        Assert.NotEqual(password, account.PasswordHash);
        Assert.StartsWith("$2", account.PasswordHash);
    }

    [Fact]
    public async Task RegisterPatient_WhenEmailAlreadyExists_ReturnsConflict()
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();
        var registration = new
        {
            name = "Duplicate Patient",
            email = $"duplicate-{Guid.NewGuid():N}@example.com",
            password = "patient-password-123"
        };

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/register", registration)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/auth/register", registration)).StatusCode);
    }
}
