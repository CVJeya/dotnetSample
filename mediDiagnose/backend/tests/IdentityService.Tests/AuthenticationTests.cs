using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IdentityService.Tests;

public class AuthenticationTests
{
    [Fact]
    public async Task Login_WhenPasswordIsIncorrect_ReturnsUnauthorized()
    {
        using var factory = new IdentityApiFactory();
        using var client = factory.CreateClient();
        var email = $"login-{Guid.NewGuid():N}@example.com";
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/register", new
        {
            name = "Login Patient",
            email,
            password = "patient-password-123"
        })).StatusCode);

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "wrong-password-123"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
