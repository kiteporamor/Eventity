using Eventity.DataAccess.Context.Postgres;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Interfaces;
using Eventity.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DataAccess;

public class DatabaseConfiguration
{
    public const string ConnectionSettingsString = "ConnectionStrings";
    public string DatabaseProvider { get; set; } = "PostgreSQL";
    public string PostgreSQL { get; set; }
    public string MongoDB { get; set; }
    public string DatabaseName { get; set; } = "Eventity";
}

public static class DataBaseExtension
{
    public static IServiceCollection AddDataBase(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseConfiguration>(configuration);
        var serviceProvider = services.BuildServiceProvider();
        var dbOptions = serviceProvider.GetRequiredService<IOptions<DatabaseConfiguration>>();
        var dbConfig = dbOptions.Value;

        switch (dbConfig.DatabaseProvider.ToLower())
        {
            case "mongodb":
                services.AddMongoDb(dbConfig);
                break;
            case "postgresql":
            default:
                services.AddPostgreSql(dbConfig);
                break;
        }
        
        services.AddRepositories(dbOptions);
        return services;
    }

    private static IServiceCollection AddPostgreSql(this IServiceCollection services, DatabaseConfiguration dbConfig)
    {
        services.AddDbContext<EventityDbContext>(options =>
            options.UseNpgsql(dbConfig.PostgreSQL));
        
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        
        return services;
    }

    private static IServiceCollection AddMongoDb(this IServiceCollection services, DatabaseConfiguration dbConfig)
    {
        services.AddSingleton<IMongoClient>(sp => new MongoClient(dbConfig.MongoDB));
        
        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(dbConfig.DatabaseName);
        });
        
        services.AddScoped<IUnitOfWork, MongoUnitOfWork>();
        
        return services;
    }
}