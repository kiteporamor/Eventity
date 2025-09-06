using Eventity.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Eventity.Domain.Interfaces.Repositories;

namespace DataAccess;

public static class RepositoriesExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<INotificationRepository, NotificationRepository>();
        services.AddTransient<IParticipationRepository, ParticipationRepository>();
        services.AddTransient<IEventRepository, EventRepository>();
        return services;
    }
}
