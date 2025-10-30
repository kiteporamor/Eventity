using System.Threading.Tasks;
using Eventity.DataAccess.Context;
using Eventity.Domain.Interfaces;
using Eventity.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;


namespace DataAccess;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly EventityDbContext _context;
    private IDbContextTransaction _transaction;
    private bool _disposed = false;

    public EfUnitOfWork(EventityDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            return;
        }
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
        catch (Exception)
        {
            await RollbackAsync();
            throw;
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
        _disposed = true;
    }
}