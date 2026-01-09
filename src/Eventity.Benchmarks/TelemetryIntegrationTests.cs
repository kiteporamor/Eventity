using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Eventity.Benchmarks;

namespace Eventity.Benchmarks;

public class TelemetryIntegrationTests
{
    private readonly BenchmarkResultRepository _resultRepository = new();

    [Fact]
    public async Task CollectResourceMetricsForMinimalConfig()
    {
        await CollectMetricsForConfiguration("Minimal");
    }

    [Fact]
    public async Task CollectResourceMetricsForExtendedConfig()
    {
        await CollectMetricsForConfiguration("Extended");
    }

    [Fact]
    public async Task CollectResourceMetricsForTelemetryConfig()
    {
        await CollectMetricsForConfiguration("Telemetry");
    }

    private async Task CollectMetricsForConfiguration(string configName)
    {
        using var monitor = new ResourceMonitor($"Configuration: {configName}");
        monitor.Start();

        // Simulate test execution
        await Task.Delay(1000);

        var measurement = monitor.Stop();
        _resultRepository.Record(measurement);

        Assert.NotNull(measurement);
        Assert.Equal(configName, measurement.Name.Split(':')[1].Trim());
    }

    [Fact]
    public void GenerateComparisonReport()
    {
        var measurements = new List<ResourceMeasurement>
        {
            new ResourceMeasurement
            {
                Name = "Minimal Config",
                Elapsed = TimeSpan.FromMilliseconds(100),
                ProcessorTime = TimeSpan.FromMilliseconds(50),
                MemoryDelta = 1024 * 1024, // 1MB
                CpuUsagePercent = 10
            },
            new ResourceMeasurement
            {
                Name = "Extended Config",
                Elapsed = TimeSpan.FromMilliseconds(150),
                ProcessorTime = TimeSpan.FromMilliseconds(75),
                MemoryDelta = 2048 * 1024, // 2MB
                CpuUsagePercent = 15
            },
            new ResourceMeasurement
            {
                Name = "Telemetry Config",
                Elapsed = TimeSpan.FromMilliseconds(200),
                ProcessorTime = TimeSpan.FromMilliseconds(100),
                MemoryDelta = 5 * 1024 * 1024, // 5MB
                CpuUsagePercent = 20
            }
        };

        var report = BenchmarkResultRepository.GenerateReport(measurements);

        Assert.NotEmpty(report);
        Assert.Contains("Minimal Config", report);
        Assert.Contains("Extended Config", report);
        Assert.Contains("Telemetry Config", report);
        Assert.Contains("Summary", report);

        // Save report
        var reportPath = Path.Combine("reports", "telemetry_comparison_report.txt");
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? ".");
        File.WriteAllText(reportPath, report);

        Assert.True(File.Exists(reportPath));
    }

    [Fact]
    public void GenerateJsonReport()
    {
        var measurements = new List<ResourceMeasurement>
        {
            new ResourceMeasurement
            {
                Name = "Minimal Config",
                Elapsed = TimeSpan.FromMilliseconds(100),
                ProcessorTime = TimeSpan.FromMilliseconds(50),
                MemoryDelta = 1024 * 1024,
                CpuUsagePercent = 10
            },
            new ResourceMeasurement
            {
                Name = "Extended Config",
                Elapsed = TimeSpan.FromMilliseconds(150),
                ProcessorTime = TimeSpan.FromMilliseconds(75),
                MemoryDelta = 2048 * 1024,
                CpuUsagePercent = 15
            },
            new ResourceMeasurement
            {
                Name = "Telemetry Config",
                Elapsed = TimeSpan.FromMilliseconds(200),
                ProcessorTime = TimeSpan.FromMilliseconds(100),
                MemoryDelta = 5 * 1024 * 1024,
                CpuUsagePercent = 20
            }
        };

        var jsonReport = new
        {
            Timestamp = DateTime.UtcNow,
            TotalTests = measurements.Count,
            Measurements = measurements,
            Summary = new
            {
                TotalElapsedMs = measurements[0].Elapsed.TotalMilliseconds + 
                                 measurements[1].Elapsed.TotalMilliseconds + 
                                 measurements[2].Elapsed.TotalMilliseconds,
                AverageCpuUsage = (measurements[0].CpuUsagePercent + 
                                  measurements[1].CpuUsagePercent + 
                                  measurements[2].CpuUsagePercent) / measurements.Count
            }
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(jsonReport, options);

        var reportPath = Path.Combine("reports", "telemetry_comparison_report.json");
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? ".");
        File.WriteAllText(reportPath, json);

        Assert.True(File.Exists(reportPath));
    }
}
