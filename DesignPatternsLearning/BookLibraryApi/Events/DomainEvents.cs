namespace BookLibraryApi.Events;

// EVENT SOURCING: events describe facts that have already happened.
public abstract record ProductEvent(int ProductId, DateTime OccurredAt);
public sealed record ProductCreated(int Id, string Name, decimal InitialPrice, DateTime At)
    : ProductEvent(Id, At);
public sealed record ProductPriceChanged(int Id, decimal NewPrice, DateTime At)
    : ProductEvent(Id, At);
