using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DirectoryService.Application;
using DirectoryService.Domain;
using DirectoryService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Api;

public static class DoctorEndpoints
{
    public static IEndpointRouteBuilder MapDoctorEndpoints(
        this IEndpointRouteBuilder endpoints,
        TimeZoneInfo hospitalTimeZone)
    {
        endpoints.MapGet("/api/doctors", async (DoctorDirectory directory, CancellationToken cancellationToken) =>
        {
            var doctors = await directory.GetPublicDoctorsAsync(cancellationToken);
            return Results.Ok(doctors.Select(ToPublicResponse));
        });

        endpoints.MapGet("/api/doctors/{doctorId:guid}", async (
            Guid doctorId,
            DoctorDirectory directory,
            CancellationToken cancellationToken) =>
        {
            var doctor = await directory.GetPublicDoctorAsync(doctorId, cancellationToken);
            return doctor is null ? Results.NotFound() : Results.Ok(ToPublicResponse(doctor));
        });

        var doctorGroup = endpoints.MapGroup("/api/doctors/me")
            .RequireAuthorization("DoctorOnly");
        doctorGroup.MapGet("/shifts", async (
            ClaimsPrincipal principal,
            DoctorDirectory directory,
            DirectoryDbContext db,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetSubject(principal, out var userId))
            {
                return Results.Unauthorized();
            }

            var doctor = await directory.GetByUserIdAsync(userId, cancellationToken);
            if (doctor is null)
            {
                return Results.NotFound();
            }

            var shifts = await db.DoctorShifts.AsNoTracking()
                .Where(shift => shift.DoctorId == doctor.Id)
                .OrderBy(shift => shift.DayOfWeek)
                .ThenBy(shift => shift.StartTime)
                .ToListAsync(cancellationToken);
            return Results.Ok(shifts.Select(ToShiftResponse));
        });

        doctorGroup.MapPut("/shifts", async Task<IResult> (
            ClaimsPrincipal principal,
            UpdateDoctorScheduleRequest request,
            ManageDoctorSchedule manager,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetSubject(principal, out var userId))
            {
                return Results.Unauthorized();
            }

            if (DirectoryRequestValidation.Validate(request) is { } validationErrors)
            {
                return Results.ValidationProblem(validationErrors);
            }

            try
            {
                var shifts = await manager.ReplaceAsync(userId, request.Shifts, cancellationToken);
                return shifts is null ? Results.NotFound() : Results.NoContent();
            }
            catch (ScheduleValidationException exception)
            {
                return Results.BadRequest(new { message = exception.Message });
            }
        });

        endpoints.MapGet("/internal/doctors/{doctorId:guid}/shifts", async (
            Guid doctorId,
            DateOnly date,
            DoctorDirectory directory,
            ManageDoctorSchedule manager,
            CancellationToken cancellationToken) =>
        {
            var doctor = await directory.GetPublicDoctorAsync(doctorId, cancellationToken);
            if (doctor is null)
            {
                return Results.NotFound();
            }

            var shifts = await manager.GetForDateAsync(doctorId, date.DayOfWeek, cancellationToken);
            return Results.Ok(new DoctorScheduleResponse(
                doctorId,
                hospitalTimeZone.Id,
                shifts.Select(ToShiftResponse).ToArray()));
        });

        var adminDoctors = endpoints.MapGroup("/api/admin/doctors")
            .RequireAuthorization("AdminOnly");
        adminDoctors.MapPost("", async Task<IResult> (
            CreateDoctorProfileRequest request,
            DoctorDirectory directory,
            CancellationToken cancellationToken) =>
        {
            if (DirectoryRequestValidation.Validate(request) is { } validationErrors)
            {
                return Results.ValidationProblem(validationErrors);
            }

            try
            {
                var doctor = await directory.CreateAsync(
                    request.UserId,
                    request.Name,
                    request.Specialty,
                    request.Biography,
                    cancellationToken);
                return Results.Created($"/api/doctors/{doctor.Id}", ToAdminResponse(doctor));
            }
            catch (DirectoryConflictException exception)
            {
                return Results.Conflict(new { message = exception.Message });
            }
        });

        adminDoctors.MapPut("/{doctorId:guid}", async Task<IResult> (
            Guid doctorId,
            UpdateDoctorProfileRequest request,
            DoctorDirectory directory,
            CancellationToken cancellationToken) =>
        {
            if (DirectoryRequestValidation.Validate(request) is { } validationErrors)
            {
                return Results.ValidationProblem(validationErrors);
            }

            var doctor = await directory.UpdateAsync(
                doctorId,
                request.Name,
                request.Specialty,
                request.Biography,
                request.IsActive.GetValueOrDefault(),
                cancellationToken);
            return doctor is null ? Results.NotFound() : Results.Ok(ToAdminResponse(doctor));
        });

        adminDoctors.MapDelete("/{doctorId:guid}", async (
            Guid doctorId,
            DoctorDirectory directory,
            CancellationToken cancellationToken) =>
            await directory.DeactivateAsync(doctorId, cancellationToken)
                ? Results.NoContent()
                : Results.NotFound());

        return endpoints;
    }

    private static PublicDoctorResponse ToPublicResponse(DoctorProfile doctor) =>
        new(doctor.Id, doctor.Name, doctor.Specialty, doctor.Biography);

    private static AdminDoctorResponse ToAdminResponse(DoctorProfile doctor) =>
        new(doctor.Id, doctor.UserId, doctor.Name, doctor.Specialty, doctor.Biography, doctor.IsActive);

    private static DoctorShiftResponse ToShiftResponse(Domain.DoctorShift shift) =>
        new(shift.DayOfWeek, shift.Shift, shift.StartTime, shift.EndTime, shift.AppointmentDurationMinutes);

    private static bool TryGetSubject(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue("sub"), out userId);
}

public sealed record CreateDoctorProfileRequest(
    [property: Required] Guid UserId,
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: Required, StringLength(120, MinimumLength = 1)] string Specialty,
    [property: StringLength(1000)] string? Biography);

public sealed record UpdateDoctorProfileRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: Required, StringLength(120, MinimumLength = 1)] string Specialty,
    [property: StringLength(1000)] string? Biography,
    bool? IsActive);

public sealed record UpdateDoctorScheduleRequest(
    [property: Required] IReadOnlyCollection<DoctorShiftInput> Shifts);

public sealed record PublicDoctorResponse(Guid Id, string Name, string Specialty, string Biography);

public sealed record AdminDoctorResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Specialty,
    string Biography,
    bool IsActive);

public sealed record DoctorShiftResponse(
    int DayOfWeek,
    string Shift,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int AppointmentDurationMinutes);

public sealed record DoctorScheduleResponse(
    Guid DoctorId,
    string HospitalTimeZone,
    IReadOnlyList<DoctorShiftResponse> Shifts);
