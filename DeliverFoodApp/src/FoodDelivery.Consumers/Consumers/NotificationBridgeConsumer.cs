using MassTransit;
using FoodDelivery.Shared.Events;
using Microsoft.Extensions.Logging;

namespace FoodDelivery.Consumers.Consumers;

/// <summary>
/// SCENARIO 10: Real-time notifications via SignalR
/// This consumer bridges broker events → SignalR WebSocket push to browser
/// </summary>
public class PaymentProcessedNotificationConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly ILogger<PaymentProcessedNotificationConsumer> _logger;

    public PaymentProcessedNotificationConsumer(ILogger<PaymentProcessedNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        _logger.LogInformation("Publishing payment notification for order {OrderId}", context.Message.OrderId);

        // In real app, push via SignalR to the specific user's browser
        // This consumer would have access to IHubContext<OrderHub> injected
        // await _hubContext.Clients.Group($"user_{/* lookup customerId */}")
        //     .SendAsync("OrderStatusChanged", new { ... });

        await Task.CompletedTask;
    }
}

public class OrderCompletedNotificationConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly ILogger<OrderCompletedNotificationConsumer> _logger;

    public OrderCompletedNotificationConsumer(ILogger<OrderCompletedNotificationConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        _logger.LogInformation("Publishing order completion notification for order {OrderId}", context.Message.OrderId);

        // In real app:
        // await _hubContext.Clients.Group($"user_{context.Message.CustomerId}")
        //     .SendAsync("OrderStatusChanged", new
        //     {
        //         OrderId = context.Message.OrderId,
        //         Status = "Completed",
        //         Message = "Order completed! Your food is being prepared."
        //     });

        await Task.CompletedTask;
    }
}
