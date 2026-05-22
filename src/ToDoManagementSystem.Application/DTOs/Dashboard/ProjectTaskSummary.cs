namespace ToDoManagementSystem.Application.DTOs.Dashboard;

/// <summary>Aggregated task statistics for a single project.</summary>
public class ProjectTaskSummary
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public double CompletionRate => TotalTasks == 0 ? 0 : Math.Round((double)CompletedTasks / TotalTasks * 100, 2);
}
