using FoodDelivery.Api.Data;
using FoodDelivery.Api.Services;
using Grpc.Core;

namespace FoodDelivery.Api.Services;

/// <summary>
/// SCENARIO 7: Get order history (gRPC - fast internal queries)
/// gRPC is 10-100x faster than REST for internal microservices
/// </summary>
public class OrderGrpcService : OrderGrpc.OrderGrpcBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<OrderGrpcService> _logger;

    public OrderGrpcService(AppDbContext db, ILogger<OrderGrpcService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public override async Task<OrderHistoryResponse> GetOrderHistory(
        OrderHistoryRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("gRPC: GetOrderHistory for customer {CustomerId}", request.CustomerId);

        var orders = await _db.Orders
            .Where(o => o.CustomerId == request.CustomerId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(request.Limit > 0 ? request.Limit : 50)
            .Select(o => new OrderSummary
            {
                OrderId = o.Id.ToString(),
                TotalAmount = (double)o.TotalAmount,
                Status = o.Status,
                CreatedAt = o.CreatedAt.ToString("O")
            })
            .ToListAsync();

        var response = new OrderHistoryResponse();
        response.Orders.AddRange(orders);
        
        _logger.LogInformation("gRPC: Returned {Count} orders", orders.Count);
        return response;
    }

    public override async Task<OrderDetailResponse> GetOrderDetail(
        OrderDetailRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("gRPC: GetOrderDetail for order {OrderId}", request.OrderId);

        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == Guid.Parse(request.OrderId));

        if (order == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Order not found"));
        }

        var response = new OrderDetailResponse
        {
            OrderId = order.Id.ToString(),
            CustomerId = order.CustomerId,
            TotalAmount = (double)order.TotalAmount,
            Status = order.Status,
            CreatedAt = order.CreatedAt.ToString("O"),
            UpdatedAt = (order.UpdatedAt?.ToString("O")) ?? ""
        };

        response.Items.AddRange(order.Items.Select(i => new OrderItem
        {
            MenuItemId = i.MenuItemId,
            Name = i.Name,
            Quantity = i.Quantity,
            UnitPrice = (double)i.UnitPrice
        }));

        return response;
    }
}
