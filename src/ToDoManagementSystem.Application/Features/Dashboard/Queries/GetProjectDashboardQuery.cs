using MediatR;
using ToDoManagementSystem.Application.DTOs.Dashboard;

namespace ToDoManagementSystem.Application.Features.Dashboard.Queries;

/// <summary>Query to retrieve project-wise task statistics for the dashboard.</summary>
public class GetProjectDashboardQuery : IRequest<IEnumerable<ProjectTaskSummary>>
{
    public Guid UserId { get; set; }
}
