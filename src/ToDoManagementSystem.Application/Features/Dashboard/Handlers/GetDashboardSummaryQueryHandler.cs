using MediatR;
using ToDoManagementSystem.Application.DTOs.Dashboard;
using ToDoManagementSystem.Application.Features.Dashboard.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;

namespace ToDoManagementSystem.Application.Features.Dashboard.Handlers;

/// <summary>Computes aggregated task counts for the dashboard summary.</summary>
public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryResponse>
{
    private readonly ITaskRepository _taskRepository;

    public GetDashboardSummaryQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    /// <summary>Fetches user tasks and computes count aggregations.</summary>
    public async Task<DashboardSummaryResponse> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<TaskItem> tasks = await _taskRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        List<TaskItem> taskList = tasks.ToList();

        return new DashboardSummaryResponse
        {
            TotalTasks = taskList.Count,
            CompletedTasks = taskList.Count(t => t.Status == DomainTaskStatus.Completed),
            PendingTasks = taskList.Count(t => t.Status == DomainTaskStatus.Pending),
            InProgressTasks = taskList.Count(t => t.Status == DomainTaskStatus.InProgress),
            OverdueTasks = taskList.Count(t => t.DueDate < DateTime.UtcNow && t.Status != DomainTaskStatus.Completed)
        };
    }
}
