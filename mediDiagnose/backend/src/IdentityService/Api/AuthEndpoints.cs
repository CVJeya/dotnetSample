using System.ComponentModel.DataAnnotations;
using IdentityService.Application;

namespace IdentityService.Api;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth");

        group.MapPost("/register", async Task<IResult> (
            RegisterPatientRequest request,
            RegisterPatient registerPatient,
            JwtTokenIssuer tokenIssuer,
            CancellationToken cancellationToken) =>
        {
            if (IdentityRequestValidation.Validate(request) is { } validationErrors)
            {
                return Results.ValidationProblem(validationErrors);
            }

            try
            {
                var account = await registerPatient.ExecuteAsync(
                    request.Name,
                    request.Email,
                    request.Password,
                    cancellationToken);
                return Results.Ok(AuthResponse.From(account.Id, account.Role, tokenIssuer.Issue(account)));
            }
            catch (IdentityConflictException exception)
            {
                return Results.Conflict(new { message = exception.Message });
            }
        });

        group.MapPost("/login", async Task<IResult> (
            LoginRequest request,
            AuthenticateUser authenticateUser,
            JwtTokenIssuer tokenIssuer,
            CancellationToken cancellationToken) =>
        {
            if (IdentityRequestValidation.Validate(request) is { } validationErrors)
            {
                return Results.ValidationProblem(validationErrors);
            }

            var account = await authenticateUser.ExecuteAsync(request.Email, request.Password, cancellationToken);
            return account is null
                ? Results.Unauthorized()
                : Results.Ok(AuthResponse.From(account.Id, account.Role, tokenIssuer.Issue(account)));
        });

        return endpoints;
    }
}

public sealed record RegisterPatientRequest(
    [property: Required, StringLength(100, MinimumLength = 1)] string Name,
    [property: Required, EmailAddress, StringLength(200)] string Email,
    [property: Required, StringLength(100, MinimumLength = 8)] string Password);

public sealed record LoginRequest(
    [property: Required, EmailAddress, StringLength(200)] string Email,
    [property: Required] string Password);

public sealed record AuthResponse(string AccessToken, Guid UserId, string Role)
{
    public static AuthResponse From(Guid userId, string role, string accessToken) => new(accessToken, userId, role);
}
