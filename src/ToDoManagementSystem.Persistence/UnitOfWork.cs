using Microsoft.EntityFrameworkCore.Storage;
using ToDoManagementSystem.Application.Interfaces;
using ToDoManagementSystem.Persistence.Context;

namespace ToDoManagementSystem.Persistence;

/// <summary>EF Core unit-of-work wrapping SaveChanges and database transactions.</summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Persists all pending changes to the database.</summary>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await _context.SaveChangesAsync(ct);

    /// <summary>Begins a new database transaction.</summary>
    public async Task BeginTransactionAsync(CancellationToken ct = default) =>
        _transaction = await _context.Database.BeginTransactionAsync(ct);

    /// <summary>Commits the current transaction.</summary>
    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
            await _transaction.CommitAsync(ct);
    }

    /// <summary>Rolls back the current transaction.</summary>
    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
            await _transaction.RollbackAsync(ct);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
