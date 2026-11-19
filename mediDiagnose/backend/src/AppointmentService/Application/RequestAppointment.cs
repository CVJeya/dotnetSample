using System.Data;
using AppointmentService.Domain;
using AppointmentService.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Application;

public enum AppointmentRequestOutcome
{
    Created,
    DoctorNotFound,
    PatientProfileRequired,
    Unavailable
}

public sealed record AppointmentRequestResult(
    AppointmentRequestOutcome Outcome,
    AppointmentRequest? Appointment);

public sealed class RequestAppointment(
    AppointmentsDbContext db,
    IDirectoryClient directory,
    TimeZoneInfo hospitalTimeZone)
{
    public const int ShiftCapacityLimit = ShiftCapacity.MaximumReservations;

    public async Task<AppointmentRequestResult> CreateAsync(
        Guid patientId,
        Guid doctorId,
        DateTimeOffset startsAt,
        CancellationToken cancellationToken = default)
    {
        var patient = await directory.GetPatientAsync(patientId, cancellationToken);
        if (patient is not { IsActive: true })
        {
            return new AppointmentRequestResult(AppointmentRequestOutcome.PatientProfileRequired, null);
        }

        var localDate = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(startsAt, hospitalTimeZone).DateTime);
        var schedule = await directory.GetScheduleAsync(doctorId, localDate, cancellationToken);
        if (schedule is null)
        {
            return new AppointmentRequestResult(AppointmentRequestOutcome.DoctorNotFound, null);
        }

        if (!string.Equals(schedule.HospitalTimeZone, hospitalTimeZone.Id, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Directory and Appointments services use different hospital time zones.");
        }

        var startsAtUtc = startsAt.UtcDateTime;
        var selectedShift = schedule.Shifts.FirstOrDefault(shift =>
            GenerateAvailability.Generate(
                    localDate,
                    new DoctorShiftSchedule(
                        shift.Shift,
                        shift.StartTime,
                        shift.EndTime,
                        shift.AppointmentDurationMinutes),
                    hospitalTimeZone)
                .Any(slot => slot.StartsAt.UtcDateTime == startsAtUtc));
        if (selectedShift is null)
        {
            return new AppointmentRequestResult(AppointmentRequestOutcome.Unavailable, null);
        }

        var generatedSlot = GenerateAvailability.Generate(
                localDate,
                new DoctorShiftSchedule(
                    selectedShift.Shift,
                    selectedShift.StartTime,
                    selectedShift.EndTime,
                    selectedShift.AppointmentDurationMinutes),
                hospitalTimeZone)
            .Single(slot => slot.StartsAt.UtcDateTime == startsAtUtc);

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "ShiftCapacities" ("DoctorId", "LocalDate", "Shift", "ReservedCount")
            VALUES ({doctorId}, {localDate}, {selectedShift.Shift}, 0)
            ON CONFLICT ("DoctorId", "LocalDate", "Shift") DO NOTHING
            """, cancellationToken);

        var reserved = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE "ShiftCapacities"
            SET "ReservedCount" = "ReservedCount" + 1
            WHERE "DoctorId" = {doctorId}
                AND "LocalDate" = {localDate}
                AND "Shift" = {selectedShift.Shift}
                AND "ReservedCount" < {ShiftCapacityLimit}
            """, cancellationToken);
        if (reserved == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new AppointmentRequestResult(AppointmentRequestOutcome.Unavailable, null);
        }

        var appointment = new AppointmentRequest
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            PatientId = patientId,
            StartsAtUtc = startsAtUtc,
            EndsAtUtc = generatedSlot.EndsAt.UtcDateTime,
            LocalDate = localDate,
            Shift = selectedShift.Shift,
            Status = AppointmentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
        var inserted = await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "AppointmentRequests"
                ("Id", "DoctorId", "PatientId", "StartsAtUtc", "EndsAtUtc", "LocalDate",
                 "Shift", "Status", "CreatedAtUtc", "DecidedByUserId", "DecidedAtUtc")
            VALUES
                ({appointment.Id}, {appointment.DoctorId}, {appointment.PatientId},
                 {appointment.StartsAtUtc}, {appointment.EndsAtUtc}, {appointment.LocalDate},
                 {appointment.Shift}, {appointment.Status}, {appointment.CreatedAtUtc}, NULL, NULL)
            ON CONFLICT DO NOTHING
            """, cancellationToken);
        if (inserted == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new AppointmentRequestResult(AppointmentRequestOutcome.Unavailable, null);
        }

        await transaction.CommitAsync(cancellationToken);
        return new AppointmentRequestResult(AppointmentRequestOutcome.Created, appointment);
    }
}
