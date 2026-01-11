using Eventity.DataAccess.Context.Postgres;
using Eventity.DataAccess.Repositories.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Eventity.DataAccess.Tests.Repositories;

public class ParticipationRepositoryFixture : IDisposable
{
    public ILogger<ParticipationRepository> Logger { get; }

    public ParticipationRepositoryFixture()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        Logger = loggerFactory.CreateLogger<ParticipationRepository>();
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