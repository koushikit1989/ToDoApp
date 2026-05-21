namespace ToDoManagementSystem.Domain.Exceptions;

/// <summary>Thrown when a user attempts an action they are not authorized to perform.</summary>
public class UnauthorizedException : Exception
{
    /// <summary>Initializes with a default message.</summary>
    public UnauthorizedException() : base("You are not authorized to perform this action.") { }

    /// <summary>Initializes with a custom message.</summary>
    public UnauthorizedException(string message) : base(message) { }
}
