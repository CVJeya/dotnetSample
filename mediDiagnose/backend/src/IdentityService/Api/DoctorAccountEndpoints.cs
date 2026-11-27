using System.ComponentModel.DataAnnotations;
using IdentityService.Application;

namespace IdentityService.Api;

public static class DoctorAccountEndpoints
{
    public static IEndpointRouteBuilder MapDoctorAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/admin/doctor-accounts", async Task<IResult> (
            CreateDoctorAccountRequest request,
            CreateDoctorAccount createDoctorAccount,
            CancellationToken cancellationToken) =>
        {
            if (IdentityRequestValidation.Validate(request) is { } validationErrors)
            {
                return Results.ValidationProblem(validationErrors);
            }

            try
            {
                var account = await createDoctorAccount.ExecuteAsync(
                    request.Name,
                    request.Email,
                    request.Password,
                    cancellationToken);
                return Results.Created(
                    $"/api/admin/doctor-accounts/{account.Id}",
                    new { userId = account.Id, account.Name, account.Email, account.Role });
            }
            catch (IdentityConflictException exception)
            {
                return Results.Conflict(new { message = exception.Message });
            }
        }).RequireAuthorization("AdminOnly");

        return endpoints;
    }
}

public sealed record CreateDoctorAccountRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: Required, EmailAddress, StringLength(200)] string Email,
    [property: Required, StringLength(100, MinimumLength = 8)] string Password);
