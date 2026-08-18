# Player Club Dynasty — game workspace

The game is a Godot 4.7.1 .NET presentation over an engine-neutral C# domain.
Core, simulation, content contracts, persistence, Application services and
benchmarks run without starting Godot. Football is one replaceable sport pack;
external content and save files enter through public contracts.

## Prerequisites

- the .NET SDK selected by `global.json`;
- Godot 4.7.1 stable Mono for editor and presentation work;
- Bash for the repository entrypoint scripts.

Run commands from this `game` directory so SDK selection is enforced. Generated
`.godot`, `bin`, `obj`, test-result and benchmark-artifact directories are local
outputs and are ignored by Git. Godot `.uid` files beside authored scripts and
scenes are source metadata, so they remain versioned.

## Run the game

Use the launcher so Godot Mono receives the same .NET 8 runtime as the build:

```shell
./scripts/run-game.sh
```

If Godot or .NET are installed outside the standard locations, pass them
explicitly. This also avoids the `Failed to load .NET runtime / hostfxr` dialog
when a graphical process does not inherit the interactive shell environment:

```shell
DOTNET_ROOT=/path/to/dotnet GODOT_BIN=/path/to/godot ./scripts/run-game.sh
```

The launcher holds a per-user lock so repeated clicks do not open duplicate
game processes. Automated parallel smoke runs can opt out explicitly with
`PCDYNASTY_ALLOW_MULTIPLE=1`.

## Quality gates

Run every engine-neutral test, including integration and latency budgets:

```shell
./scripts/test-all.sh
```

Run only the fast forbidden-dependency gate:

```shell
./scripts/check-architecture.sh
```

Verify that no ignored build, editor, test, benchmark, local-data or save
artifact has been forced into the Git index:

```shell
./scripts/check-repository-hygiene.sh
```

The game CI runs this check before restore, build and tests.

Build the Godot presentation and run the headless SmokeBoot journey:

```shell
./scripts/run-godot-smoke.sh              # uses an installed Godot or GODOT_BIN
./scripts/run-godot-smoke.sh --provision  # downloads the pinned editor first
```

The pinned editor version lives in `game/godot.version`; the launcher, this
gate and CI all read it, so they cannot drift apart. Downloads land in the
ignored `.tools/` directory.

Compare benchmark results against the versioned budgets after a run:

```shell
bun scripts/compare-benchmark-budgets.script.ts
```

Every budget names the benchmark that measures it. A result without a budget,
or slower than the strictest budget claiming it, fails the comparison.

Both scripts accept extra `dotnet test` arguments. For example,
`./scripts/test-all.sh --no-restore` reuses an existing restore in CI.

Verify that the Godot adapter still loads after the engine-neutral gates:

```shell
godot --headless --editor --quit --path godot
```

## Benchmarks

Fast regression budgets execute with the normal test suite. Run the diagnostic
BenchmarkDotNet harness separately in Release mode:

```shell
dotnet run -c Release --project benchmarks/PCDynasty.Benchmarks -- --join
```

Reports are written under `BenchmarkDotNet.Artifacts` with runtime and hardware
metadata. Do not commit those reports. Budget changes require a comparable
before/after run and an architecture decision explaining any product impact.

## External data

The original starter pack is under `content/starter-world`. Runtime packs and
career saves remain external inputs; never compile mutable player databases or
save data into the domain assemblies. Keep local imported data and saves in the
ignored `data/` and `saves/` paths.
