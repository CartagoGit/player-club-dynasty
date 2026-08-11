#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
game_dir="$(cd -- "$script_dir/.." && pwd)"

dotnet test \
  "$game_dir/tests/PCDynasty.Architecture.Tests/PCDynasty.Architecture.Tests.csproj" \
  --configuration Release \
  "$@"
