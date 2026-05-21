using System.Diagnostics;
using MediatR;
using Serilog;

namespace ToDoManagementSystem.Application.Behaviors;

/// <summary>MediatR pipeline behavior that logs command/query names and execution durations.</summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>Wraps handler execution with structured Serilog timing logs.</summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        Stopwatch stopwatch = Stopwatch.StartNew();

        Log.ForContext<LoggingBehavior<TRequest, TResponse>>()
           .Information("Handling {RequestName}", requestName);

        TResponse response = await next();

        stopwatch.Stop();

        Log.ForContext<LoggingBehavior<TRequest, TResponse>>()
           .Information("Handled {RequestName} in {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);

        return response;
    }
}
