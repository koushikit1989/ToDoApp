namespace ToDoManagementSystem.Domain.Exceptions;

/// <summary>Thrown when FluentValidation finds one or more invalid inputs.</summary>
public class ValidationException : Exception
{
    /// <summary>All validation failure messages.</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>Initializes with a list of validation errors.</summary>
    public ValidationException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors.ToList().AsReadOnly();
    }

    /// <summary>Initializes with a single error message.</summary>
    public ValidationException(string message) : base(message)
    {
        Errors = new List<string> { message }.AsReadOnly();
    }
}
