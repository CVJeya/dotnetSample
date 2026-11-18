using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.Api.Data;

namespace FoodDelivery.Api.Controllers;

/// <summary>
/// SCENARIO 4: Check inventory stock (REST query - instant answer needed)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<InventoryController> _logger;

    public InventoryController(AppDbContext db, ILogger<InventoryController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("{menuItemId}/stock")]
    public async Task<IActionResult> CheckStock(string menuItemId)
    {
        _logger.LogInformation("Checking stock for item: {MenuItemId}", menuItemId);
        
        var inventory = await _db.Inventory
            .Where(i => i.MenuItemId == menuItemId)
            .FirstOrDefaultAsync();

        if (inventory == null)
            return NotFound(new { Message = "Item not found" });

        return Ok(new
        {
            MenuItemId = menuItemId,
            Available = inventory.QuantityAvailable > 0,
            QuantityAvailable = inventory.QuantityAvailable,
            ReservedQuantity = inventory.ReservedQuantity,
            TotalQuantity = inventory.QuantityAvailable + inventory.ReservedQuantity
        });
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllInventory()
    {
        var inventory = await _db.Inventory.ToListAsync();
        return Ok(inventory);
    }

    [HttpPost("{menuItemId}/reserve")]
    public async Task<IActionResult> ReserveStock(string menuItemId, [FromBody] int quantity)
    {
        _logger.LogInformation("Reserving {Quantity} units of {MenuItemId}", quantity, menuItemId);
        
        var inventory = await _db.Inventory
            .FirstOrDefaultAsync(i => i.MenuItemId == menuItemId);

        if (inventory == null)
            return NotFound(new { Message = "Item not found" });

        if (inventory.QuantityAvailable < quantity)
            return BadRequest(new { Message = "Insufficient stock", AvailableQuantity = inventory.QuantityAvailable });

        inventory.QuantityAvailable -= quantity;
        inventory.ReservedQuantity += quantity;
        inventory.LastUpdated = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Stock reserved", RemainingQuantity = inventory.QuantityAvailable });
    }
}
