using System.Net.Http.Headers;
using System.Text.Json;
using Xunit;

namespace Gateway.Tests;

public class GatewayRoutingTests
{
    [Theory]
    [InlineData("/api/auth/login", "Identity")]
    [InlineData("/api/admin/doctor-accounts", "Identity")]
    [InlineData("/api/doctors", "Directory")]
    [InlineData("/api/patients/me", "Directory")]
    [InlineData("/api/admin/doctors", "Directory")]
    [InlineData("/api/admin/doctors/doctor-id", "Directory")]
    [InlineData("/api/admin/patients", "Directory")]
    [InlineData("/api/admin/patients/patient-id", "Directory")]
    [InlineData("/api/availability/doctors/doctor-id", "Appointments")]
    [InlineData("/api/appointments/mine", "Appointments")]
    [InlineData("/api/admin/appointments", "Appointments")]
    [InlineData("/api/doctors/me/appointments", "Appointments")]
    public async Task ApiRoute_IsForwardedToExpectedService(string path, string expectedService)
    {
        using var factory = new GatewayApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(path);
        Assert.Equal(200, (int)response.StatusCode);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(expectedService, payload.RootElement.GetProperty("service").GetString());
        Assert.Equal(path, payload.RootElement.GetProperty("path").GetString());
    }

    [Fact]
    public async Task AuthorizationHeader_IsPreservedWhenForwarding()
    {
        using var factory = new GatewayApiFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "gateway-test-token");

        var response = await client.GetAsync("/api/appointments/mine");
        Assert.Equal(200, (int)response.StatusCode);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal("Bearer gateway-test-token",
            payload.RootElement.GetProperty("authorization").GetString());
    }
}
