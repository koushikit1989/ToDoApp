using AutoMapper;
using MediatR;
using ToDoManagementSystem.Application.DTOs.Projects;
using ToDoManagementSystem.Application.Features.Projects.Queries;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;

namespace ToDoManagementSystem.Application.Features.Projects.Handlers;

/// <summary>Returns a single project by ID.</summary>
public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IMapper _mapper;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _mapper = mapper;
    }

    /// <summary>Finds the project or throws NotFoundException.</summary>
    public async Task<ProjectResponse> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        Project? project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null || project.IsDeleted)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        return _mapper.Map<ProjectResponse>(project);
    }
}
