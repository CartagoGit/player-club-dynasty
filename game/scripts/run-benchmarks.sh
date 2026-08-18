#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
game_dir="$(cd -- "$script_dir/.." && pwd)"
artifact_dir="$game_dir/BenchmarkDotNet.Artifacts"
context_dir="$artifact_dir/context"

mkdir -p "$context_dir"
cp "$game_dir/benchmarks/budgets.json" "$context_dir/simulation-budgets.json"
cp "$game_dir/benchmarks/interaction-budgets.json" "$context_dir/interaction-budgets.json"
git -C "$game_dir" rev-parse HEAD > "$context_dir/commit.txt"
dotnet --info > "$context_dir/dotnet-info.txt"

# Without a filter BenchmarkDotNet asks which benchmarks to run. With no
# terminal it prints the menu, exits 0 and measures nothing, so the job looked
# green while producing no results at all. Select everything by default.
benchmark_arguments=(--join --filter "${BENCHMARK_FILTER:-*}")

cd -- "$game_dir"
dotnet run \
  --configuration Release \
  --project benchmarks/PCDynasty.Benchmarks \
  -- \
  "${benchmark_arguments[@]}" \
  "$@"
