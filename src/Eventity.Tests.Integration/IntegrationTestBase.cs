using Eventity.DataAccess.Context;
using Eventity.DataAccess.Repositories;
using Eventity.Application.Services;
using Eventity.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Eventity.Tests.Integration;

public class IntegrationTestBase : IAsyncLifetime
{
    protected ServiceProvider ServiceProvider { get; private set; }
    protected EventityDbContext DbContext { get; private set; }

    public async Task InitializeAsync()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<EventityDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IParticipationRepository, ParticipationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        
        services.AddScoped<AuthService>();
        services.AddScoped<EventService>();
        services.AddScoped<ParticipationService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<UserService>();
        
        services.AddLogging(builder => builder.AddConsole());
        
        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<EventityDbContext>();
        
        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.DisposeAsync();
        ServiceProvider.Dispose();
    }

    protected T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }
}