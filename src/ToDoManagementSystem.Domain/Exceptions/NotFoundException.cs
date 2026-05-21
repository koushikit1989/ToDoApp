namespace ToDoManagementSystem.Domain.Exceptions;

/// <summary>Thrown when a requested resource does not exist.</summary>
public class NotFoundException : Exception
{
    /// <summary>Initializes a not-found exception for a named resource.</summary>
    public NotFoundException(string name, object key)
        : base($"Entity '{name}' with key '{key}' was not found.") { }

    /// <summary>Initializes a not-found exception with a custom message.</summary>
    public NotFoundException(string message) : base(message) { }
}
