using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Eventity.Domain.Enums;
using Eventity.Application.Services;
using Eventity.Domain.Interfaces.Services;
using Eventity.DataAccess.Context;
using Eventity.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace Eventity.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, targetCount: 5)]
public class TelemetryBenchmark
{
    private IServiceProvider? _serviceProvider;
    private IAuthService? _authService;
    private IUserService? _userService;
    private IEventService? _eventService;
    private EventityDbContext? _dbContext;
    private LoggingLevelSwitch? _loggingLevelSwitch;
    private string _testUserLogin = string.Empty;
    private string _testUserPassword = "TestPassword123!";

    [Params("Minimal", "Extended", "Telemetry")]
    public string? ConfigurationProfile { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var configFileName = ConfigurationProfile switch
        {
            "Extended" => "appsettings.extended.json",
            "Telemetry" => "appsettings.telemetry.json",
            _ => "appsettings.minimal.json"
        };

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(configFileName, optional: true)
            .AddEnvironmentVariables()
            .Build();

        _loggingLevelSwitch = new LoggingLevelSwitch();
        var logLevel = ConfigurationProfile switch
        {
            "Extended" => Serilog.Events.LogEventLevel.Debug,
            "Telemetry" => Serilog.Events.LogEventLevel.Debug,
            _ => Serilog.Events.LogEventLevel.Information
        };

        _loggingLevelSwitch.MinimumLevel = logLevel;

        var services = new ServiceCollection();

        // Configure logging
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(new LoggerConfiguration()
                .MinimumLevel.ControlledBy(_loggingLevelSwitch)
                .WriteTo.Console()
                .WriteTo.File("logs/benchmark-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger());
        });

        // Configure database
        var connectionString = config.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Port=5432;Database=eventity_benchmark;Username=postgres;Password=postgres";

        services.AddDbContext<EventityDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly("Eventity.DataAccess")));

        // Add repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IParticipationRepository, ParticipationRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IParticipationService, ParticipationService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IJwtService, JwtService>();

        _serviceProvider = services.BuildServiceProvider();
        _authService = _serviceProvider.GetRequiredService<IAuthService>();
        _userService = _serviceProvider.GetRequiredService<IUserService>();
        _eventService = _serviceProvider.GetRequiredService<IEventService>();
        _dbContext = _serviceProvider.GetRequiredService<EventityDbContext>();

        // Setup database
        _dbContext.Database.EnsureCreated();

        _testUserLogin = $"benchmarkuser-{Guid.NewGuid()}";

        // Cleanup
        _dbContext.Users.RemoveRange(_dbContext.Users);
        _dbContext.SaveChanges();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _dbContext?.Dispose();
        (_serviceProvider as ServiceProvider)?.Dispose();
    }

    [Benchmark]
    public async Task UserRegistration()
    {
        var uniqueLogin = $"{_testUserLogin}-{Guid.NewGuid()}";
        await _authService!.RegisterUser(
            "Benchmark User",
            $"{uniqueLogin}@test.com",
            uniqueLogin,
            _testUserPassword,
            UserRoleEnum.User);
    }

    [Benchmark]
    public async Task UserAuthentication()
    {
        var uniqueLogin = $"{_testUserLogin}-{Guid.NewGuid()}";
        await _authService!.RegisterUser(
            "Benchmark User",
            $"{uniqueLogin}@test.com",
            uniqueLogin,
            _testUserPassword,
            UserRoleEnum.User);

        await _authService.AuthenticateUser(uniqueLogin, _testUserPassword);
    }

    [Benchmark]
    public async Task EventCreation()
    {
        var uniqueLogin = $"{_testUserLogin}-{Guid.NewGuid()}";
        var authResult = await _authService!.RegisterUser(
            "Benchmark User",
            $"{uniqueLogin}@test.com",
            uniqueLogin,
            _testUserPassword,
            UserRoleEnum.Admin);

        var validation = new Validation(authResult.User.Id, true);

        await _eventService!.AddEvent(
            "Benchmark Event",
            "Test event for benchmarking",
            DateTime.UtcNow.AddDays(7),
            "123 Test Street",
            validation);
    }

    [Benchmark]
    public async Task ComplexScenario()
    {
        // Register multiple users
        var users = new List<(string login, string email)>();
        for (int i = 0; i < 5; i++)
        {
            var uniqueLogin = $"{_testUserLogin}-scenario-{i}-{Guid.NewGuid()}";
            users.Add((uniqueLogin, $"{uniqueLogin}@test.com"));
        }

        var registeredUsers = new List<(string id, string login)>();
        foreach (var (login, email) in users)
        {
            var result = await _authService!.RegisterUser(
                "Benchmark User",
                email,
                login,
                _testUserPassword,
                i == 0 ? UserRoleEnum.Admin : UserRoleEnum.User);

            registeredUsers.Add((result.User.Id.ToString(), login));
        }

        // Create event
        var adminId = Guid.Parse(registeredUsers[0].id);
        var validation = new Validation(adminId, true);

        var eventResult = await _eventService!.AddEvent(
            "Benchmark Event",
            "Test event for benchmarking",
            DateTime.UtcNow.AddDays(7),
            "123 Test Street",
            validation);

        // Add participants
        foreach (var (id, _) in registeredUsers.Skip(1))
        {
            var userId = Guid.Parse(id);
            await _eventService.AddParticipation(eventResult.Id, userId, ParticipationStatusEnum.Accepted);
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<TelemetryBenchmark>();
    }
}

// Helper class for validation
public class Validation
{
    public Guid CurrentUserId { get; }
    public bool IsAdmin { get; }

    public Validation(Guid currentUserId, bool isAdmin)
    {
        CurrentUserId = currentUserId;
        IsAdmin = isAdmin;
    }
}
