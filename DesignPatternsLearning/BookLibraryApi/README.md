# BookLibraryApi – Architectural Design Patterns

This API project is added beside the original console pattern-learning project.
It demonstrates patterns 21–30 using one Product example and the ASP.NET Core built-in DI container.

## Run

```bash
cd BookLibraryApi
dotnet run
```

The app prints its local HTTP/HTTPS addresses. Open the API endpoints below using Swagger-free tools such as curl, Postman, or the browser.

## Example requests

Create a product:

```http
POST /api/products
Content-Type: application/json

{"name":"Laptop","price":50000}
```

Read products:

```http
GET /api/products
GET /api/products/1
GET /api/products/active
GET /api/products/cached/1
```

Run the Event Sourcing demonstration:

```http
POST /api/products/event-sourcing-demo
```

## Pattern map

| Pattern | Where to look | Demonstration |
|---|---|---|
| Repository | `Repositories/` | Controller/handlers use an abstraction instead of storage details |
| Unit of Work | `UnitOfWork/` | Command handler commits after adding a product |
| CQRS | `CQRS/` | Separate create command and read queries |
| Mediator | `Mediator/` | Controller sends requests; mediator locates handlers |
| Specification | `Specifications/` | Reusable active-product filtering rule |
| Proxy | `Patterns/ProxyProductReadService.cs` | Caches product reads before calling the real service |
| DI Container | `Program.cs` | ASP.NET Core creates and injects registered dependencies |
| Middleware | `Middleware/RequestTimingMiddleware.cs` | Logs before and after the rest of the HTTP pipeline |
| Interceptor | `Patterns/LoggingProductReadService.cs` | Wrapper logs around service calls (decorator-style interception) |
| Event Sourcing | `Events/` | Stores events and rebuilds aggregate state by replay |

## Important learning notes

- This project uses an in-memory store so it runs without SQL Server or third-party NuGet packages. Data is lost when the API stops.
- The in-memory Unit of Work has a no-op commit. In an EF Core version, inject one `DbContext` into repositories and Unit of Work, then call `SaveChangesAsync()` at the commit boundary.
- The proxy cache is intentionally simple and does not expire or invalidate entries. Production caching needs expiration and invalidation.
- The custom mediator is a small teaching implementation. For production, consider a mature mediator library and pipeline behaviors.
- The logging wrapper is an explicit decorator/interceptor example. It is not CLR-wide automatic method interception.
- Event sourcing requires durable event storage, optimistic concurrency, schema/version strategy, and operational tooling in production.
- The first 20 GoF pattern demos remain in the original console project. They are not all automatically required in every Web API; apply each when it solves a real design problem.
