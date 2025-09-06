using System;
using System.Linq;
using System.Threading.Tasks;
using Eventity.DataAccess.Context;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

public class EventRepositoryTests : IDisposable
{
    private readonly EventityDbContext _context;
    private readonly EventRepository _repository;
    private readonly ILogger<EventRepository> _logger;

    public EventRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<EventityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EventityDbContext(options);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<EventRepository>();
        _logger = logger;
        _repository = new EventRepository(_context, _logger);
    }

    [Fact]
    public async Task AddAsync_ShouldSaveEvent()
    {
        var eventToAdd = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Test Event Title", 
            Address = "123 Main St",
            Description = "Test Event",
            DateTime = DateTime.UtcNow,
            OrganizerId = Guid.NewGuid(),
        };

        await _repository.AddAsync(eventToAdd);
        var retrievedEvent = await _repository.GetByIdAsync(eventToAdd.Id);

        Assert.NotNull(retrievedEvent);
        Assert.Equal(eventToAdd.Id, retrievedEvent.Id);
        Assert.Equal(eventToAdd.Description, retrievedEvent.Description);
        Assert.Equal(eventToAdd.Title, retrievedEvent.Title);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingEvent()
    {
        var eventToUpdate = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Test Event Title",
            Address = "Old Address",
            Description = "Old Description",
            DateTime = DateTime.UtcNow,
            OrganizerId = Guid.NewGuid(),
        };

        await _repository.AddAsync(eventToUpdate);

        eventToUpdate.Description = "Updated Description";
        eventToUpdate.Address = "Updated Address";
        await _repository.UpdateAsync(eventToUpdate);

        var updatedEvent = await _repository.GetByIdAsync(eventToUpdate.Id);

        Assert.Equal("Updated Description", updatedEvent.Description);
        Assert.Equal("Updated Address", updatedEvent.Address);
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteEvent()
    {
        var eventToDelete = new Event
        {
            Id = Guid.NewGuid(),
            Title = "Test Event Title",
            Address = "Some Address",
            Description = "To Be Deleted",
            DateTime = DateTime.UtcNow,
            OrganizerId = Guid.NewGuid(),
        };

        await _repository.AddAsync(eventToDelete);

        await _repository.RemoveAsync(eventToDelete.Id);

        var deletedEvent = _context.Events.FirstOrDefault(e => e.Id == eventToDelete.Id);
        Assert.Null(deletedEvent);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
