using Microsoft.EntityFrameworkCore;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Domain.Entities;
using ToDoManagementSystem.Persistence.Context;

namespace ToDoManagementSystem.Persistence.Repositories;

/// <summary>Project-specific repository with name-uniqueness check and task-count queries.</summary>
public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    /// <summary>Returns all active (non-deleted) projects, ordered by name.</summary>
    public async Task<IEnumerable<Project>> GetAllActiveAsync(CancellationToken ct = default) =>
        await _dbSet.Where(p => p.IsActive)
                    .Include(p => p.Tasks)
                    .OrderBy(p => p.ProjectName)
                    .ToListAsync(ct);

    /// <summary>Returns all non-deleted projects created by a specific user with task counts.</summary>
    public async Task<IEnumerable<Project>> GetByCreatorAsync(Guid userId, CancellationToken ct = default) =>
        await _dbSet.Where(p => p.CreatedBy == userId)
                    .Include(p => p.Tasks)
                    .OrderBy(p => p.ProjectName)
                    .ToListAsync(ct);

    /// <summary>Returns true if another non-deleted project already has the given name.</summary>
    public async Task<bool> ProjectNameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
    {
        IQueryable<Project> query = _dbSet.Where(p => p.ProjectName == name);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync(ct);
    }

    /// <summary>Returns all non-deleted projects with their tasks loaded (for dashboard task-count mapping).</summary>
    public async Task<IEnumerable<Project>> GetProjectsWithTasksAsync(CancellationToken ct = default) =>
        await _dbSet.Include(p => p.Tasks)
                    .OrderBy(p => p.ProjectName)
                    .ToListAsync(ct);
}
