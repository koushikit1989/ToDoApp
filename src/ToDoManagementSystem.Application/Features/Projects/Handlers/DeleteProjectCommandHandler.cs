using MediatR;
using Serilog;
using ToDoManagementSystem.Application.Features.Projects.Commands;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Domain.Exceptions;

namespace ToDoManagementSystem.Application.Features.Projects.Handlers;

/// <summary>Soft-deletes a project by setting IsDeleted = true.</summary>
public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, bool>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProjectCommandHandler(IProjectRepository projectRepository, IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>Marks the project as deleted without removing the row.</summary>
    public async Task<bool> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        Project? project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null || project.IsDeleted)
            throw new NotFoundException(nameof(Project), request.ProjectId);

        project.IsDeleted = true;
        project.IsActive = false;
        project.UpdatedDate = DateTime.UtcNow;

        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        Log.ForContext<DeleteProjectCommandHandler>()
           .Information("Project soft-deleted: {ProjectId}", request.ProjectId);

        return true;
    }
}
