using System.Globalization;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

namespace PCDynasty.Benchmarks;

public static class BenchmarkProgram
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher
            .FromAssembly(typeof(BenchmarkProgram).Assembly)
            .Run(args, BenchmarkConfig.Instance);
    }
}

public sealed class BenchmarkConfig : ManualConfig
{
    public const int ReproducibleSeed = 0x5EED_2026;

    private BenchmarkConfig()
    {
        string seed = ReproducibleSeed.ToString(CultureInfo.InvariantCulture);

        AddJob(Job.ShortRun
            .WithId("PCDynasty-net8")
            .WithEnvironmentVariable("PCDYNASTY_BENCHMARK_SEED", seed));
        AddDiagnoser(MemoryDiagnoser.Default);
        AddExporter(MarkdownExporter.GitHub, JsonExporter.Full);
        AddLogger(ConsoleLogger.Default);
        AddColumnProvider(DefaultColumnProviders.Instance);
        WithArtifactsPath(Path.Combine(
            Directory.GetCurrentDirectory(),
            "BenchmarkDotNet.Artifacts"));
    }

    public static BenchmarkConfig Instance { get; } = new();
}
