using Microsoft.AspNetCore.Mvc;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using FoodDelivery.Api.Data;
using FoodDelivery.Api.Models;
using FoodDelivery.Api.Hubs;
using FoodDelivery.Shared.Events;

namespace FoodDelivery.Api.Controllers;

/// <summary>
/// SCENARIO 2: Place order (REST 202 Accepted + async broker processing)
/// SCENARIO 7: Get order history (REST query)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        AppDbContext db,
        IPublishEndpoint publish,
        IHubContext<OrderHub> hub,
        ILogger<OrderController> logger)
    {
        _db = db;
        _publishEndpoint = publish;
        _hubContext = hub;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderRequest request)
    {
        _logger.LogInformation("Placing order for customer {CustomerId}", request.CustomerId);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = request.CustomerId,
            Items = request.Items,
            TotalAmount = request.Items.Sum(i => i.UnitPrice * i.Quantity),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // SCENARIO 2: Publish event for async processing
        // Return 202 Accepted immediately (don't wait for consumers)
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            CustomerEmail = request.CustomerEmail,
            Items = request.Items.Select(i => new OrderItemDto
            {
                MenuItemId = i.MenuItemId,
                Name = i.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList(),
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt
        };

        await _publishEndpoint.Publish(orderCreatedEvent);

        // SCENARIO 10: Real-time notification via SignalR
        await _hubContext.Clients.User(order.CustomerId)
            .SendAsync("OrderStatusChanged", new
            {
                OrderId = order.Id,
                Status = "Pending",
                Message = "Order placed! Processing payment..."
            });

        _logger.LogInformation("Order {OrderId} published to broker", order.Id);

        return Accepted(new { OrderId = order.Id, Status = "Pending" });
    }

    // SCENARIO 7: Get order history (REST query)
    [HttpGet("history/{customerId}")]
    public async Task<IActionResult> GetHistory(string customerId)
    {
        _logger.LogInformation("Retrieving order history for customer {CustomerId}", customerId);

        var orders = await _db.Orders
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(50)
            .ToListAsync();

        return Ok(new { Count = orders.Count, Orders = orders });
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderDetail(Guid orderId)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return NotFound(new { Message = "Order not found" });

        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var orders = await _db.Orders
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var total = await _db.Orders.CountAsync();

        return Ok(new { Total = total, Page = page, PageSize = pageSize, Orders = orders });
    }
}

public class CreateOrderRequest
{
    public string CustomerId { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public List<OrderItem> Items { get; set; } = [];
}
