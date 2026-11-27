using AppointmentService.Domain;
using AppointmentService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application;

public enum AppointmentDecisionOutcome
{
    Decided,
    NotFound,
    AlreadyDecided
}

public sealed record AppointmentDecisionResult(
    AppointmentDecisionOutcome Outcome,
    AppointmentRequest? Appointment);

public sealed class DecideAppointment(AppointmentsDbContext db)
{
    public async Task<AppointmentDecisionResult> DecideAsync(
        Guid appointmentId,
        Guid adminUserId,
        bool approve,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var decision = approve ? AppointmentStatus.Approved : AppointmentStatus.Declined;
        var updated = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "AppointmentRequests"
            SET "Status" = {decision},
                "DecidedByUserId" = {adminUserId},
                "DecidedAtUtc" = {DateTime.UtcNow}
            WHERE "Id" = {appointmentId}
                AND "Status" = {AppointmentStatus.Pending}
            """, cancellationToken);

        if (updated == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            var exists = await db.AppointmentRequests.AsNoTracking()
                .AnyAsync(item => item.Id == appointmentId, cancellationToken);
            return new AppointmentDecisionResult(
                exists ? AppointmentDecisionOutcome.AlreadyDecided : AppointmentDecisionOutcome.NotFound,
                null);
        }

        if (!approve)
        {
            var appointment = await db.AppointmentRequests.AsNoTracking()
                .SingleAsync(item => item.Id == appointmentId, cancellationToken);
            var released = await db.Database.ExecuteSqlInterpolatedAsync($"""
                UPDATE "ShiftCapacities"
                SET "ReservedCount" = "ReservedCount" - 1
                WHERE "DoctorId" = {appointment.DoctorId}
                    AND "LocalDate" = {appointment.LocalDate}
                    AND "Shift" = {appointment.Shift}
                    AND "ReservedCount" > 0
                """, cancellationToken);
            if (released != 1)
            {
                throw new InvalidOperationException("The pending appointment has no matching capacity reservation.");
            }
        }

        await transaction.CommitAsync(cancellationToken);
        var decidedAppointment = await db.AppointmentRequests.AsNoTracking()
            .SingleAsync(item => item.Id == appointmentId, cancellationToken);
        return new AppointmentDecisionResult(AppointmentDecisionOutcome.Decided, decidedAppointment);
    }
}
