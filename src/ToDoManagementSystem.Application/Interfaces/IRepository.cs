namespace ToDoManagementSystem.Application.Interfaces;

/// <summary>Generic CRUD repository abstraction.</summary>
public interface IRepository<T> where T : class
{
    /// <summary>Gets an entity by its GUID primary key.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Gets all entities.</summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Adds a new entity to the change tracker.</summary>
    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>Marks an entity as modified in the change tracker.</summary>
    void Update(T entity);

    /// <summary>Marks an entity for deletion.</summary>
    void Delete(T entity);
}
