using BookLibraryApi.CQRS;
using BookLibraryApi.Events;
using BookLibraryApi.Mediator;
using BookLibraryApi.Models;
using BookLibraryApi.Repositories;
using BookLibraryApi.Specifications;
using BookLibraryApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookLibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(
    IAppMediator mediator,
    IProductRepository repository,
    IProductReadService productReadService,
    InMemoryEventStore eventStore) : ControllerBase
{
    // CQRS + Mediator: controller sends a command; it does not call a repository directly.
    [HttpPost]
    public async Task<ActionResult<Product>> Create(CreateProductCommand command, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(command.Name) || command.Price < 0)
            return BadRequest("Name is required and price cannot be negative.");

        var product = await mediator.SendAsync(command, token);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // CQRS query through the same mediator abstraction.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id, CancellationToken token)
    {
        var product = await mediator.SendAsync(new GetProductQuery(id), token);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet]
    public Task<IReadOnlyList<Product>> GetAll(CancellationToken token) =>
        mediator.SendAsync(new GetProductsQuery(), token);

    // SPECIFICATION: reusable rule object encapsulates the active-product filter.
    [HttpGet("active")]
    public async Task<IReadOnlyList<Product>> GetActive() =>
        await repository.FindAsync(new ActiveProductsSpecification());

    // PROXY + INTERCEPTOR/DECORATOR: the registered service is wrapped by both.
    [HttpGet("cached/{id:int}")]
    public async Task<ActionResult<Product>> GetCached(int id)
    {
        var product = await productReadService.GetAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    // EVENT SOURCING demo: appends events and rebuilds aggregate state from the event stream.
    [HttpPost("event-sourcing-demo")]
    public ActionResult<object> EventSourcingDemo()
    {
        var id = Random.Shared.Next(100000, 999999);
        var aggregate = new ProductAggregate();
        eventStore.Append(aggregate.Create(id, "Event-sourced Laptop", 50000m));
        eventStore.Append(aggregate.ChangePrice(55000m));

        var replayed = ProductAggregate.FromHistory(eventStore.Read(id));
        return Ok(new { CurrentState = new { replayed.Id, replayed.Name, replayed.Price },
            Events = eventStore.Read(id) });
    }
}
