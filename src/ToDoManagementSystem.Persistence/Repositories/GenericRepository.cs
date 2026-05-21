using Microsoft.EntityFrameworkCore;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Persistence.Context;

namespace ToDoManagementSystem.Persistence.Repositories;

/// <summary>Generic EF Core repository implementing basic CRUD operations.</summary>
public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>Finds an entity by its GUID primary key.</summary>
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _dbSet.FindAsync(new object[] { id }, ct);

    /// <summary>Returns all entities (respects global query filters).</summary>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) =>
        await _dbSet.ToListAsync(ct);

    /// <summary>Adds an entity to the change tracker.</summary>
    public virtual async Task AddAsync(T entity, CancellationToken ct = default) =>
        await _dbSet.AddAsync(entity, ct);

    /// <summary>Marks an entity as modified.</summary>
    public virtual void Update(T entity) =>
        _dbSet.Update(entity);

    /// <summary>Marks an entity for deletion.</summary>
    public virtual void Delete(T entity) =>
        _dbSet.Remove(entity);
}
