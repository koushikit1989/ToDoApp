using System.Net;
using System.Text.Json;
using Serilog;
using ToDoManagementSystem.Domain.Exceptions;
using ToDoManagementSystem.Shared.Responses;

namespace ToDoManagementSystem.API.Middleware;

/// <summary>Global exception handler middleware — maps domain exceptions to HTTP status codes.</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>Wraps the request pipeline in a try/catch and returns structured ApiResponse errors.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        string message;
        List<string> errors = new();

        switch (exception)
        {
            case NotFoundException notFound:
                statusCode = HttpStatusCode.NotFound;
                message = notFound.Message;
                break;

            case UnauthorizedException unauthorized:
                statusCode = HttpStatusCode.Unauthorized;
                message = unauthorized.Message;
                break;

            case ValidationException validation:
                statusCode = HttpStatusCode.BadRequest;
                message = "Validation failed.";
                errors = validation.Errors.ToList();
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "An unexpected error occurred.";
                Log.ForContext<ExceptionHandlingMiddleware>()
                   .Error(exception, "Unhandled exception: {Message}", exception.Message);
                break;
        }

        ApiResponse<object> response = ApiResponse<object>.Fail(message, (int)statusCode, errors);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        string json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
