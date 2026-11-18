using MassTransit;
using FoodDelivery.Shared.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace FoodDelivery.Consumers.Consumers;

/// <summary>
/// SCENARIO 5: Payment processing (broker, slow, retry-safe)
/// SCENARIO 9: Charge credit card (with automatic retry and dead-letter)
/// </summary>
public class PaymentConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PaymentConsumer> _logger;

    public PaymentConsumer(IPublishEndpoint publish, ILogger<PaymentConsumer> logger)
    {
        _publishEndpoint = publish;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        _logger.LogInformation("Processing payment for order {OrderId} - Amount: ${Amount}",
            context.Message.OrderId, context.Message.TotalAmount);

        try
        {
            // Simulate credit card charge (3-5 seconds)
            // In real scenario, call Stripe/PayPal/Square API
            await Task.Delay(3000);

            // Simulate random payment failure for demo (10% failure rate)
            if (Random.Shared.Next(0, 10) == 0)
            {
                throw new Exception("Payment gateway returned error: Insufficient funds");
            }

            // SCENARIO 5, 9: Payment succeeded
            await _publishEndpoint.Publish(new PaymentProcessedEvent
            {
                OrderId = context.Message.OrderId,
                TransactionId = Guid.NewGuid().ToString("N"),
                Amount = context.Message.TotalAmount,
                ProcessedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Payment success for order {OrderId}", context.Message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Payment failed for order {OrderId}", context.Message.OrderId);

            // Publish failure event
            await _publishEndpoint.Publish(new PaymentFailedEvent
            {
                OrderId = context.Message.OrderId,
                Reason = ex.Message
            });

            // MassTransit automatically retries based on UseMessageRetry config in Program.cs
            // After max retries, message goes to dead-letter queue
            throw; // Trigger retry
        }
    }
}
