namespace ToDoManagementSystem.Shared.Helpers;

/// <summary>UTC date/time helpers.</summary>
public static class DateTimeHelper
{
    /// <summary>Returns current UTC time.</summary>
    public static DateTime UtcNow() => DateTime.UtcNow;

    /// <summary>Checks if a date is in the past (before today UTC).</summary>
    public static bool IsPastDate(DateTime date) => date.Date < DateTime.UtcNow.Date;

    /// <summary>Formats a due date for display.</summary>
    public static string FormatDueDate(DateTime date) => date.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>Checks if a task is overdue based on its due date and completion status.</summary>
    public static bool IsOverdue(DateTime dueDate, bool isCompleted) =>
        !isCompleted && dueDate < DateTime.UtcNow;
}
