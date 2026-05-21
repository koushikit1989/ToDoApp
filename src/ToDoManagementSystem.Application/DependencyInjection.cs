using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ToDoManagementSystem.Application.Behaviors;

namespace ToDoManagementSystem.Application;

/// <summary>Extension methods registering all Application-layer services with the DI container.</summary>
public static class DependencyInjection
{
    /// <summary>Registers MediatR, AutoMapper, FluentValidation, and pipeline behaviors.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddAutoMapper(typeof(DependencyInjection).Assembly);

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
