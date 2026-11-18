using System.Linq.Expressions;
using BookLibraryApi.Models;

namespace BookLibraryApi.Specifications;

public sealed class ActiveProductsSpecification : ISpecification<Product>
{
    // The same rule can be reused by repositories and application services.
    public Expression<Func<Product, bool>> Criteria => product => product.IsActive;
}
