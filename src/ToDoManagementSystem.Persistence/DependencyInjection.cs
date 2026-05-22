using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Persistence.Context;
using ToDoManagementSystem.Persistence.Repositories;

namespace ToDoManagementSystem.Persistence;

/// <summary>Extension methods registering all Persistence-layer services with the DI container.</summary>
public static class DependencyInjection
{
    /// <summary>Registers DbContext (SQL Server), repositories, and UnitOfWork.</summary>
    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
