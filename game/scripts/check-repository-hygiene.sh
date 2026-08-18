#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
repository_root="$(cd -- "$script_dir/../.." && pwd)"

declare -a violations=()
while IFS= read -r -d '' tracked_path; do
  case "$tracked_path" in
    .cache/* | */.cache/* | \
    .verify-tmp/* | */.verify-tmp/* | \
    node_modules/* | */node_modules/* | \
    dist/* | */dist/* | \
    build/* | */build/* | \
    coverage/* | */coverage/* | \
    [Bb]in/* | */[Bb]in/* | \
    [Oo]bj/* | */[Oo]bj/* | \
    TestResults/* | */TestResults/* | \
    BenchmarkDotNet.Artifacts/* | */BenchmarkDotNet.Artifacts/* | \
    .godot/* | */.godot/* | \
    .import/* | */.import/* | \
    .mono/* | */.mono/* | \
    data/* | game/data/* | \
    saves/* | game/saves/* | \
    *.pck | *.apk | *.aab | \
    *.sqlite-journal | *.sqlite-shm | *.sqlite-wal)
      violations+=("$tracked_path")
      ;;
  esac
done < <(git -C "$repository_root" ls-files -z)

if ((${#violations[@]} > 0)); then
  printf 'Generated or local-only artifacts are tracked:\n' >&2
  printf '  %s\n' "${violations[@]}" >&2
  exit 1
fi

if ! git -C "$repository_root" ls-files -- '*.uid' | grep -q .; then
  printf 'No authored Godot UID metadata is tracked.\n' >&2
  exit 1
fi

printf 'Repository hygiene passed: no generated artifacts are tracked.\n'
