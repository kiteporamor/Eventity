using Eventity.Web.Converters;
using Microsoft.Extensions.DependencyInjection;

namespace Web;

public static class ConvertersExtension
{
    public static void AddDtoConverters(this IServiceCollection services)
    {
        services.AddScoped<UserDtoConverter>();
        services.AddScoped<EventDtoConverter>();
        services.AddScoped<ParticipationDtoConverter>();
        services.AddScoped<NotificationDtoConverter>();
        services.AddScoped<AuthDtoConverter>();
        services.AddScoped<ValidationDtoConverter>();
    }
}
