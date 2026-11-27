namespace AppointmentService.Domain;

public sealed class ShiftCapacity
{
    public const int MaximumReservations = 20;

    public Guid DoctorId { get; set; }
    public DateOnly LocalDate { get; set; }
    public string Shift { get; set; } = string.Empty;
    public int ReservedCount { get; set; }
}
