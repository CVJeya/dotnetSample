using BuildingBlocks.Configuration;
using DirectoryService.Api;
using DirectoryService.Application;
using DirectoryService.Infrastructure;
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
var internalServiceKey = builder.Configuration["InternalServices:ApiKey"]
    ?? throw new InvalidOperationException("InternalServices__ApiKey is not set.");
if (internalServiceKey.Length < 32)
{
    throw new InvalidOperationException("InternalServices__ApiKey must be at least 32 characters.");
}

builder.Services.AddDbContext<DirectoryDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddSingleton(new InternalServiceKey(internalServiceKey));
builder.Services.AddScoped<DoctorDirectory>();
builder.Services.AddScoped<PatientDirectory>();
builder.Services.AddScoped<ManageDoctorSchedule>();
builder.Services.AddSingleton(hospitalTimeZone);
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
    var db = scope.ServiceProvider.GetRequiredService<DirectoryDbContext>();
    if (db.Database.IsRelational())
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
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "directory" }));
app.MapDoctorEndpoints(hospitalTimeZone);
app.MapPatientEndpoints();
await app.RunAsync();

public partial class Program;
