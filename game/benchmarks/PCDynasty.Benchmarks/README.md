# PCDynasty simulation benchmarks

This executable measures engine-neutral simulation code. It does not load Godot,
render scenes, use wall-clock time as game state, or depend on an installed game.

Every benchmark must derive generated fixtures from
`BenchmarkConfig.ReproducibleSeed`. BenchmarkDotNet records the runtime, operating
system, processor, JIT, GC mode, elapsed time, and allocation data in the console
summary and in `BenchmarkDotNet.Artifacts/results/`. Both Markdown and full JSON
reports are produced; generated artifacts are ignored by Git.

Run all benchmarks from the repository root:

```shell
PATH=/tmp/pcdynasty-dotnet:$PATH dotnet run -c Release \
  --project game/benchmarks/PCDynasty.Benchmarks -- --join
```

Run one benchmark family while iterating:

```shell
PATH=/tmp/pcdynasty-dotnet:$PATH dotnet run -c Release \
  --project game/benchmarks/PCDynasty.Benchmarks -- --filter '*Contest*'
```

Do not compare absolute timings from different machines. The committed budgets
are fast regression gates for a stable CI runner; BenchmarkDotNet reports are the
source for profiling and for intentionally recalibrating those budgets.
