using BookLibraryApi.Mediator;
using BookLibraryApi.Models;

namespace BookLibraryApi.CQRS;

// CQRS: commands change state; queries read state.
public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<Product>;
public sealed record GetProductQuery(int Id) : IRequest<Product?>;
public sealed record GetProductsQuery() : IRequest<IReadOnlyList<Product>>;
