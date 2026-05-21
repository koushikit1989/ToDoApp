namespace ToDoManagementSystem.Application.Interfaces;

/// <summary>Transactional email service abstraction.</summary>
public interface IEmailService
{
    /// <summary>Sends a password-reset email with a reset link.</summary>
    Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink, CancellationToken ct = default);

    /// <summary>Sends a task-due-soon reminder email.</summary>
    Task SendReminderEmailAsync(string toEmail, string toName, string taskTitle, DateTime dueDate, CancellationToken ct = default);
}
