using BuildingBlocks.Configuration;
using IdentityService.Api;
using IdentityService.Application;
using IdentityService.Domain;
using IdentityService.Infrastructure;
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

builder.Services.AddDbContext<IdentityDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<RegisterPatient>();
builder.Services.AddScoped<AuthenticateUser>();
builder.Services.AddScoped<CreateDoctorAccount>();
builder.Services.AddScoped<BootstrapAdmin>();
builder.Services.AddSingleton<JwtTokenIssuer>();
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
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(AccountRole.Admin)));

var app = builder.Build();
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
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
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "identity" }));
app.MapAuthEndpoints();
app.MapDoctorAccountEndpoints();

if (args.Contains("--bootstrap-admin", StringComparer.Ordinal))
{
    var email = Environment.GetEnvironmentVariable("INITIAL_ADMIN_EMAIL")
        ?? throw new InvalidOperationException("INITIAL_ADMIN_EMAIL is required for admin bootstrap.");
    var password = Environment.GetEnvironmentVariable("INITIAL_ADMIN_PASSWORD")
        ?? throw new InvalidOperationException("INITIAL_ADMIN_PASSWORD is required for admin bootstrap.");
    await using var scope = app.Services.CreateAsyncScope();
    var bootstrap = scope.ServiceProvider.GetRequiredService<BootstrapAdmin>();
    await bootstrap.RunAsync(email, password);
    Console.WriteLine("Initial administrator account created.");
    return;
}

await app.RunAsync();

public partial class Program;
