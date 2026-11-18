using MassTransit;
using FoodDelivery.Shared.Events;
using Microsoft.Extensions.Logging;

namespace FoodDelivery.Consumers.Consumers;

/// <summary>
/// SCENARIO 6: Notify 5 services (publish-subscribe pattern)
/// This consumer runs in parallel with PaymentConsumer, EmailConsumer, etc.
/// All 5 receive the SAME OrderCreatedEvent without OrderController knowing about them.
/// </summary>
public class InventoryReserveConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<InventoryReserveConsumer> _logger;

    public InventoryReserveConsumer(IPublishEndpoint publish, ILogger<InventoryReserveConsumer> logger)
    {
        _publishEndpoint = publish;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        _logger.LogInformation("Reserving inventory for order {OrderId}", context.Message.OrderId);

        try
        {
            // Only reserve inventory AFTER payment succeeds
            // This avoids reserving stock for orders that never get paid
            await Task.Delay(2000); // Simulate inventory system call

            // Simulate inventory reservation
            var success = Random.Shared.Next(0, 10) != 0; // 90% success rate for demo

            if (!success)
            {
                throw new Exception("Inventory system returned error: Item out of stock");
            }

            await _publishEndpoint.Publish(new InventoryReservedEvent
            {
                OrderId = context.Message.OrderId,
                Success = true,
                FailedItem = null
            });

            _logger.LogInformation("Inventory reserved for order {OrderId}", context.Message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Inventory reservation failed for order {OrderId}", context.Message.OrderId);

            await _publishEndpoint.Publish(new InventoryReservedEvent
            {
                OrderId = context.Message.OrderId,
                Success = false,
                FailedItem = "Pizza"
            });

            throw;
        }
    }
}
