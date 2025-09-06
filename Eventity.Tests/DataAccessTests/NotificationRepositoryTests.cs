using Eventity.DataAccess.Context;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Linq;
using Eventity.DataAccess.Converters;
using Microsoft.Extensions.Logging;

public class NotificationRepositoryTests
{
    private readonly EventityDbContext _context;
    private readonly NotificationRepository _repository;
    private readonly ILogger<NotificationRepository> _logger;

    public NotificationRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<EventityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new EventityDbContext(options);
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<NotificationRepository>();
        _logger = logger;
        _repository = new NotificationRepository(_context, _logger);
    }
    
    private DbContextOptions<EventityDbContext> CreateNewContextOptions()
    {
        return new DbContextOptionsBuilder<EventityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task AddAsync_ShouldSaveNotificationToDatabase()
    {
        var options = CreateNewContextOptions();
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            ParticipationId = Guid.NewGuid(),
            Text = "Test Notification",
            SentAt = DateTime.UtcNow
        };

        using (var context = new EventityDbContext(options))
        {
            var repository = new NotificationRepository(context, _logger);

            await repository.AddAsync(notification);
        }

        using (var context = new EventityDbContext(options))
        {
            var exists = await context.Notifications.AnyAsync(n => n.Text == "Test Notification");
            Assert.True(exists);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNotification_WhenExists()
    {
        var options = CreateNewContextOptions();
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            ParticipationId = Guid.NewGuid(),
            Text = "Test Notification",
            SentAt = DateTime.UtcNow
        };

        using (var context = new EventityDbContext(options))
        {
            context.Notifications.Add(notification.ToDb());
            await context.SaveChangesAsync();
        }

        using (var context = new EventityDbContext(options))
        {
            var repository = new NotificationRepository(context, _logger);

            var result = await repository.GetByIdAsync(notification.Id);

            Assert.NotNull(result);
            Assert.Equal(notification.Text, result.Text);
        }
    }

    [Fact]
    public async Task RemoveAsync_ShouldDeleteNotificationFromDatabase()
    {
        var options = CreateNewContextOptions();
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            ParticipationId = Guid.NewGuid(),
            Text = "Test Notification",
            SentAt = DateTime.UtcNow
        };

        using (var context = new EventityDbContext(options))
        {
            context.Notifications.Add(notification.ToDb());
            await context.SaveChangesAsync();
        }

        using (var context = new EventityDbContext(options))
        {
            var repository = new NotificationRepository(context, _logger);

            await repository.RemoveAsync(notification.Id);
        }

        using (var context = new EventityDbContext(options))
        {
            var exists = await context.Notifications.AnyAsync(n => n.Id == notification.Id);
            Assert.False(exists);
        }
    }
}
