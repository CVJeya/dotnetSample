namespace AppointmentService.Domain;

public static class AppointmentStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Declined = "Declined";
}

public sealed class AppointmentRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DoctorId { get; set; }
    public Guid PatientId { get; set; }
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public DateOnly LocalDate { get; set; }
    public string Shift { get; set; } = string.Empty;
    public string Status { get; set; } = AppointmentStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? DecidedByUserId { get; set; }
    public DateTime? DecidedAtUtc { get; set; }
}
