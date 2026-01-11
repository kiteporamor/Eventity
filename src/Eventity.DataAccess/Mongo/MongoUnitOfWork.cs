using System;
using System.Threading.Tasks;
using Eventity.Domain.Interfaces;
using MongoDB.Driver;
using MongoDB.Driver.Core.Clusters;

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

        if (!TransactionsSupported())
        {
            return;
        }

        try
        {
            _session = await _database.Client.StartSessionAsync();
            _session.StartTransaction();
        }
        catch (MongoCommandException ex) when (ex.Message.Contains("do not support transactions"))
        {
            _session = null;
        }
        catch (NotSupportedException)
        {
            _session = null;
        }
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

    private bool TransactionsSupported()
    {
        var clusterType = _database.Client.Cluster.Description.Type;
        return clusterType == ClusterType.ReplicaSet || clusterType == ClusterType.Sharded;
    }
}
