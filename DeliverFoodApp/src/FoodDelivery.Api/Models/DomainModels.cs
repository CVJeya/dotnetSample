namespace FoodDelivery.Api.Models;

public class Order
{
    public Guid Id { get; set; }
    public string CustomerId { get; set; } = "";
    public List<OrderItem> Items { get; set; } = [];
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";  // Pending, PaymentProcessed, InventoryReserved, Completed, Failed
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class OrderItem
{
    public string MenuItemId { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class MenuItem
{
    public string MenuItemId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public string Description { get; set; } = "";
    public bool IsAvailable { get; set; } = true;
}

public class Inventory
{
    public string MenuItemId { get; set; } = "";
    public int QuantityAvailable { get; set; }
    public int ReservedQuantity { get; set; }
    public DateTime LastUpdated { get; set; }
}
