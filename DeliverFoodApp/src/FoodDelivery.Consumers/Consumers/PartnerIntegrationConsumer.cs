using MassTransit;
using FoodDelivery.Shared.Events;
using Microsoft.Extensions.Logging;

namespace FoodDelivery.Consumers.Consumers;

/// <summary>
/// SCENARIO 11: External partner integration (broker trigger → REST call)
/// Even though this trigger came from a broker, the actual call to the
/// external partner is REST because they don't speak AMQP.
/// </summary>
public class PartnerIntegrationConsumer : IConsumer<OrderCompletedEvent>
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PartnerIntegrationConsumer> _logger;

    public PartnerIntegrationConsumer(
        IHttpClientFactory httpClientFactory,
        ILogger<PartnerIntegrationConsumer> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ExternalPartners");
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        _logger.LogInformation("Notifying delivery partner for order {OrderId}", context.Message.OrderId);

        try
        {
            // SCENARIO 11: REST call to external partner
            var partnerPayload = new
            {
                OrderId = context.Message.OrderId.ToString(),
                CustomerId = context.Message.CustomerId,
                Items = context.Message.Items,
                TotalAmount = context.Message.TotalAmount,
                CompletedAt = context.Message.CompletedAt
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/v2/orders",
                partnerPayload);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully notified delivery partner for order {OrderId}",
                    context.Message.OrderId);
            }
            else
            {
                _logger.LogWarning("Partner API returned {StatusCode} for order {OrderId}",
                    response.StatusCode, context.Message.OrderId);
                throw new HttpRequestException($"Partner API returned {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify delivery partner for order {OrderId}",
                context.Message.OrderId);

            // BROKER BENEFIT: if partner is down, message stays in queue
            // and retries automatically (no manual retry needed)
            throw;
        }
    }
}
