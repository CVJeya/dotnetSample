using Microsoft.AspNetCore.SignalR;

namespace FoodDelivery.Api.Hubs;

/// <summary>
/// SCENARIO 10: Real-time notifications via SignalR WebSocket
/// Users connect and receive push updates about their orders
/// </summary>
public class OrderHub : Hub
{
    private readonly ILogger<OrderHub> _logger;

    public OrderHub(ILogger<OrderHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.GetHttpContext()?.Request.Query["userId"].ToString();
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} connected to order hub", userId);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("User disconnected from order hub");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SubscribeToOrder(Guid orderId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        _logger.LogInformation("Connection {ConnectionId} subscribed to order {OrderId}", 
            Context.ConnectionId, orderId);
    }

    public async Task UnsubscribeFromOrder(Guid orderId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from order {OrderId}", 
            Context.ConnectionId, orderId);
    }
}
