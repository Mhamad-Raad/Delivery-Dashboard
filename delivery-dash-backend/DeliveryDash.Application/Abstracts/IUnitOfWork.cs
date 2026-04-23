namespace DeliveryDash.Application.Abstracts
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Starts a DB transaction, or joins an already-active one (returning a no-op scope) so nested calls are safe.
        /// </summary>
        Task<ITransactionScope> BeginTransactionAsync(CancellationToken ct = default);
    }

    public interface ITransactionScope : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken ct = default);
        Task RollbackAsync(CancellationToken ct = default);
    }
}
