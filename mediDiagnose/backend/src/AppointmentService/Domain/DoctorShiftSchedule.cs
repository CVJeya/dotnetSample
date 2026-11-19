namespace AppointmentService.Domain;

public sealed record DoctorShiftSchedule(
    string Shift,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int AppointmentDurationMinutes);
