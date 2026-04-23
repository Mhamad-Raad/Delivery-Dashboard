using DeliveryDash.Application.Abstracts;
using Microsoft.EntityFrameworkCore.Storage;

namespace DeliveryDash.Infrastructure.UnitOfWork
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly DeliveryDash.Infrastructure.ApplicationDbContext _context;

        public EfUnitOfWork(DeliveryDash.Infrastructure.ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ITransactionScope> BeginTransactionAsync(CancellationToken ct = default)
        {
            // If an outer transaction is already in progress on this DbContext, return a no-op scope
            // so nested calls participate in the outer transaction without reopening it.
            if (_context.Database.CurrentTransaction != null)
            {
                return NoOpTransactionScope.Instance;
            }

            var tx = await _context.Database.BeginTransactionAsync(ct);
            return new EfTransactionScope(tx);
        }
    }

    internal sealed class EfTransactionScope : ITransactionScope
    {
        private readonly IDbContextTransaction _tx;

        public EfTransactionScope(IDbContextTransaction tx)
        {
            _tx = tx;
        }

        public Task CommitAsync(CancellationToken ct = default) => _tx.CommitAsync(ct);
        public Task RollbackAsync(CancellationToken ct = default) => _tx.RollbackAsync(ct);
        public ValueTask DisposeAsync() => _tx.DisposeAsync();
    }

    internal sealed class NoOpTransactionScope : ITransactionScope
    {
        public static readonly NoOpTransactionScope Instance = new();
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
