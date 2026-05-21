namespace ToDoManagementSystem.Shared.Responses;

/// <summary>Generic API response wrapper returned by all endpoints.</summary>
public class ApiResponse<T>
{
    /// <summary>Indicates whether the request succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>Human-readable status message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Response payload.</summary>
    public T? Data { get; set; }

    /// <summary>List of validation or error messages.</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>HTTP status code mirrored in the body.</summary>
    public int StatusCode { get; set; }

    /// <summary>UTC timestamp of the response.</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>Creates a successful 200 response.</summary>
    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Data = data, Message = message, StatusCode = 200 };

    /// <summary>Creates a successful 201 created response.</summary>
    public static ApiResponse<T> Created(T data, string message = "Created successfully") =>
        new() { Success = true, Data = data, Message = message, StatusCode = 201 };

    /// <summary>Creates a failure response with optional error list.</summary>
    public static ApiResponse<T> Fail(string message, int statusCode = 400, List<string>? errors = null) =>
        new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors ?? new() };
}
