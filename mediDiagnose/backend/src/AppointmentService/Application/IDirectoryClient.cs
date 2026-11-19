namespace AppointmentService.Application;

public interface IDirectoryClient
{
    Task<DirectoryDoctorSchedule?> GetScheduleAsync(
        Guid doctorId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<Guid?> GetDoctorIdByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<DirectoryPatientProfile?> GetPatientAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

public sealed record DirectoryDoctorSchedule(
    Guid DoctorId,
    string HospitalTimeZone,
    IReadOnlyList<DirectoryShift> Shifts);

public sealed record DirectoryShift(
    string Shift,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int AppointmentDurationMinutes);

public sealed record DirectoryPatientProfile(
    Guid UserId,
    string Name,
    string Email,
    string PhoneNumber,
    bool IsActive);
