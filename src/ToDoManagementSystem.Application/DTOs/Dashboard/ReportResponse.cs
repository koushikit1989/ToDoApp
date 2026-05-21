using ToDoManagementSystem.Application.DTOs.Tasks;

namespace ToDoManagementSystem.Application.DTOs.Dashboard;

/// <summary>Detailed report data combining summary stats with task breakdowns.</summary>
public class ReportResponse
{
    public DashboardSummaryResponse Summary { get; set; } = new();
    public IEnumerable<TaskResponse> RecentTasks { get; set; } = Enumerable.Empty<TaskResponse>();
    public IEnumerable<TasksByPriorityResponse> TasksByPriority { get; set; } = Enumerable.Empty<TasksByPriorityResponse>();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>Count of tasks grouped by priority.</summary>
public class TasksByPriorityResponse
{
    public string Priority { get; set; } = string.Empty;
    public int Count { get; set; }
}
