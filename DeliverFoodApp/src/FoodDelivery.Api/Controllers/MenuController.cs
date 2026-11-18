using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FoodDelivery.Api.Data;

namespace FoodDelivery.Api.Controllers;

/// <summary>
/// SCENARIO 1: Search products (REST query - immediate answer needed)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<MenuController> _logger;

    public MenuController(AppDbContext db, ILogger<MenuController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        _logger.LogInformation("Searching menu items with query: {Query}", query);
        
        var items = await _db.MenuItems
            .Where(m => m.Name.Contains(query) || m.Category.Contains(query))
            .Take(20)
            .ToListAsync();
        
        return Ok(new { Count = items.Count, Items = items });
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetByCategory(string category)
    {
        var items = await _db.MenuItems
            .Where(m => m.Category == category)
            .ToListAsync();
        
        return Ok(items);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.MenuItems.ToListAsync();
        return Ok(items);
    }
}
