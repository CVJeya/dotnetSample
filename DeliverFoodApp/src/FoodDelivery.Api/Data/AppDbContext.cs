using Microsoft.EntityFrameworkCore;
using FoodDelivery.Api.Models;

namespace FoodDelivery.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<MenuItem> MenuItems { get; set; } = null!;
    public DbSet<Inventory> Inventory { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed menu items
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem 
            { 
                MenuItemId = "item-1", 
                Name = "Margherita Pizza", 
                Category = "Pizza", 
                Price = 12.99m,
                Description = "Classic pizza with mozzarella and basil",
                IsAvailable = true
            },
            new MenuItem 
            { 
                MenuItemId = "item-2", 
                Name = "Pepperoni Pizza", 
                Category = "Pizza", 
                Price = 14.99m,
                Description = "Pizza with pepperoni and cheese",
                IsAvailable = true
            },
            new MenuItem 
            { 
                MenuItemId = "item-3", 
                Name = "Caesar Salad", 
                Category = "Salad", 
                Price = 8.99m,
                Description = "Fresh Caesar salad with croutons",
                IsAvailable = true
            },
            new MenuItem 
            { 
                MenuItemId = "item-4", 
                Name = "Garlic Bread", 
                Category = "Sides", 
                Price = 4.99m,
                Description = "Toasted bread with garlic butter",
                IsAvailable = true
            },
            new MenuItem 
            { 
                MenuItemId = "item-5", 
                Name = "Coca Cola", 
                Category = "Beverages", 
                Price = 2.99m,
                Description = "Cold Coca Cola",
                IsAvailable = true
            }
        );

        // Seed inventory
        modelBuilder.Entity<Inventory>().HasData(
            new Inventory { MenuItemId = "item-1", QuantityAvailable = 100, ReservedQuantity = 0, LastUpdated = DateTime.UtcNow },
            new Inventory { MenuItemId = "item-2", QuantityAvailable = 100, ReservedQuantity = 0, LastUpdated = DateTime.UtcNow },
            new Inventory { MenuItemId = "item-3", QuantityAvailable = 50, ReservedQuantity = 0, LastUpdated = DateTime.UtcNow },
            new Inventory { MenuItemId = "item-4", QuantityAvailable = 200, ReservedQuantity = 0, LastUpdated = DateTime.UtcNow },
            new Inventory { MenuItemId = "item-5", QuantityAvailable = 150, ReservedQuantity = 0, LastUpdated = DateTime.UtcNow }
        );
    }
}
