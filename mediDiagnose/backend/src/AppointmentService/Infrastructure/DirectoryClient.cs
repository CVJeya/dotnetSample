using System.Net;
using System.Net.Http.Json;
using AppointmentService.Application;

namespace AppointmentService.Infrastructure;

public sealed class DirectoryClient(HttpClient httpClient, IConfiguration configuration) : IDirectoryClient
{
    private const string ServiceKeyHeader = "X-Internal-Service-Key";
    private readonly string _serviceKey = configuration["InternalServices:ApiKey"]
        ?? throw new InvalidOperationException("InternalServices__ApiKey is not set.");

    public async Task<DirectoryDoctorSchedule?> GetScheduleAsync(
        Guid doctorId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(
            HttpMethod.Get,
            $"/internal/doctors/{doctorId}/shifts?date={date:yyyy-MM-dd}");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DirectoryDoctorSchedule>(
            cancellationToken: cancellationToken);
    }

    public async Task<Guid?> GetDoctorIdByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"/internal/doctors/by-user/{userId}");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var doctor = await response.Content.ReadFromJsonAsync<DirectoryDoctorLookup>(
            cancellationToken: cancellationToken);
        return doctor?.DoctorId;
    }

    public async Task<DirectoryPatientProfile?> GetPatientAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"/internal/patients/{userId}");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DirectoryPatientProfile>(
            cancellationToken: cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string uri)
    {
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add(ServiceKeyHeader, _serviceKey);
        return request;
    }

    private sealed record DirectoryDoctorLookup(Guid DoctorId, Guid UserId);
}
