using ToDoManagementSystem.Domain.Entities;

namespace ToDoManagementSystem.Application.Interfaces;

/// <summary>Project-specific repository extending the generic CRUD interface.</summary>
public interface IProjectRepository : IRepository<Project>
{
    /// <summary>Returns all active (non-deleted) projects.</summary>
    Task<IEnumerable<Project>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>Returns all non-deleted projects created by a specific user, including task counts.</summary>
    Task<IEnumerable<Project>> GetByCreatorAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Checks whether a project name already exists, optionally excluding a specific project (for edit).</summary>
    Task<bool> ProjectNameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default);

    /// <summary>Returns projects with their task counts for the dashboard.</summary>
    Task<IEnumerable<Project>> GetProjectsWithTasksAsync(CancellationToken ct = default);
}
