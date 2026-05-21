using Microsoft.EntityFrameworkCore;
using ToDoManagementSystem.Persistence.Context;

namespace ToDoManagementSystem.API.Extensions;

/// <summary>Extension methods for the application builder and database migration.</summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>Applies pending EF Core migrations automatically on startup.</summary>
    public static async Task MigrateDatabase(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
