using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Infrastructure.BackgroundJobs;

/// <summary>Hangfire recurring jobs for task reminders and overdue detection.</summary>
public class ReminderJobService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReminderJobService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    /// <summary>Runs every hour: finds tasks due within 24 hours and emails users.</summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task SendUpcomingDueRemindersAsync()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ITaskRepository taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();
        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        IEnumerable<TaskItem> upcomingTasks = await taskRepository.GetUpcomingDueTasksAsync();

        foreach (TaskItem task in upcomingTasks)
        {
            if (task.User is not null)
            {
                await emailService.SendReminderEmailAsync(
                    task.User.Email,
                    task.User.FullName,
                    task.Title,
                    task.DueDate);
            }
        }

        Log.ForContext<ReminderJobService>()
           .Information("Reminder job processed {Count} upcoming tasks", upcomingTasks.Count());
    }

    /// <summary>Runs daily at midnight: logs overdue tasks for monitoring.</summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task LogOverdueTasksAsync()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ITaskRepository taskRepository = scope.ServiceProvider.GetRequiredService<ITaskRepository>();

        IEnumerable<TaskItem> overdueTasks = await taskRepository.GetOverdueTasksAsync();

        Log.ForContext<ReminderJobService>()
           .Warning("Overdue tasks count: {Count}", overdueTasks.Count());
    }

    /// <summary>Registers recurring jobs with Hangfire.</summary>
    public static void RegisterRecurringJobs(IRecurringJobManager recurringJobManager, IServiceScopeFactory scopeFactory)
    {
        recurringJobManager.AddOrUpdate<ReminderJobService>(
            "upcoming-due-reminders",
            job => job.SendUpcomingDueRemindersAsync(),
            "0 * * * *");

        recurringJobManager.AddOrUpdate<ReminderJobService>(
            "log-overdue-tasks",
            job => job.LogOverdueTasksAsync(),
            Cron.Daily);
    }
}
