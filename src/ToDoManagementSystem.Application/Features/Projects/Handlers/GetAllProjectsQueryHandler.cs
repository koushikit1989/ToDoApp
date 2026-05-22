using AutoMapper;
using MediatR;
using ToDoManagementSystem.Application.DTOs.Projects;
using ToDoManagementSystem.Application.Features.Projects.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Application.Features.Projects.Handlers;

/// <summary>Returns all projects (optionally filtered to active only).</summary>
public class GetAllProjectsQueryHandler : IRequestHandler<GetAllProjectsQuery, IEnumerable<ProjectResponse>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;

    public GetAllProjectsQueryHandler(IProjectRepository projectRepository, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    /// <summary>Fetches projects and maps to response DTOs.</summary>
    public async Task<IEnumerable<ProjectResponse>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<Project> projects = request.ActiveOnly
            ? await _projectRepository.GetAllActiveAsync(cancellationToken)
            : await _projectRepository.GetProjectsWithTasksAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ProjectResponse>>(projects);
    }
}
