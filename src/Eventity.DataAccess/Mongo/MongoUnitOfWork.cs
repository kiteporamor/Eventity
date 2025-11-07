using System;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces;
using MongoDB.Driver;

namespace DataAccess;

public class MongoUnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly IMongoDatabase _database;
    private IClientSessionHandle _session;
    private bool _disposed = false;

    public MongoUnitOfWork(IMongoDatabase database)
    {
        _database = database;
    }

    public async Task BeginTransactionAsync()
    {
        if (_session != null)
        {
            return;
        }
        
        _session = await _database.Client.StartSessionAsync();
        _session.StartTransaction();
    }

    public async Task CommitAsync()
    {
        try
        {
            if (_session != null && _session.IsInTransaction)
            {
                await _session.CommitTransactionAsync();
                await CleanupSessionAsync();
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
        if (_session != null && _session.IsInTransaction)
        {
            await _session.AbortTransactionAsync();
        }
        await CleanupSessionAsync();
    }

    public Task SaveChangesAsync()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (!_disposed)
        {
            await CleanupSessionAsync();
            _disposed = true;
        }
    }

    private async Task CleanupSessionAsync()
    {
        if (_session != null)
        {
            _session.Dispose();
            _session = null;
        }
    }
}