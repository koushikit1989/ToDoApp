using MediatR;
using ToDoManagementSystem.Application.DTOs.Dashboard;
using ToDoManagementSystem.Application.Features.Dashboard.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;

namespace ToDoManagementSystem.Application.Features.Dashboard.Handlers;

/// <summary>Builds project-wise task summaries for the dashboard.</summary>
public class GetProjectDashboardQueryHandler : IRequestHandler<GetProjectDashboardQuery, IEnumerable<ProjectTaskSummary>>
{
    private readonly ITaskRepository _taskRepository;

    public GetProjectDashboardQueryHandler(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    /// <summary>Groups the user's tasks by project and returns per-project stats.</summary>
    public async Task<IEnumerable<ProjectTaskSummary>> Handle(
        GetProjectDashboardQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<TaskItem> tasks = await _taskRepository.GetByUserIdWithProjectAsync(request.UserId, cancellationToken);

        List<ProjectTaskSummary> summaries = tasks
            .Where(t => t.ProjectId.HasValue && t.Project is not null)
            .GroupBy(t => t.ProjectId!.Value)
            .Select(g => new ProjectTaskSummary
            {
                ProjectId = g.Key,
                ProjectName = g.First().Project!.ProjectName,
                TotalTasks = g.Count(),
                CompletedTasks = g.Count(t => t.Status == DomainTaskStatus.Completed),
                PendingTasks = g.Count(t => t.Status == DomainTaskStatus.Pending),
                InProgressTasks = g.Count(t => t.Status == DomainTaskStatus.InProgress)
            })
            .ToList();

        return summaries;
    }
}
