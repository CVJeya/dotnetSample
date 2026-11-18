using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FoodDelivery.Consumers.Consumers;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    // ─────────────────────────────────────────────────────────
    // Database
    // ─────────────────────────────────────────────────────────
    var connectionString = context.Configuration.GetConnectionString("Default") ?? 
        "Server=sql-server,1433;Database=FoodDelivery;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True";

    services.AddDbContext<AppDbContext>(opts =>
        opts.UseSqlServer(connectionString));

    // ─────────────────────────────────────────────────────────
    // SCENARIO 5, 6, 9, 12: MassTransit + RabbitMQ
    // Consumers register automatically via assembly scan
    // ─────────────────────────────────────────────────────────
    services.AddMassTransit(x =>
    {
        x.AddConsumers(typeof(Program).Assembly);

        x.UsingRabbitMq((ctx, cfg) =>
        {
            var host = context.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
            var username = context.Configuration["RabbitMQ:Username"] ?? "guest";
            var password = context.Configuration["RabbitMQ:Password"] ?? "guest";

            cfg.Host(host, h =>
            {
                h.Username(username);
                h.Password(password);
            });

            // Configure receive endpoints with retry policies
            cfg.ReceiveEndpoint("order-created", e =>
            {
                e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                e.ConfigureConsumer<PaymentConsumer>(ctx);
                e.ConfigureConsumer<InventoryReserveConsumer>(ctx);
                e.ConfigureConsumer<EmailConsumer>(ctx);
                e.ConfigureConsumer<PartnerIntegrationConsumer>(ctx);
                e.ConfigureConsumer<PaymentProcessedNotificationConsumer>(ctx);
                e.ConfigureConsumer<OrderCompletedNotificationConsumer>(ctx);
            });

            // Hosts consumers auto-subscribe to events
            cfg.ConfigureEndpoints(ctx);
        });
    });

    // ─────────────────────────────────────────────────────────
    // External partners HTTP client
    // ─────────────────────────────────────────────────────────
    services.AddHttpClient("ExternalPartners", client =>
    {
        var baseUrl = context.Configuration["PartnerApi:BaseUrl"] ?? "https://api.delivery-partner.com";
        client.BaseAddress = new Uri(baseUrl);
        var key = context.Configuration["PartnerApi:Key"] ?? "demo-key";
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
    });

    // Logging
    services.AddLogging(options =>
    {
        options.AddConsole();
        options.SetMinimumLevel(LogLevel.Information);
    });
});

var host = builder.Build();
await host.RunAsync();

// DbContext for consumers to update order status
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
