using MediatR;
using ToDoManagementSystem.Application.DTOs.Dashboard;

namespace ToDoManagementSystem.Application.Features.Dashboard.Queries;

/// <summary>Query to retrieve aggregated task statistics for the dashboard.</summary>
public class GetDashboardSummaryQuery : IRequest<DashboardSummaryResponse>
{
    public Guid UserId { get; set; }
}
