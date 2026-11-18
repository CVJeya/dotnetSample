using MassTransit;
using FoodDelivery.Shared.Events;
using Microsoft.Extensions.Logging;

namespace FoodDelivery.Consumers.Consumers;

/// <summary>
/// SCENARIO 3: Send confirmation email (async, don't wait for response)
/// This is one of the 5 services notified when OrderCreatedEvent is published
/// </summary>
public class EmailConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly ILogger<EmailConsumer> _logger;

    public EmailConsumer(ILogger<EmailConsumer> logger) => _logger = logger;

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        _logger.LogInformation("Sending confirmation email to {Email} for order {OrderId}",
            context.Message.CustomerEmail, context.Message.OrderId);

        try
        {
            // Simulate SMTP call (1-2 seconds)
            // In real scenario, use SendGrid/Twilio/AWS SES/SMTP
            await Task.Delay(1000);

            // Email body
            var emailBody = $@"
                Dear Customer,

                Your order #{context.Message.OrderId} has been completed!

                Items:
                {string.Join("\n", context.Message.Items.Select(i => $"  - {i.Name} x{i.Quantity} @ ${i.UnitPrice}"))}

                Total Amount: ${context.Message.TotalAmount}
                Completed At: {context.Message.CompletedAt:G}

                Thank you for your order!
                - Food Delivery Team
            ";

            _logger.LogInformation("Email sent successfully to {Email}", context.Message.CustomerEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", context.Message.CustomerEmail);
            throw;
        }
    }
}
