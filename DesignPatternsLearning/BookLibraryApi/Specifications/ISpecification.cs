using System.Linq.Expressions;

namespace BookLibraryApi.Specifications;

// SPECIFICATION: encapsulates a reusable business/query rule.
public interface ISpecification<T>
{
    Expression<Func<T, bool>> Criteria { get; }
}
