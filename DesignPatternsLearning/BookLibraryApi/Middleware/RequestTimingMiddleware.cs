namespace BookLibraryApi.Middleware;

// PIPELINE / MIDDLEWARE: each component can run before and after the next component.
public sealed class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var started = System.Diagnostics.Stopwatch.StartNew();
        logger.LogInformation("Middleware: {Method} {Path} started",
            context.Request.Method, context.Request.Path);

        await next(context); // Pass control to the next middleware / endpoint.

        started.Stop();
        logger.LogInformation("Middleware: request completed in {Elapsed} ms", started.ElapsedMilliseconds);
    }
}
