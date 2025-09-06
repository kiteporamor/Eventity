using System.Threading.Tasks;
using Eventity.DataAccess.Context;
using Eventity.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccess;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly EventityDbContext _context;
    private IDbContextTransaction _transaction;

    public EfUnitOfWork(EventityDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        await _transaction.CommitAsync();
    }

    public async Task RollbackAsync()
    {
        await _transaction.RollbackAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
    }
}
