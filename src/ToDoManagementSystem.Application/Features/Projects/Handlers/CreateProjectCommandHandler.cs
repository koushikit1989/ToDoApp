using AutoMapper;
using MediatR;
using Serilog;
using ToDoManagementSystem.Application.DTOs.Projects;
using ToDoManagementSystem.Application.Features.Projects.Commands;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;

namespace ToDoManagementSystem.Application.Features.Projects.Handlers;

/// <summary>Creates a new project for the authenticated user.</summary>
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProjectCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>Validates uniqueness of project name, persists, and returns the DTO.</summary>
    public async Task<ProjectResponse> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        bool nameExists = await _projectRepository.ProjectNameExistsAsync(request.ProjectName, null, cancellationToken);
        if (nameExists)
            throw new ValidationException($"Project name '{request.ProjectName}' is already taken.");

        Project project = new()
        {
            Id = Guid.NewGuid(),
            ProjectName = request.ProjectName,
            ProjectCode = request.ProjectCode,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = true,
            IsDeleted = false,
            CreatedBy = request.CreatedBy,
            CreatedDate = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.ForContext<CreateProjectCommandHandler>()
           .Information("Project created: {ProjectId} '{ProjectName}' by user {UserId}",
               project.Id, project.ProjectName, request.CreatedBy);

        return _mapper.Map<ProjectResponse>(project);
    }
}
