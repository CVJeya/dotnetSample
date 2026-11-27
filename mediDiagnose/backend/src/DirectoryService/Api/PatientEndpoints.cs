using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DirectoryService.Application;
using DirectoryService.Domain;

namespace DirectoryService.Api;

public static class PatientEndpoints
{
    public static IEndpointRouteBuilder MapPatientEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var ownProfile = endpoints.MapGroup("/api/patients")
            .RequireAuthorization("PatientOnly");
        ownProfile.MapGet("/me", async Task<IResult> (
            ClaimsPrincipal principal,
            PatientDirectory directory,
            CancellationToken cancellationToken) =>
        {
            if (!TryGetSubject(principal, out var userId))
            {
                return Results.Unauthorized();
            }

            var patient = await directory.GetAsync(userId, cancellationToken);
            return patient is not { IsActive: true }
                ? Results.NotFound()
                : Results.Ok(ToResponse(patient));
        });

        ownProfile.MapPut("/me", async Task<IResult> (
            ClaimsPrincipal principal,
            UpdateOwnPatientProfileRequest request,
            PatientDirectory directory,
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

            var patient = await directory.SaveOwnProfileAsync(
                userId,
                request.Name,
                request.Email,
                request.PhoneNumber,
                cancellationToken);
            return patient is null ? Results.NotFound() : Results.Ok(ToResponse(patient));
        });

        var adminPatients = endpoints.MapGroup("/api/admin/patients")
            .RequireAuthorization("AdminOnly");
        adminPatients.MapGet("", async (
            PatientDirectory directory,
            CancellationToken cancellationToken) =>
        {
            var patients = await directory.GetActivePatientsAsync(cancellationToken);
            return Results.Ok(patients.Select(ToResponse));
        });

        adminPatients.MapPut("/{userId:guid}", async Task<IResult> (
            Guid userId,
            UpdatePatientProfileRequest request,
            PatientDirectory directory,
            CancellationToken cancellationToken) =>
        {
            if (DirectoryRequestValidation.Validate(request) is { } validationErrors)
            {
                return Results.ValidationProblem(validationErrors);
            }

            var patient = await directory.UpdateAsync(
                userId,
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.IsActive.GetValueOrDefault(),
                cancellationToken);
            return patient is null ? Results.NotFound() : Results.Ok(ToResponse(patient));
        });

        adminPatients.MapDelete("/{userId:guid}", async (
            Guid userId,
            PatientDirectory directory,
            CancellationToken cancellationToken) =>
            await directory.DeactivateAsync(userId, cancellationToken)
                ? Results.NoContent()
                : Results.NotFound());

        endpoints.MapGet("/internal/patients/{userId:guid}", async (
            Guid userId,
            HttpRequest request,
            InternalServiceKey serviceKey,
            PatientDirectory directory,
            CancellationToken cancellationToken) =>
        {
            if (!serviceKey.IsValid(request))
            {
                return Results.Unauthorized();
            }

            var patient = await directory.GetAsync(userId, cancellationToken);
            return patient is null ? Results.NotFound() : Results.Ok(ToResponse(patient));
        });

        return endpoints;
    }

    private static PatientProfileResponse ToResponse(PatientProfile patient) =>
        new(patient.UserId, patient.Name, patient.Email, patient.PhoneNumber, patient.IsActive);

    private static bool TryGetSubject(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue("sub"), out userId);
}

public sealed record UpdateOwnPatientProfileRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: Required, EmailAddress, StringLength(200)] string Email,
    [property: StringLength(30)] string? PhoneNumber);

public sealed record UpdatePatientProfileRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: Required, EmailAddress, StringLength(200)] string Email,
    [property: StringLength(30)] string? PhoneNumber,
    bool? IsActive);

public sealed record PatientProfileResponse(Guid UserId, string Name, string Email, string PhoneNumber, bool IsActive);
