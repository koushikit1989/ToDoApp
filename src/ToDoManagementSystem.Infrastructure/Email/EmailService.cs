using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Serilog;
using ToDoManagementSystem.Application.Interfaces;

namespace ToDoManagementSystem.Infrastructure.Email;

/// <summary>MailKit-based SMTP email service for transactional emails.</summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Sends a password reset email with a secure link.</summary>
    public async Task SendPasswordResetEmailAsync(
        string toEmail,
        string toName,
        string resetLink,
        CancellationToken ct = default)
    {
        string subject = "Reset Your Password — To-Do Management System";
        string body = $@"
            <h2>Password Reset Request</h2>
            <p>Hello {toName},</p>
            <p>Click the link below to reset your password. This link expires in 1 hour.</p>
            <p><a href='{resetLink}'>Reset Password</a></p>
            <p>If you did not request a password reset, please ignore this email.</p>";

        await SendEmailAsync(toEmail, toName, subject, body, ct);
    }

    /// <summary>Sends a task due-soon reminder email.</summary>
    public async Task SendReminderEmailAsync(
        string toEmail,
        string toName,
        string taskTitle,
        DateTime dueDate,
        CancellationToken ct = default)
    {
        string subject = $"Task Reminder: '{taskTitle}' is due soon";
        string body = $@"
            <h2>Task Reminder</h2>
            <p>Hello {toName},</p>
            <p>Your task <strong>{taskTitle}</strong> is due on <strong>{dueDate:yyyy-MM-dd HH:mm} UTC</strong>.</p>
            <p>Please complete it before the deadline.</p>";

        await SendEmailAsync(toEmail, toName, subject, body, ct);
    }

    private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody, CancellationToken ct)
    {
        try
        {
            MimeMessage message = new();
            message.From.Add(new MailboxAddress(
                _configuration["Email:SenderName"] ?? "To-Do System",
                _configuration["Email:SenderEmail"] ?? "noreply@todo.local"));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using SmtpClient client = new();
            await client.ConnectAsync(
                _configuration["Email:SmtpHost"] ?? "localhost",
                int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                SecureSocketOptions.StartTls,
                ct);

            string? smtpUser = _configuration["Email:SmtpUser"];
            string? smtpPass = _configuration["Email:SmtpPassword"];
            if (!string.IsNullOrEmpty(smtpUser))
                await client.AuthenticateAsync(smtpUser, smtpPass, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            Log.ForContext<EmailService>()
               .Error(ex, "Failed to send email to {Email}", toEmail);
        }
    }
}
