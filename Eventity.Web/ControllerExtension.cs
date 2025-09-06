using Eventity.Web.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace Web;

public static class ControllerExtension
{
    public static void AddDtoConverters(this IServiceCollection services)
    {
        services.AddScoped<UserDtoConverter>();
        services.AddScoped<EventDtoConverter>();
        services.AddScoped<ParticipationDtoConverter>();
        services.AddScoped<NotificationDtoConverter>();
    }
}
