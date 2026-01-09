using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Web
{
    /// <summary>
    /// Extension for adding monitoring and diagnostic configuration
    /// Supports different logging levels for performance comparison
    /// </summary>
    public static class TelemetryExtension
    {
        public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
        {
            var telemetryConfig = configuration.GetSection("Telemetry");
            var isEnabled = telemetryConfig.GetValue<bool>("Enabled");

            if (!isEnabled)
            {
                return services;
            }

            // Configure structured logging for diagnostics
            services.Configure<LoggerFilterOptions>(opt =>
            {
                opt.MinLevel = LogLevel.Debug;
            });

            return services;
        }
    }
}
