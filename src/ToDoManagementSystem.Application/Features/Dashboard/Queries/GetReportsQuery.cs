using MediatR;
using ToDoManagementSystem.Application.DTOs.Dashboard;

namespace ToDoManagementSystem.Application.Features.Dashboard.Queries;

/// <summary>Query to retrieve detailed report data including recent tasks and priority breakdown.</summary>
public class GetReportsQuery : IRequest<ReportResponse>
{
    public Guid UserId { get; set; }
}
