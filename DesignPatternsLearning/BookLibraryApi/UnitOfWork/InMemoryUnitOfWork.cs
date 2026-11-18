namespace BookLibraryApi.UnitOfWork;

public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    // This is a no-op for the demo store, where writes are immediate.
    // With EF Core, this method would call DbContext.SaveChangesAsync().
    public Task SaveChangesAsync() => Task.CompletedTask;
}
