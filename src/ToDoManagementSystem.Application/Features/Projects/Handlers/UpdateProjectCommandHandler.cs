using AutoMapper;
using MediatR;
using Serilog;
using ToDoManagementSystem.Application.DTOs.Projects;
using ToDoManagementSystem.Application.Features.Projects.Commands;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;

namespace ToDoManagementSystem.Application.Features.Projects.Handlers;

/// <summary>Applies partial updates to an existing project.</summary>
public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProjectCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>Finds the project, applies changes, saves, and returns the updated DTO.</summary>
    public async Task<ProjectResponse> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        Project? project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null || project.IsDeleted)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        if (request.ProjectName is not null && request.ProjectName != project.ProjectName)
        {
            bool nameExists = await _projectRepository.ProjectNameExistsAsync(
                request.ProjectName, request.ProjectId, cancellationToken);
            if (nameExists)
                throw new ValidationException($"Project name '{request.ProjectName}' is already taken.");
            project.ProjectName = request.ProjectName;
        }

        if (request.ProjectCode is not null) project.ProjectCode = request.ProjectCode;
        if (request.Description is not null) project.Description = request.Description;
        if (request.StartDate.HasValue) project.StartDate = request.StartDate;
        if (request.EndDate.HasValue) project.EndDate = request.EndDate;
        if (request.IsActive.HasValue) project.IsActive = request.IsActive.Value;
        project.UpdatedDate = DateTime.UtcNow;

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.ForContext<UpdateProjectCommandHandler>()
           .Information("Project updated: {ProjectId}", request.ProjectId);

        return _mapper.Map<ProjectResponse>(project);
    }
}
