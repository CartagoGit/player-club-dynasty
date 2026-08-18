#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
game_root="$(cd "${script_dir}/.." && pwd)"
repository_root="$(cd "${game_root}/.." && pwd)"

if [[ "${PCDYNASTY_ALLOW_MULTIPLE:-0}" != "1" ]]; then
    runtime_dir="${XDG_RUNTIME_DIR:-/tmp}"
    exec 9>"${runtime_dir}/pcdynasty-game.lock"
    if ! flock -n 9; then
        printf '%s\n' "PC Dynasty is already running." >&2
        exit 2
    fi
fi

find_dotnet() {
    if [[ -n "${DOTNET_ROOT:-}" && -x "${DOTNET_ROOT}/dotnet" ]]; then
        printf '%s\n' "${DOTNET_ROOT}/dotnet"
        return
    fi

    if command -v dotnet >/dev/null 2>&1; then
        command -v dotnet
        return
    fi

    for candidate in "${HOME}/.dotnet/dotnet" "${repository_root}/.tools/dotnet/dotnet"; do
        if [[ -x "${candidate}" ]]; then
            printf '%s\n' "${candidate}"
            return
        fi
    done

    return 1
}

find_godot() {
    if [[ -n "${GODOT_BIN:-}" && -x "${GODOT_BIN}" ]]; then
        printf '%s\n' "${GODOT_BIN}"
        return
    fi

    for command_name in godot godot4; do
        if command -v "${command_name}" >/dev/null 2>&1; then
            command -v "${command_name}"
            return
        fi
    done

    return 1
}

if ! dotnet_bin="$(find_dotnet)"; then
    printf '%s\n' "PC Dynasty requires the .NET 8 SDK. Install it or set DOTNET_ROOT." >&2
    exit 1
fi

resolved_dotnet="$(readlink -f "${dotnet_bin}")"
export DOTNET_ROOT="$(dirname "${resolved_dotnet}")"
export PATH="${DOTNET_ROOT}:${PATH}"

if ! "${resolved_dotnet}" --list-runtimes | grep -q '^Microsoft.NETCore.App 8\.'; then
    printf '%s\n' "PC Dynasty requires the .NET 8 runtime; ${DOTNET_ROOT} does not provide it." >&2
    exit 1
fi

if ! godot_bin="$(find_godot)"; then
    printf '%s\n' "Godot 4.7.1 Mono was not found. Install it or set GODOT_BIN." >&2
    exit 1
fi

exec "${godot_bin}" --path "${game_root}/godot" "$@"
