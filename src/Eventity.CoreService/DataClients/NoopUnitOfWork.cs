using System.Threading.Tasks;
using Eventity.Domain.Interfaces;

namespace Eventity.CoreService.DataClients;

public class NoopUnitOfWork : IUnitOfWork
{
    public Task BeginTransactionAsync() => Task.CompletedTask;

    public Task CommitAsync() => Task.CompletedTask;

    public Task RollbackAsync() => Task.CompletedTask;

    public Task SaveChangesAsync() => Task.CompletedTask;

    public void Dispose()
    {
    }
}
