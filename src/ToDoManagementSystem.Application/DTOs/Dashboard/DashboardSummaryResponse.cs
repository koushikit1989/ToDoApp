namespace ToDoManagementSystem.Application.DTOs.Dashboard;


/// <summary>Aggregated task statistics for the dashboard.</summary>
public class DashboardSummaryResponse
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int OverdueTasks { get; set; }
    public double CompletionRate => TotalTasks == 0 ? 0 : Math.Round((double)CompletedTasks / TotalTasks * 100, 2);
    public IEnumerable<ProjectTaskSummary> ProjectTaskSummaries { get; set; } = Enumerable.Empty<ProjectTaskSummary>();
}
