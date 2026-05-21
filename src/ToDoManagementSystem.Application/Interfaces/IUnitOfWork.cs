namespace ToDoManagementSystem.Application.Interfaces;

/// <summary>Unit-of-work abstraction wrapping EF Core transactions.</summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Begins a database transaction.</summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>Commits the current transaction.</summary>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>Rolls back the current transaction.</summary>
    Task RollbackAsync(CancellationToken ct = default);
}
