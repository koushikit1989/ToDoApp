using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoManagementSystem.Application.DTOs.Tasks;
using ToDoManagementSystem.Application.Features.Tasks.Commands;
using ToDoManagementSystem.Application.Features.Tasks.Queries;
using ToDoManagementSystem.Shared.Responses;

namespace ToDoManagementSystem.API.Controllers;

/// <summary>Task management endpoints — all require JWT authentication.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns a paginated list of all tasks for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TaskResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        GetAllTasksQuery query = new()
        {
            UserId = GetCurrentUserId(),
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        PagedResponse<TaskResponse> result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<PagedResponse<TaskResponse>>.Ok(result));
    }

    /// <summary>Returns a single task by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        GetTaskByIdQuery query = new() { TaskId = id, UserId = GetCurrentUserId() };
        TaskResponse result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<TaskResponse>.Ok(result));
    }

    /// <summary>Returns a filtered, paginated list of tasks.</summary>
    [HttpGet("filter")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TaskResponse>>), 200)]
    public async Task<IActionResult> GetFiltered([FromQuery] TaskFilterRequest filter, CancellationToken ct)
    {
        GetFilteredTasksQuery query = new() { UserId = GetCurrentUserId(), Filter = filter };
        PagedResponse<TaskResponse> result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<PagedResponse<TaskResponse>>.Ok(result));
    }

    /// <summary>Searches tasks by title or description.</summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TaskResponse>>), 200)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        TaskFilterRequest filter = new() { SearchTerm = q, PageNumber = pageNumber, PageSize = pageSize };
        GetFilteredTasksQuery query = new() { UserId = GetCurrentUserId(), Filter = filter };
        PagedResponse<TaskResponse> result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<PagedResponse<TaskResponse>>.Ok(result));
    }

    /// <summary>Creates a new task for the current user.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        CreateTaskCommand command = new()
        {
            UserId = GetCurrentUserId(),
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            DueDate = request.DueDate,
            ProjectId = request.ProjectId
        };

        TaskResponse result = await _mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<TaskResponse>.Created(result, "Task created successfully."));
    }

    /// <summary>Updates an existing task.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request, CancellationToken ct)
    {
        UpdateTaskCommand command = new()
        {
            TaskId = id,
            UserId = GetCurrentUserId(),
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = request.Status,
            DueDate = request.DueDate,
            ProjectId = request.ProjectId
        };

        TaskResponse result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<TaskResponse>.Ok(result, "Task updated successfully."));
    }

    /// <summary>Soft-deletes a task by setting IsDeleted = true.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        DeleteTaskCommand command = new() { TaskId = id, UserId = GetCurrentUserId() };
        bool result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<bool>.Ok(result, "Task deleted successfully."));
    }

    /// <summary>Updates only the status of a task.</summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<TaskResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] int status, CancellationToken ct)
    {
        MarkTaskStatusCommand command = new() { TaskId = id, UserId = GetCurrentUserId(), Status = status };
        TaskResponse result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<TaskResponse>.Ok(result, "Task status updated."));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
