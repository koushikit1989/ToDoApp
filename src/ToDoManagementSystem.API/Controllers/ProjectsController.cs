using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoManagementSystem.Application.DTOs.Projects;
using ToDoManagementSystem.Application.Features.Projects.Commands;
using ToDoManagementSystem.Application.Features.Projects.Queries;
using ToDoManagementSystem.Shared.Responses;

namespace ToDoManagementSystem.API.Controllers;

/// <summary>Project management endpoints — all require JWT authentication.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns all projects. Pass activeOnly=true to filter to active projects only.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProjectResponse>>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool activeOnly = false,
        CancellationToken ct = default)
    {
        GetAllProjectsQuery query = new() { ActiveOnly = activeOnly };
        IEnumerable<ProjectResponse> result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<IEnumerable<ProjectResponse>>.Ok(result));
    }

    /// <summary>Returns a single project by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        GetProjectByIdQuery query = new() { ProjectId = id };
        ProjectResponse result = await _mediator.Send(query, ct);
        return Ok(ApiResponse<ProjectResponse>.Ok(result));
    }

    /// <summary>Creates a new project.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        CreateProjectCommand command = new()
        {
            ProjectName = request.ProjectName,
            ProjectCode = request.ProjectCode,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedBy = GetCurrentUserId()
        };

        ProjectResponse result = await _mediator.Send(command, ct);
        return StatusCode(201, ApiResponse<ProjectResponse>.Created(result, "Project created successfully."));
    }

    /// <summary>Updates an existing project.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        UpdateProjectCommand command = new()
        {
            ProjectId = id,
            RequestedBy = GetCurrentUserId(),
            ProjectName = request.ProjectName,
            ProjectCode = request.ProjectCode,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive
        };

        ProjectResponse result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<ProjectResponse>.Ok(result, "Project updated successfully."));
    }

    /// <summary>Soft-deletes a project.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        DeleteProjectCommand command = new() { ProjectId = id, RequestedBy = GetCurrentUserId() };
        bool result = await _mediator.Send(command, ct);
        return Ok(ApiResponse<bool>.Ok(result, "Project deleted successfully."));
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
