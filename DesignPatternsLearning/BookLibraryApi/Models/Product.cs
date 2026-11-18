namespace BookLibraryApi.Models;

// Shared domain model used by the API pattern demonstrations.
public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
