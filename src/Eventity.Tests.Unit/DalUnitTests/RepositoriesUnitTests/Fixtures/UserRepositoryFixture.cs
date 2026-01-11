using Eventity.DataAccess.Context.Postgres;
using Eventity.DataAccess.Repositories.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Eventity.DataAccess.Tests.Repositories;

public class UserRepositoryFixture : IDisposable
{
    public ILogger<UserRepository> Logger { get; }

    public UserRepositoryFixture()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = loggerFactory.CreateLogger<UserRepository>();
    }

    public void Dispose()
    {
    }

    public async Task<EventityDbContext> CreateContextAsync()
    {
        var options = new DbContextOptionsBuilder<EventityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new EventityDbContext(options);
        await context.Database.EnsureCreatedAsync();
        return context;
    }
}