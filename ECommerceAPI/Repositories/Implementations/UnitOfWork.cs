using ECommerceAPI.Data;
using ECommerceAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerceAPI.Repositories.Implementations
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ECommerceDbContext _context;

        // Holds the currently active database transaction.
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ECommerceDbContext context)
        {
            _context = context;
        }

        // Saves all pending changes in the DbContext to the database.
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Starts a new database transaction if one is not already active.
        public async Task BeginTransactionAsync()
        {
            // Do not start another transaction if one is already active.
            if (_transaction != null)
            {
                return;
            }

            // Starts a new database transaction.
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        // Commits the current transaction and releases its resources.
        public async Task CommitTransactionAsync()
        {
            // Nothing to commit if there is no active transaction.
            if (_transaction == null)
            {
                return;
            }

            // Permanently saves all operations performed within the transaction.
            await _transaction.CommitAsync();

            // Releases the resources used by the transaction.
            await _transaction.DisposeAsync();

            // Clears the transaction reference because it is no longer active.
            _transaction = null;
        }

        // Rolls back the current transaction and releases its resources.
        public async Task RollbackTransactionAsync()
        {
            // Nothing to roll back if there is no active transaction.
            if (_transaction == null)
            {
                return;
            }

            // Cancels all database operations performed within the transaction.
            await _transaction.RollbackAsync();

            // Releases the resources used by the transaction.
            await _transaction.DisposeAsync();

            // Clears the transaction reference because it is no longer active.
            _transaction = null;
        }

        // Releases the active transaction when the Unit of Work is disposed.
        public async ValueTask DisposeAsync()
        {
            // Dispose the transaction only if one is still active.
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();

                _transaction = null;
            }
        }
    }
}
