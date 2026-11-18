using BookLibraryApi.Mediator;
using BookLibraryApi.Models;
using BookLibraryApi.Repositories;
using BookLibraryApi.UnitOfWork;

namespace BookLibraryApi.CQRS;

// Command handler: performs a write and commits through Unit of Work.
public sealed class CreateProductHandler(
    IProductRepository repository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProductCommand, Product>
{
    public async Task<Product> HandleAsync(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product { Name = request.Name, Price = request.Price, IsActive = true };
        await repository.AddAsync(product);
        await unitOfWork.SaveChangesAsync();
        return product;
    }
}

// Query handlers do not modify state. They are separate from command handlers.
public sealed class GetProductHandler(IProductRepository repository)
    : IRequestHandler<GetProductQuery, Product?>
{
    public Task<Product?> HandleAsync(GetProductQuery request, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(request.Id);
}

public sealed class GetProductsHandler(IProductRepository repository)
    : IRequestHandler<GetProductsQuery, IReadOnlyList<Product>>
{
    public Task<IReadOnlyList<Product>> HandleAsync(GetProductsQuery request, CancellationToken cancellationToken) =>
        repository.GetAllAsync();
}
