using BenchmarkDotNet.Running;
using Eventity.Benchmarks;

namespace Eventity.Benchmarks.Runner;

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<TelemetryBenchmark>();
    }
}
