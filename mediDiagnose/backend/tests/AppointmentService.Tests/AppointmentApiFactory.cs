using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using AppointmentService.Application;
using AppointmentService.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace AppointmentService.Tests;

public sealed class AppointmentApiFactory : WebApplicationFactory<Program>
{
    public const string JwtKey = "appointments-test-secret-key-that-is-at-least-32-characters";
    public static readonly DateOnly TestDate = new(2026, 11, 23);
    public TestDirectoryClient Directory { get; } = new();
    private readonly string _databaseName = $"appointments-tests-{Guid.NewGuid():N}";
    private SqliteConnection? _keepAlive;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _keepAlive = new SqliteConnection($"Data Source={_databaseName};Mode=Memory;Cache=Shared");
        _keepAlive.Open();

        builder.UseSetting("DB_URL", "Host=localhost;Database=appointments-tests;Username=test;Password=test");
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Jwt:Issuer", "MediDiagnose");
        builder.UseSetting("Jwt:Audience", "MediDiagnoseClient");
        builder.UseSetting("Hospital:TimeZone", "UTC");
        builder.UseSetting("Directory:BaseUrl", "http://directory.test");
        builder.UseSetting("InternalServices:ApiKey", "appointment-test-internal-key-at-least-32");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDbContextOptionsConfiguration<AppointmentsDbContext>>();
            services.RemoveAll<DbContextOptions<AppointmentsDbContext>>();
            services.AddDbContext<AppointmentsDbContext>(options =>
                options.UseSqlite($"Data Source={_databaseName};Mode=Memory;Cache=Shared"));
            services.RemoveAll<IDirectoryClient>();
            services.AddSingleton<IDirectoryClient>(Directory);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _keepAlive?.Dispose();
        }
    }

    public HttpClient CreateAuthenticatedClient(Guid userId, string role)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", CreateToken(userId, role));
        return client;
    }

    public static string CreateToken(Guid userId, string role)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            "MediDiagnose",
            "MediDiagnoseClient",
            [
                new Claim("sub", userId.ToString()),
                new Claim("role", role),
                new Claim("name", "Test User")
            ],
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public sealed class TestDirectoryClient : IDirectoryClient
{
    private readonly ConcurrentDictionary<Guid, DirectoryDoctorSchedule> _schedules = new();
    private readonly ConcurrentDictionary<Guid, Guid> _doctorIdsByUserId = new();
    private readonly ConcurrentDictionary<Guid, bool> _missingPatientProfiles = new();

    public void SetSchedule(Guid doctorId, params DirectoryShift[] shifts) =>
        _schedules[doctorId] = new DirectoryDoctorSchedule(doctorId, "UTC", shifts);

    public void LinkDoctor(Guid userId, Guid doctorId) => _doctorIdsByUserId[userId] = doctorId;

    public void RemovePatientProfile(Guid userId) => _missingPatientProfiles[userId] = true;

    public Task<DirectoryDoctorSchedule?> GetScheduleAsync(
        Guid doctorId,
        DateOnly date,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_schedules.GetValueOrDefault(doctorId));

    public Task<Guid?> GetDoctorIdByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<Guid?>(_doctorIdsByUserId.TryGetValue(userId, out var doctorId) ? doctorId : null);

    public Task<DirectoryPatientProfile?> GetPatientAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<DirectoryPatientProfile?>(_missingPatientProfiles.ContainsKey(userId)
            ? null
            : new DirectoryPatientProfile(userId, $"Patient {userId}", $"patient-{userId}@example.com", "555-0100", true));
}
