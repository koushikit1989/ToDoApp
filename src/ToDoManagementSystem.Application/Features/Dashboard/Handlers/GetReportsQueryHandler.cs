using AutoMapper;
using MediatR;
using ToDoManagementSystem.Application.DTOs.Dashboard;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Dashboard.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Enums;

namespace ToDoManagementSystem.Application.Features.Dashboard.Handlers;

/// <summary>Generates a detailed report with summary stats, recent tasks, and priority breakdown.</summary>
public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, ReportResponse>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IMapper _mapper;

    public GetReportsQueryHandler(ITaskRepository taskRepository, IMapper mapper)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
    }

    /// <summary>Builds the full report response.</summary>
    public async Task<ReportResponse> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<TaskItem> tasks = await _taskRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        List<TaskItem> taskList = tasks.ToList();

        DashboardSummaryResponse summary = new()
        {
            TotalTasks = taskList.Count,
            CompletedTasks = taskList.Count(t => t.Status == DomainTaskStatus.Completed),
            PendingTasks = taskList.Count(t => t.Status == DomainTaskStatus.Pending),
            InProgressTasks = taskList.Count(t => t.Status == DomainTaskStatus.InProgress),
            OverdueTasks = taskList.Count(t => t.DueDate < DateTime.UtcNow && t.Status != DomainTaskStatus.Completed)
        };

        IEnumerable<TaskResponse> recentTasks = _mapper.Map<IEnumerable<TaskResponse>>(
            taskList.OrderByDescending(t => t.CreatedDate).Take(10));

        IEnumerable<TasksByPriorityResponse> byPriority = taskList
            .GroupBy(t => t.Priority)
            .Select(g => new TasksByPriorityResponse
            {
                Priority = g.Key.ToString(),
                Count = g.Count()
            });

        return new ReportResponse
        {
            Summary = summary,
            RecentTasks = recentTasks,
            TasksByPriority = byPriority,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
