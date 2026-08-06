using Microsoft.EntityFrameworkCore.Storage;

using TierMatch.Application.Interfaces;

namespace TierMatch.Infrastructure.Data;

internal sealed class EfUnitOfWorkTransaction
    : IUnitOfWorkTransaction
{
    private readonly IDbContextTransaction
        _transaction;

    private bool _isCompleted;

    public EfUnitOfWorkTransaction(
        IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task CommitAsync(
        CancellationToken cancellationToken = default)
    {
        if (_isCompleted)
        {
            return;
        }

        await _transaction.CommitAsync(
            cancellationToken);

        _isCompleted = true;
    }

    public async Task RollbackAsync(
        CancellationToken cancellationToken = default)
    {
        if (_isCompleted)
        {
            return;
        }

        await _transaction.RollbackAsync(
            cancellationToken);

        _isCompleted = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_isCompleted)
        {
            await _transaction.RollbackAsync();
        }

        await _transaction.DisposeAsync();
    }
}