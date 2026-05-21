using MediatR;
using ToDoManagementSystem.Application.DTOs.Tasks;

namespace ToDoManagementSystem.Application.Features.Dashboard.Queries;

/// <summary>Query to retrieve all tasks for a user for export purposes.</summary>
public class ExportTasksQuery : IRequest<IEnumerable<TaskResponse>>
{
    public Guid UserId { get; set; }
}
