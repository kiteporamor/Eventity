using DataAccess;
using Eventity.DataAccess.Context;
using Eventity.DataAccess.Repositories;
using Eventity.Application.Services;
using Eventity.Domain.Interfaces;
using Eventity.Domain.Interfaces.Repositories;
using Eventity.Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Data.Common;
using Npgsql;

namespace Eventity.Tests.Integration;

public class IntegrationTestBase : IAsyncLifetime
{
    protected ServiceProvider ServiceProvider { get; private set; }
    protected EventityDbContext DbContext { get; private set; }
    private string _testDatabaseName;

    public async Task InitializeAsync()
    {
        _testDatabaseName = $"TestDb_{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10)}";
        
        await CreateTestDatabase();
        
        var services = new ServiceCollection();
        
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "PtmYHJnq6UhhjMUw510vZd546amBNgqWSDROkhOgkyQ",
                ["Jwt:Issuer"] = "EventityTest", 
                ["Jwt:Audience"] = "EventityUsers",
                ["Jwt:ExpireMinutes"] = "120"
            })
            .Build();
        
        services.AddSingleton<IConfiguration>(configuration);
        // bind JwtConfiguration for JwtService
        services.Configure<Eventity.Application.Services.JwtConfiguration>(opts =>
        {
            opts.Key = configuration["Jwt:Key"]!;
            opts.Issuer = configuration["Jwt:Issuer"]!;
            opts.Audience = configuration["Jwt:Audience"]!;
            opts.ExpireMinutes = int.Parse(configuration["Jwt:ExpireMinutes"]!);
        });
        
        var testDbHost = Environment.GetEnvironmentVariable("TEST_DB_HOST") ?? "test-db";
        var connectionString = $"User ID=postgres;Password=postgres;Host={testDbHost};Database={_testDatabaseName};Port=5432";
        
        services.AddDbContext<EventityDbContext>(options =>
            options.UseNpgsql(connectionString));
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IParticipationRepository, ParticipationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IParticipationService, ParticipationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUserService, UserService>();

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        
        services.AddLogging(builder => builder.AddConsole());
        
        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<EventityDbContext>();
        
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        ServiceProvider.Dispose();
        
        await DropTestDatabase();
    }

    protected T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    private async Task CreateTestDatabase()
    {
        var testDbHost = Environment.GetEnvironmentVariable("TEST_DB_HOST") ?? "test-db";
        var masterConnectionString = $"User ID=postgres;Password=postgres;Host={testDbHost};Database=EventityTest;Port=5432";
    
        var maxRetries = 5;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using var connection = new NpgsqlConnection(masterConnectionString);
                await connection.OpenAsync();
            
                var checkDbCommand = connection.CreateCommand();
                checkDbCommand.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{_testDatabaseName}'";
                var exists = await checkDbCommand.ExecuteScalarAsync() != null;
            
                if (!exists)
                {
                    var createDbCommand = connection.CreateCommand();
                    createDbCommand.CommandText = $"CREATE DATABASE \"{_testDatabaseName}\"";
                    await createDbCommand.ExecuteNonQueryAsync();
                }
                return;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                await Task.Delay(2000 * (i + 1));
            }
        }
        throw new Exception("Failed to create test database after multiple retries");
    }

    private async Task DropTestDatabase()
    {
        var masterConnectionString = "User ID=postgres;Password=postgres;Host=test-db;Database=EventityTest;Port=5432";
        
        using var connection = new NpgsqlConnection(masterConnectionString);
        await connection.OpenAsync();
        
        var terminateCommand = connection.CreateCommand();
        terminateCommand.CommandText = $@"
            SELECT pg_terminate_backend(pg_stat_activity.pid)
            FROM pg_stat_activity
            WHERE pg_stat_activity.datname = '{_testDatabaseName}' 
            AND pid <> pg_backend_pid()";
        await terminateCommand.ExecuteNonQueryAsync();
        
        var dropDbCommand = connection.CreateCommand();
        dropDbCommand.CommandText = $"DROP DATABASE IF EXISTS \"{_testDatabaseName}\"";
        await dropDbCommand.ExecuteNonQueryAsync();
    }
}