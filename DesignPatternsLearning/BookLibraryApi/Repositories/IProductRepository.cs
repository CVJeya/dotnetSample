using BookLibraryApi.Models;
using BookLibraryApi.Specifications;

namespace BookLibraryApi.Repositories;

// REPOSITORY: hides the details of how products are stored and queried.
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<IReadOnlyList<Product>> GetAllAsync();
    Task<IReadOnlyList<Product>> FindAsync(ISpecification<Product> specification);
    Task AddAsync(Product product);
    void Update(Product product);
    void Delete(Product product);
}
