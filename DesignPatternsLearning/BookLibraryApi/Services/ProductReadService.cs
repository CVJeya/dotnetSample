using BookLibraryApi.Models;
using BookLibraryApi.Repositories;

namespace BookLibraryApi.Services;

public sealed class ProductReadService(IProductRepository repository) : IProductReadService
{
    public Task<Product?> GetAsync(int id) => repository.GetByIdAsync(id);
}
