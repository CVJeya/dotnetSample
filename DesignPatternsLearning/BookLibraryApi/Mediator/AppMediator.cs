namespace BookLibraryApi.Mediator;

// MEDIATOR: controllers send requests here instead of knowing concrete handlers.
// The built-in DI container supplies the matching handler.
public sealed class AppMediator(IServiceProvider services) : IAppMediator
{
    public Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>)
            .MakeGenericType(request.GetType(), typeof(TResponse));

        var handler = services.GetRequiredService(handlerType);
        var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.HandleAsync))!;
        return (Task<TResponse>)method.Invoke(handler, [request, cancellationToken])!;
    }
}
