using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using AppointmentService.Application;
using AppointmentService.Domain;
using AppointmentService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Api;

public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/availability/doctors/{doctorId:guid}", async Task<IResult> (
            Guid doctorId,
            DateOnly date,
            string shift,
            GetAvailability availability,
            CancellationToken cancellationToken) =>
        {
            if (shift is not ("Morning" or "Evening"))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["shift"] = ["Shift must be Morning or Evening."]
                });
            }

            var slots = await availability.GetAsync(doctorId, date, shift, cancellationToken);
            return slots is null
                ? Results.NotFound()
                : Results.Ok(slots.Select(ToSlotResponse));
        });

        endpoints.MapPost("/api/appointments", async Task<IResult> (
            ClaimsPrincipal principal,
            CreateAppointmentRequest request,
            RequestAppointment appointments,
            TimeZoneInfo hospitalTimeZone,
            CancellationToken cancellationToken) =>
        {
            if (AppointmentRequestValidation.Validate(request) is { } validationErrors)
            {
                return Results.ValidationProblem(validationErrors);
            }

            if (request.DoctorId == Guid.Empty)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["doctorId"] = ["A doctor ID is required."]
                });
            }

            if (!TryGetSubject(principal, out var patientId))
            {
                return Results.Unauthorized();
            }

            if (!TryParseOffset(request.StartsAt, out var startsAt))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["startsAt"] = ["An offset-bearing timestamp is required."]
                });
            }

            var result = await appointments.CreateAsync(
                patientId,
                request.DoctorId,
                startsAt,
                cancellationToken);
            return result.Outcome switch
            {
                AppointmentRequestOutcome.Created => Results.Created(
                    $"/api/appointments/{result.Appointment!.Id}",
                    ToResponse(result.Appointment, hospitalTimeZone)),
                AppointmentRequestOutcome.DoctorNotFound => Results.NotFound(),
                AppointmentRequestOutcome.PatientProfileRequired => Results.Forbid(),
                _ => Results.Conflict(new { message = "The selected appointment time is no longer available." })
            };
        }).RequireAuthorization("PatientOnly");

        endpoints.MapGet("/api/appointments/mine", async Task<IResult> (
            ClaimsPrincipal principal,
            AppointmentsDbContext db,
            TimeZoneInfo hospitalTimeZone,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetSubject(principal, out var patientId))
            {
                return Results.Unauthorized();
            }

            var appointments = await db.AppointmentRequests.AsNoTracking()
                .Where(item => item.PatientId == patientId)
                .OrderBy(item => item.StartsAtUtc)
                .ToListAsync(cancellationToken);
            return Results.Ok(appointments.Select(appointment => ToResponse(appointment, hospitalTimeZone)));
        }).RequireAuthorization("PatientOnly");

        var adminAppointments = endpoints.MapGroup("/api/admin/appointments")
            .RequireAuthorization("AdminOnly");
        adminAppointments.MapGet("", async Task<IResult> (
            string? status,
            AppointmentsDbContext db,
            IDirectoryClient directory,
            TimeZoneInfo hospitalTimeZone,
            CancellationToken cancellationToken) =>
        {
            var requestedStatus = status ?? AppointmentStatus.Pending;
            if (requestedStatus is not (AppointmentStatus.Pending or AppointmentStatus.Approved or AppointmentStatus.Declined))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["status"] = ["Status must be Pending, Approved, or Declined."]
                });
            }

            var appointments = await db.AppointmentRequests.AsNoTracking()
                .Where(item => item.Status == requestedStatus)
                .OrderBy(item => item.StartsAtUtc)
                .ToListAsync(cancellationToken);
            var response = new List<AppointmentWithPatientResponse>(appointments.Count);
            foreach (var appointment in appointments)
            {
                response.Add(await ToDetailedResponse(
                    appointment,
                    directory,
                    hospitalTimeZone,
                    cancellationToken));
            }

            return Results.Ok(response);
        });

        adminAppointments.MapPost("/{appointmentId:guid}/approve", async Task<IResult> (
            Guid appointmentId,
            ClaimsPrincipal principal,
            DecideAppointment decisions,
            TimeZoneInfo hospitalTimeZone,
            CancellationToken cancellationToken) =>
            await DecideAsync(
                appointmentId,
                principal,
                decisions,
                hospitalTimeZone,
                approve: true,
                cancellationToken));

        adminAppointments.MapPost("/{appointmentId:guid}/decline", async Task<IResult> (
            Guid appointmentId,
            ClaimsPrincipal principal,
            DecideAppointment decisions,
            TimeZoneInfo hospitalTimeZone,
            CancellationToken cancellationToken) =>
            await DecideAsync(
                appointmentId,
                principal,
                decisions,
                hospitalTimeZone,
                approve: false,
                cancellationToken));

        endpoints.MapGet("/api/doctors/me/appointments", async Task<IResult> (
            ClaimsPrincipal principal,
            IDirectoryClient directory,
            AppointmentsDbContext db,
            TimeZoneInfo hospitalTimeZone,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetSubject(principal, out var doctorUserId))
            {
                return Results.Unauthorized();
            }

            var doctorId = await directory.GetDoctorIdByUserIdAsync(doctorUserId, cancellationToken);
            if (doctorId is null)
            {
                return Results.NotFound();
            }

            var appointments = await db.AppointmentRequests.AsNoTracking()
                .Where(item => item.DoctorId == doctorId)
                .OrderBy(item => item.StartsAtUtc)
                .ToListAsync(cancellationToken);
            var response = new List<AppointmentWithPatientResponse>(appointments.Count);
            foreach (var appointment in appointments)
            {
                response.Add(await ToDetailedResponse(
                    appointment,
                    directory,
                    hospitalTimeZone,
                    cancellationToken));
            }

            return Results.Ok(response);
        }).RequireAuthorization("DoctorOnly");

        return endpoints;
    }

    private static async Task<IResult> DecideAsync(
        Guid appointmentId,
        ClaimsPrincipal principal,
        DecideAppointment decisions,
        TimeZoneInfo hospitalTimeZone,
        bool approve,
        CancellationToken cancellationToken)
    {
        if (!TryGetSubject(principal, out var adminUserId))
        {
            return Results.Unauthorized();
        }

        var result = await decisions.DecideAsync(appointmentId, adminUserId, approve, cancellationToken);
        return result.Outcome switch
        {
            AppointmentDecisionOutcome.Decided => Results.Ok(ToResponse(result.Appointment!, hospitalTimeZone)),
            AppointmentDecisionOutcome.NotFound => Results.NotFound(),
            _ => Results.Conflict(new { message = "Only pending appointments can be decided." })
        };
    }

    private static async Task<AppointmentWithPatientResponse> ToDetailedResponse(
        AppointmentRequest appointment,
        IDirectoryClient directory,
        TimeZoneInfo hospitalTimeZone,
        CancellationToken cancellationToken)
    {
        var patient = await directory.GetPatientAsync(appointment.PatientId, cancellationToken)
            ?? throw new InvalidOperationException($"Patient profile {appointment.PatientId} was not found.");
        return new AppointmentWithPatientResponse(
            appointment.Id,
            appointment.DoctorId,
            appointment.PatientId,
            AsOffset(appointment.StartsAtUtc, hospitalTimeZone),
            AsOffset(appointment.EndsAtUtc, hospitalTimeZone),
            appointment.Shift,
            appointment.Status,
            patient.Name,
            patient.Email,
            patient.PhoneNumber);
    }

    private static AppointmentResponse ToResponse(
        AppointmentRequest appointment,
        TimeZoneInfo hospitalTimeZone) =>
        new(
            appointment.Id,
            appointment.DoctorId,
            appointment.PatientId,
            AsOffset(appointment.StartsAtUtc, hospitalTimeZone),
            AsOffset(appointment.EndsAtUtc, hospitalTimeZone),
            appointment.Shift,
            appointment.Status);

    private static AvailableSlotResponse ToSlotResponse(Domain.AppointmentSlot slot) =>
        new(slot.StartsAt, slot.EndsAt);

    private static DateTimeOffset AsOffset(DateTime utc, TimeZoneInfo timeZone) =>
        TimeZoneInfo.ConvertTime(
            new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)),
            timeZone);

    private static bool TryGetSubject(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue("sub"), out userId);

    private static bool TryParseOffset(string value, out DateTimeOffset startsAt)
    {
        var hasOffset = value.EndsWith('Z')
            || value.Length >= 6 && (value[^6] is '+' or '-') && value[^3] == ':';
        if (!hasOffset)
        {
            startsAt = default;
            return false;
        }

        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out startsAt);
    }
}

public sealed record CreateAppointmentRequest(
    [property: Required] Guid DoctorId,
    [property: Required] string StartsAt);

public sealed record AvailableSlotResponse(DateTimeOffset StartsAt, DateTimeOffset EndsAt);

public sealed record AppointmentResponse(
    Guid Id,
    Guid DoctorId,
    Guid PatientId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Shift,
    string Status);

public sealed record AppointmentWithPatientResponse(
    Guid Id,
    Guid DoctorId,
    Guid PatientId,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Shift,
    string Status,
    string PatientName,
    string PatientEmail,
    string PatientPhoneNumber);
