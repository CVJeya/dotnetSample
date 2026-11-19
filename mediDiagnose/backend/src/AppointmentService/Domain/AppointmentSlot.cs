namespace AppointmentService.Domain;

public sealed record AppointmentSlot(DateTimeOffset StartsAt, DateTimeOffset EndsAt);
