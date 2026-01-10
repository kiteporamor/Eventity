using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Web;

public static class OpenTelemetryExtensions
{
    private const string DefaultServiceName = "Eventity.Web";

    public static IServiceCollection AddOpenTelemetryIfEnabled(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var otelSection = configuration.GetSection("OpenTelemetry");
        var isEnabled = otelSection.GetValue<bool>("Enabled");
        if (!isEnabled)
        {
            return services;
        }

        var serviceName = otelSection.GetValue<string>("ServiceName") ?? DefaultServiceName;
        var serviceVersion = otelSection.GetValue<string>("ServiceVersion") ?? "1.0.0";
        var exporter = otelSection.GetValue<string>("Exporter") ?? "Console";
        var otlpEndpoint = otelSection.GetValue<string>("OtlpEndpoint");

        services.AddOpenTelemetry()
            .ConfigureResource(resource =>
                resource.AddService(serviceName, serviceVersion: serviceVersion))
            .WithTracing(tracing =>
            {
                tracing.SetSampler(new AlwaysOnSampler());
                tracing.AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                });
                tracing.AddHttpClientInstrumentation();

                AddTraceExporter(tracing, exporter, otlpEndpoint);
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();

                AddMetricExporter(metrics, exporter, otlpEndpoint);
            });

        return services;
    }

    private static void AddTraceExporter(
        TracerProviderBuilder builder,
        string exporter,
        string? otlpEndpoint)
    {
        if (exporter.Equals("Otlp", StringComparison.OrdinalIgnoreCase))
        {
            builder.AddOtlpExporter(options => ConfigureOtlp(options, otlpEndpoint));
            return;
        }

        if (exporter.Equals("Console", StringComparison.OrdinalIgnoreCase))
        {
            builder.AddConsoleExporter();
        }
    }

    private static void AddMetricExporter(
        MeterProviderBuilder builder,
        string exporter,
        string? otlpEndpoint)
    {
        if (exporter.Equals("Otlp", StringComparison.OrdinalIgnoreCase))
        {
            builder.AddOtlpExporter(options => ConfigureOtlp(options, otlpEndpoint));
            return;
        }

        if (exporter.Equals("Console", StringComparison.OrdinalIgnoreCase))
        {
            builder.AddConsoleExporter();
        }
    }

    private static void ConfigureOtlp(OtlpExporterOptions options, string? otlpEndpoint)
    {
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            options.Endpoint = new Uri(otlpEndpoint);
        }
    }
}
