using TierMatch.Application.Interfaces;

namespace TierMatch.Infrastructure.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<IUnitOfWorkTransaction>
        BeginTransactionAsync(
            CancellationToken cancellationToken = default)
    {
        var transaction =
            await _context.Database
                .BeginTransactionAsync(
                    cancellationToken);

        return new EfUnitOfWorkTransaction(
            transaction);
    }
}