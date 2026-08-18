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

benchmark_arguments=(--join)
if [[ -n "${BENCHMARK_FILTER:-}" ]]; then
  benchmark_arguments+=(--filter "$BENCHMARK_FILTER")
fi

cd -- "$game_dir"
dotnet run \
  --configuration Release \
  --project benchmarks/PCDynasty.Benchmarks \
  -- \
  "${benchmark_arguments[@]}" \
  "$@"
