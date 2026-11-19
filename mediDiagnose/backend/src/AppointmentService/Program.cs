using BuildingBlocks.Configuration;
using AppointmentService.Api;
using AppointmentService.Application;
using AppointmentService.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connectionString = DatabaseConnectionSettings.RequireDbUrl(builder.Configuration);
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt__Key is not set.");
if (jwtKey.Length < 32)
{
    throw new InvalidOperationException("Jwt__Key must be at least 32 characters.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MediDiagnose";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MediDiagnoseClient";
var timeZoneId = builder.Configuration["Hospital:TimeZone"]
    ?? throw new InvalidOperationException("Hospital__TimeZone is not set.");
var hospitalTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
var directoryBaseUrl = builder.Configuration["Directory:BaseUrl"]
    ?? throw new InvalidOperationException("Directory__BaseUrl is not set.");
if (!Uri.TryCreate(directoryBaseUrl, UriKind.Absolute, out var directoryUri)
    || directoryUri.Scheme is not ("http" or "https"))
{
    throw new InvalidOperationException("Directory__BaseUrl must be an absolute HTTP or HTTPS URL.");
}

var internalServiceKey = builder.Configuration["InternalServices:ApiKey"]
    ?? throw new InvalidOperationException("InternalServices__ApiKey is not set.");
if (internalServiceKey.Length < 32)
{
    throw new InvalidOperationException("InternalServices__ApiKey must be at least 32 characters.");
}

builder.Services.AddDbContext<AppointmentsDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddSingleton(hospitalTimeZone);
builder.Services.AddScoped<GetAvailability>();
builder.Services.AddScoped<RequestAppointment>();
builder.Services.AddScoped<DecideAppointment>();
builder.Services.AddHttpClient<IDirectoryClient, DirectoryClient>(client => client.BaseAddress = directoryUri);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            RoleClaimType = "role",
            NameClaimType = "name"
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DoctorOnly", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("PatientOnly", policy => policy.RequireRole("Patient"));
});

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppointmentsDbContext>();
    if (db.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
    {
        await db.Database.MigrateAsync();
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }
}

app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "appointments" }));
app.MapAppointmentEndpoints();
app.Run();

public partial class Program;
