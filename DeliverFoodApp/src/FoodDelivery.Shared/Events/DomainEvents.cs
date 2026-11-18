namespace FoodDelivery.Shared.Events;

public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public string CustomerId { get; init; } = "";
    public string CustomerEmail { get; init; } = "";
    public List<OrderItemDto> Items { get; init; } = [];
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record PaymentProcessedEvent
{
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; } = "";
    public decimal Amount { get; init; }
    public DateTime ProcessedAt { get; init; }
}

public record PaymentFailedEvent
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = "";
}

public record InventoryReservedEvent
{
    public Guid OrderId { get; init; }
    public bool Success { get; init; }
    public string? FailedItem { get; init; }
}

public record OrderCompletedEvent
{
    public Guid OrderId { get; init; }
    public string CustomerId { get; init; } = "";
    public string CustomerEmail { get; init; } = "";
    public List<OrderItemDto> Items { get; init; } = [];
    public decimal TotalAmount { get; init; }
    public DateTime CompletedAt { get; init; }
}

public record RefundPaymentEvent
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string Reason { get; init; } = "";
}

public class OrderItemDto
{
    public string MenuItemId { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
