using System.Collections.Concurrent;
using BookLibraryApi.Models;
using BookLibraryApi.Services;

namespace BookLibraryApi.Patterns;

// PROXY: controls access to the real service and caches repeated reads.
// Demo note: cache invalidation/expiry is omitted; production apps often use IMemoryCache or Redis.
public sealed class ProxyProductReadService(IProductReadService realService) : IProductReadService
{
    private readonly ConcurrentDictionary<int, Product> _cache = new();

    public async Task<Product?> GetAsync(int id)
    {
        if (_cache.TryGetValue(id, out var cached)) return cached;
        var product = await realService.GetAsync(id);
        if (product is not null) _cache[id] = product;
        return product;
    }
}
