using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

namespace Eventity.Benchmarks;

/// <summary>
/// Measures CPU time and memory usage for performance testing
/// </summary>
public class ResourceMeasurement
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Elapsed { get; set; }
    public TimeSpan ProcessorTime { get; set; }
    public long MemoryBefore { get; set; }
    public long MemoryAfter { get; set; }
    public long MemoryDelta { get; set; }
    public double MemoryMB => MemoryDelta / 1024.0 / 1024.0;
    public int ThreadCount { get; set; }
    public double CpuUsagePercent { get; set; }

    public override string ToString()
    {
        return $"{Name}:\n" +
               $"  Elapsed Time: {Elapsed.TotalMilliseconds:F2}ms\n" +
               $"  Processor Time: {ProcessorTime.TotalMilliseconds:F2}ms\n" +
               $"  Memory Delta: {MemoryMB:F2}MB\n" +
               $"  CPU Usage: {CpuUsagePercent:F2}%\n" +
               $"  Thread Count: {ThreadCount}";
    }
}

/// <summary>
/// Utility class for measuring resource consumption
/// </summary>
public class ResourceMonitor : IDisposable
{
    private readonly string _name;
    private readonly Process _process;
    private DateTime _startTime;
    private TimeSpan _startProcessorTime;
    private long _startMemory;
    private int _startThreadCount;

    public ResourceMonitor(string name)
    {
        _name = name;
        _process = Process.GetCurrentProcess();
    }

    public void Start()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        _startTime = DateTime.UtcNow;
        _startProcessorTime = _process.TotalProcessorTime;
        _startMemory = _process.WorkingSet64;
        _startThreadCount = _process.Threads.Count;
    }

    public ResourceMeasurement Stop()
    {
        var endTime = DateTime.UtcNow;
        var endProcessorTime = _process.TotalProcessorTime;
        var endMemory = _process.WorkingSet64;
        var endThreadCount = _process.Threads.Count;

        var measurement = new ResourceMeasurement
        {
            Name = _name,
            StartTime = _startTime,
            EndTime = endTime,
            Elapsed = endTime - _startTime,
            ProcessorTime = endProcessorTime - _startProcessorTime,
            MemoryBefore = _startMemory,
            MemoryAfter = endMemory,
            MemoryDelta = endMemory - _startMemory,
            ThreadCount = endThreadCount,
            CpuUsagePercent = Environment.ProcessorCount > 0 
                ? ((endProcessorTime - _startProcessorTime).TotalMilliseconds / (endTime - _startTime).TotalMilliseconds / Environment.ProcessorCount) * 100
                : 0
        };

        return measurement;
    }

    public void Dispose()
    {
        _process?.Dispose();
    }
}

/// <summary>
/// Repository for storing benchmark results
/// </summary>
public class BenchmarkResultRepository
{
    private readonly List<ResourceMeasurement> _measurements = new();

    public void Record(ResourceMeasurement measurement)
    {
        _measurements.Add(measurement);
    }

    public IReadOnlyList<ResourceMeasurement> GetResults() => _measurements.AsReadOnly();

    public void Clear()
    {
        _measurements.Clear();
    }

    public static string GenerateReport(IReadOnlyList<ResourceMeasurement> measurements)
    {
        if (measurements.Count == 0)
            return "No measurements recorded.";

        var report = "=== Resource Consumption Report ===\n\n";

        var totalElapsed = TimeSpan.Zero;
        var totalProcessorTime = TimeSpan.Zero;
        long totalMemory = 0;
        double totalCpuUsage = 0;

        foreach (var measurement in measurements)
        {
            report += $"{measurement}\n\n";
            totalElapsed += measurement.Elapsed;
            totalProcessorTime += measurement.ProcessorTime;
            totalMemory += measurement.MemoryDelta;
            totalCpuUsage += measurement.CpuUsagePercent;
        }

        report += "=== Summary ===\n";
        report += $"Total Tests: {measurements.Count}\n";
        report += $"Total Elapsed Time: {totalElapsed.TotalMilliseconds:F2}ms\n";
        report += $"Total Processor Time: {totalProcessorTime.TotalMilliseconds:F2}ms\n";
        report += $"Total Memory Delta: {(totalMemory / 1024.0 / 1024.0):F2}MB\n";
        report += $"Average CPU Usage: {(totalCpuUsage / measurements.Count):F2}%\n";

        return report;
    }
}
