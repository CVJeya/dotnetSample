using BookLibraryApi.CQRS;
using BookLibraryApi.Data;
using BookLibraryApi.Events;
using BookLibraryApi.Mediator;
using BookLibraryApi.Middleware;
using BookLibraryApi.Models;
using BookLibraryApi.Patterns;
using BookLibraryApi.Repositories;
using BookLibraryApi.Services;
using BookLibraryApi.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// DEPENDENCY INJECTION CONTAINER: ASP.NET Core's built-in container creates dependencies.
// Singleton: one shared instance for the application lifetime (safe for this demo store).
builder.Services.AddSingleton<InMemoryDatabase>();
builder.Services.AddSingleton<InMemoryEventStore>();

// Scoped: one instance per HTTP request. Transient would create a new instance per resolution.
builder.Services.AddScoped<IProductRepository, InMemoryProductRepository>();
builder.Services.AddScoped<IUnitOfWork, InMemoryUnitOfWork>();
builder.Services.AddScoped<IAppMediator, AppMediator>();

// Register CQRS handlers. The mediator resolves the matching handler from DI.
builder.Services.AddScoped<IRequestHandler<CreateProductCommand, Product>, CreateProductHandler>();
builder.Services.AddScoped<IRequestHandler<GetProductQuery, Product?>, GetProductHandler>();
builder.Services.AddScoped<IRequestHandler<GetProductsQuery, IReadOnlyList<Product>>, GetProductsHandler>();

// PROXY + INTERCEPTOR/DECORATOR composition:
// real service -> caching proxy -> logging wrapper -> controller.
builder.Services.AddScoped<ProductReadService>();
builder.Services.AddScoped<IProductReadService>(sp =>
{
    IProductReadService real = sp.GetRequiredService<ProductReadService>();
    IProductReadService proxy = new ProxyProductReadService(real);
    return new LoggingProductReadService(proxy, sp.GetRequiredService<ILogger<LoggingProductReadService>>());
});

builder.Services.AddControllers();

var app = builder.Build();

// Middleware is ordered: this logger wraps the remaining request pipeline.
app.UseMiddleware<RequestTimingMiddleware>();
app.MapControllers();

app.Run();
