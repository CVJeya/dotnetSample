using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Tests;

public sealed class GatewayApiFactory : WebApplicationFactory<Program>
{
    private readonly EchoServer _identity = EchoServer.StartAsync("Identity").GetAwaiter().GetResult();
    private readonly EchoServer _directory = EchoServer.StartAsync("Directory").GetAwaiter().GetResult();
    private readonly EchoServer _appointments = EchoServer.StartAsync("Appointments").GetAwaiter().GetResult();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ServiceUrls:Identity", _identity.BaseAddress);
        builder.UseSetting("ServiceUrls:Directory", _directory.BaseAddress);
        builder.UseSetting("ServiceUrls:Appointments", _appointments.BaseAddress);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _identity.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _directory.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _appointments.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
    }

    private sealed class EchoServer : IAsyncDisposable
    {
        private readonly WebApplication _app;

        private EchoServer(WebApplication app, string baseAddress)
        {
            _app = app;
            BaseAddress = baseAddress;
        }

        public string BaseAddress { get; }

        public static async Task<EchoServer> StartAsync(string service)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
            var app = builder.Build();
            app.MapGet("/{**path}", async (HttpContext context) =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await context.Response.WriteAsJsonAsync(new
                {
                    service,
                    path = context.Request.Path.Value,
                    query = context.Request.QueryString.Value,
                    authorization = context.Request.Headers.Authorization.ToString()
                });
            });
            await app.StartAsync();
            var address = app.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()!
                .Addresses.Single();
            return new EchoServer(app, address);
        }

        public async ValueTask DisposeAsync()
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
