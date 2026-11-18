using BookLibraryApi.Models;
using BookLibraryApi.Services;

namespace BookLibraryApi.Patterns;

// INTERCEPTOR / DECORATOR STYLE: runs cross-cutting logging around another service.
// ASP.NET Core does not provide a universal method-interception feature by default;
// this explicit wrapper makes the interception behavior visible.
public sealed class LoggingProductReadService(
    IProductReadService inner, ILogger<LoggingProductReadService> logger) : IProductReadService
{
    public async Task<Product?> GetAsync(int id)
    {
        logger.LogInformation("Interceptor: starting product read {ProductId}", id);
        var result = await inner.GetAsync(id);
        logger.LogInformation("Interceptor: completed product read {ProductId}", id);
        return result;
    }
}
