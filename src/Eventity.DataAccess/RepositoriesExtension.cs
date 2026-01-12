using Castle.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Eventity.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Options;
using Postgres = Eventity.DataAccess.Repositories.Postgres;
using Mongo = Eventity.DataAccess.Repositories.Mongo;

namespace DataAccess;

public static class RepositoriesExtension
{
    public static IServiceCollection AddRepositories(this IServiceCollection services,
        IOptions<DatabaseConfiguration> dbOptions)
    {
        var dbConfig = dbOptions.Value;

        switch (dbConfig.DatabaseProvider?.ToLower())
        {
            case "mongodb":
                services.AddTransient<IUserRepository, Mongo.UserRepository>();
                services.AddTransient<INotificationRepository, Mongo.NotificationRepository>();
                services.AddTransient<IParticipationRepository, Mongo.ParticipationRepository>();
                services.AddTransient<IEventRepository, Mongo.EventRepository>();
                break;
            case "postgres":
            default:
                services.AddTransient<IUserRepository, Postgres.UserRepository>();
                services.AddTransient<INotificationRepository, Postgres.NotificationRepository>();
                services.AddTransient<IParticipationRepository, Postgres.ParticipationRepository>();
                services.AddTransient<IEventRepository, Postgres.EventRepository>();
                break;
        }

        return services;
    }
}
