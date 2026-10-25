using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace ICT.PaymentGateway.Application.Common.Messaging;

/// <summary>Finds the handler for a request and runs it through the registered pipeline behaviours.</summary>
internal sealed class Dispatcher(IServiceProvider serviceProvider) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, object> Invokers = new();

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invoker = (RequestInvoker<TResponse>)Invokers.GetOrAdd(
            request.GetType(),
            static requestType => Activator.CreateInstance(
                typeof(RequestInvoker<,>).MakeGenericType(requestType, typeof(TResponse)))!);

        return invoker.InvokeAsync(request, serviceProvider, cancellationToken);
    }

    private abstract class RequestInvoker<TResponse>
    {
        public abstract Task<TResponse> InvokeAsync(
            IRequest<TResponse> request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    private sealed class RequestInvoker<TRequest, TResponse> : RequestInvoker<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TResponse> InvokeAsync(
            IRequest<TResponse> request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var typedRequest = (TRequest)request;
            var handler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().ToArray();

            Func<Task<TResponse>> pipeline = () => handler.HandleAsync(typedRequest, cancellationToken);
            for (var i = behaviors.Length - 1; i >= 0; i--)
            {
                var behavior = behaviors[i];
                var next = pipeline;
                pipeline = () => behavior.HandleAsync(typedRequest, next, cancellationToken);
            }

            return pipeline();
        }
    }
}
