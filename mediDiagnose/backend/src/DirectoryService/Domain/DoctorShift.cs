namespace DirectoryService.Domain;

public sealed class DoctorShift
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DoctorId { get; set; }
    public int DayOfWeek { get; set; }
    public string Shift { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int AppointmentDurationMinutes { get; set; }
}
