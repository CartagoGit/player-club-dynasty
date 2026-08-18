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
