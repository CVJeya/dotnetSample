namespace BookLibraryApi.Events;

// Aggregate state is rebuilt by replaying events. The event stream is the source of truth.
public sealed class ProductAggregate
{
    public int Id { get; private set; }
    public string Name { get; private set; } = "";
    public decimal Price { get; private set; }

    public static ProductAggregate FromHistory(IEnumerable<ProductEvent> history)
    {
        var aggregate = new ProductAggregate();
        foreach (var item in history) aggregate.Apply(item);
        return aggregate;
    }

    public ProductCreated Create(int id, string name, decimal price)
    {
        var @event = new ProductCreated(id, name, price, DateTime.UtcNow);
        Apply(@event);
        return @event;
    }

    public ProductPriceChanged ChangePrice(decimal price)
    {
        if (Id == 0) throw new InvalidOperationException("Create the product first.");
        var @event = new ProductPriceChanged(Id, price, DateTime.UtcNow);
        Apply(@event);
        return @event;
    }

    private void Apply(ProductEvent @event)
    {
        switch (@event)
        {
            case ProductCreated e:
                Id = e.ProductId; Name = e.Name; Price = e.InitialPrice; break;
            case ProductPriceChanged e:
                Price = e.NewPrice; break;
        }
    }
}
