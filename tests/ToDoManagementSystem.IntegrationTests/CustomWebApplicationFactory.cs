using Hangfire;
using Hangfire.InMemory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ToDoManagementSystem.Persistence.Context;

namespace ToDoManagementSystem.IntegrationTests;

/// <summary>
/// WebApplicationFactory that replaces SQL Server with InMemory providers
/// so integration tests run without a live database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "IntegrationTestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace AppDbContext (SQL Server) with InMemory — capture _dbName outside lambda
            // so all requests within one factory share the same in-memory database.
            ServiceDescriptor? dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            ServiceDescriptor? dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(AppDbContext));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            string dbName = _dbName;
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));

            // Replace Hangfire SQL Server storage with InMemory storage
            services.AddHangfire(config =>
                config.UseInMemoryStorage());

            // Remove the SQL Server health check so tests don't need a live DB
            ServiceDescriptor? healthCheckDescriptor = services.FirstOrDefault(
                d => d.ImplementationType?.Name?.Contains("SqlServer") == true ||
                     d.ImplementationFactory?.Method?.ReturnType?.Name?.Contains("SqlServer") == true);
            if (healthCheckDescriptor != null)
                services.Remove(healthCheckDescriptor);
        });
    }
}
