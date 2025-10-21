using Eventity.Domain.Interfaces;
using Eventity.Domain.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Eventity.Application.Services;

public static class ServicesExtension
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IParticipationService, ParticipationService>();
        services.AddScoped<IUserService, UserService>();
    }
}
