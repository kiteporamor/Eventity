using Eventity.DataAccess.Context;
using Eventity.DataAccess.Repositories;
using Eventity.Domain.Interfaces;
using Eventity.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess;

public static class DataBaseExtension
{
    public static IServiceCollection AddDataBase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DataBaseConnect")));
        services.AddRepositories();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        return services;
    }
}
