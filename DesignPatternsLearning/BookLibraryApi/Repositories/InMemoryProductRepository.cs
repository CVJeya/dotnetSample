using BookLibraryApi.Data;
using BookLibraryApi.Models;
using BookLibraryApi.Specifications;

namespace BookLibraryApi.Repositories;

public sealed class InMemoryProductRepository(InMemoryDatabase database) : IProductRepository
{
    public Task<Product?> GetByIdAsync(int id)
    {
        database.Products.TryGetValue(id, out var product);
        return Task.FromResult(product is null ? null : Copy(product));
    }

    public Task<IReadOnlyList<Product>> GetAllAsync() =>
        Task.FromResult<IReadOnlyList<Product>>(database.Products.Values.Select(Copy).ToList());

    public Task<IReadOnlyList<Product>> FindAsync(ISpecification<Product> specification)
    {
        var predicate = specification.Criteria.Compile();
        IReadOnlyList<Product> result = database.Products.Values
            .Where(predicate).Select(Copy).ToList();
        return Task.FromResult(result);
    }

    public Task AddAsync(Product product)
    {
        if (product.Id == 0) product.Id = Interlocked.Increment(ref database.NextId);
        database.Products[product.Id] = Copy(product);
        return Task.CompletedTask;
    }

    public void Update(Product product) => database.Products[product.Id] = Copy(product);
    public void Delete(Product product) => database.Products.TryRemove(product.Id, out _);

    private static Product Copy(Product p) =>
        new() { Id = p.Id, Name = p.Name, Price = p.Price, IsActive = p.IsActive };
}
