using BookLibraryApi.Models;

namespace BookLibraryApi.Services;

public interface IProductReadService
{
    Task<Product?> GetAsync(int id);
}
