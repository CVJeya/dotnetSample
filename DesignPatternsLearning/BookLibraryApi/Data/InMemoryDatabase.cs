using System.Collections.Concurrent;
using BookLibraryApi.Models;

namespace BookLibraryApi.Data;

// Demo-only backing store. Replace with EF Core DbContext in a database-backed app.
public sealed class InMemoryDatabase
{
    public ConcurrentDictionary<int, Product> Products { get; } = new();
    public int NextId = 0;
}
