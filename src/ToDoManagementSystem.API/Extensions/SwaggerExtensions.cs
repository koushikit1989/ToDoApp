using Microsoft.OpenApi.Models;

namespace ToDoManagementSystem.API.Extensions;

/// <summary>Swagger/OpenAPI configuration with JWT bearer authentication support.</summary>
public static class SwaggerExtensions
{
    /// <summary>Registers Swagger with JWT bearer auth button and metadata.</summary>
    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "To-Do Management System API",
                Version = "v1",
                Description = "Production-ready REST API for task management with JWT authentication.",
                Contact = new OpenApiContact { Name = "API Support", Email = "support@todo.local" }
            });

            OpenApiSecurityScheme jwtScheme = new()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token. Example: Bearer {token}"
            };

            options.AddSecurityDefinition("Bearer", jwtScheme);

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
