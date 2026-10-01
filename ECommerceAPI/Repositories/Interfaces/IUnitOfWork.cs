namespace ECommerceAPI.Repositories.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        // Saves all pending changes in the DbContext to the database.
        Task<int> SaveChangesAsync();

        // Starts a new database transaction.
        Task BeginTransactionAsync();

        // Commits the current database transaction.
        Task CommitTransactionAsync();

        // Rolls back the current database transaction.
        Task RollbackTransactionAsync();
    }
}
