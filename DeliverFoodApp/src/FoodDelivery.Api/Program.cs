using MassTransit;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.Api.Data;
using FoodDelivery.Api.Hubs;
using FoodDelivery.Api.Services;
using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────
// SCENARIO 1, 2, 4, 7: REST Controllers + health checks
// ─────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ─────────────────────────────────────────────────────────
// Database (SQL Server)
// ─────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(
        builder.Configuration.GetConnectionString("Default") ?? 
        "Server=sql-server,1433;Database=FoodDelivery;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True"
    ));

// ─────────────────────────────────────────────────────────
// SCENARIO 5, 6, 9, 12: MassTransit + RabbitMQ
// ─────────────────────────────────────────────────────────
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var username = builder.Configuration["RabbitMQ:Username"] ?? "guest";
        var password = builder.Configuration["RabbitMQ:Password"] ?? "guest";

        cfg.Host(host, h =>
        {
            h.Username(username);
            h.Password(password);
        });

        // Receive endpoints for different consumers
        cfg.ReceiveEndpoint("order-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        });

        cfg.ReceiveEndpoint("payment-processed", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        });

        cfg.ReceiveEndpoint("inventory-reserved", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        });

        cfg.ReceiveEndpoint("order-completed", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        });
    });
});

// ─────────────────────────────────────────────────────────
// SCENARIO 7 (alternative): gRPC
// ─────────────────────────────────────────────────────────
builder.Services.AddGrpc();

// ─────────────────────────────────────────────────────────
// SCENARIO 8: Kafka Producer
// ─────────────────────────────────────────────────────────
builder.Services.AddSingleton<IProducer<string, string>>(sp =>
{
    var bootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "kafka:9092";
    var config = new ProducerConfig
    {
        BootstrapServers = bootstrapServers,
        Acks = Acks.All
    };
    return new ProducerBuilder<string, string>(config).Build();
});

// ─────────────────────────────────────────────────────────
// SCENARIO 10: SignalR for real-time notifications
// ─────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ─────────────────────────────────────────────────────────
// SCENARIO 11: External partner HTTP calls
// ─────────────────────────────────────────────────────────
builder.Services.AddHttpClient("ExternalPartners", client =>
{
    var baseUrl = builder.Configuration["PartnerApi:BaseUrl"] ?? "https://api.delivery-partner.com";
    client.BaseAddress = new Uri(baseUrl);
    var key = builder.Configuration["PartnerApi:Key"] ?? "demo-key";
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
});

var app = builder.Build();

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<OrderGrpcService>();
app.MapHub<OrderHub>("/hubs/orders");

// Health checks (for K8s probes)
app.MapGet("/health", () => Results.Ok("Healthy"))
    .WithName("Health")
    .WithOpenApi();

app.MapGet("/health/ready", async (AppDbContext db) =>
{
    try
    {
        await db.Database.ExecuteSqlAsync($"SELECT 1");
        return Results.Ok("Ready");
    }
    catch
    {
        return Results.StatusCode(503);
    }
})
.WithName("Ready")
.WithOpenApi();

app.Run();
