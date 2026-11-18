using System.Collections.Concurrent;

namespace BookLibraryApi.Events;

// Demo event store. Events are lost when the process stops; use durable storage in production.
public sealed class InMemoryEventStore
{
    private readonly ConcurrentDictionary<int, List<ProductEvent>> _streams = new();

    public void Append(ProductEvent @event)
    {
        var stream = _streams.GetOrAdd(@event.ProductId, _ => []);
        lock (stream) stream.Add(@event);
    }

    public IReadOnlyList<ProductEvent> Read(int productId)
    {
        if (!_streams.TryGetValue(productId, out var stream)) return [];
        lock (stream) return stream.ToList();
    }
}
