#!/usr/bin/env bash
# Build the Godot presentation and run the SmokeBoot scene headlessly.
#
# Audit a00001 found that a slice could close as "visible in Godot" while CI
# never opened Godot at all: the executable path was only ever exercised by
# hand. This script is the gate that closes that hole, locally and in CI.
#
#   ./scripts/run-godot-smoke.sh              # use an installed Godot
#   ./scripts/run-godot-smoke.sh --provision  # download the pinned editor first
#
# GODOT_BIN overrides discovery. PCDYNASTY_TOOLS_DIR overrides the download
# cache. The pinned version lives in game/godot.version, so CI, the launcher
# and this gate can never drift apart.
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
game_dir="$(cd -- "${script_dir}/.." && pwd)"
repository_root="$(cd -- "${game_dir}/.." && pwd)"
project_dir="${game_dir}/godot"
version="$(tr -d '[:space:]' < "${game_dir}/godot.version")"
tools_dir="${PCDYNASTY_TOOLS_DIR:-${repository_root}/.tools}"
install_dir="${tools_dir}/godot/${version}"
provision=0

for argument in "$@"; do
    case "${argument}" in
        --provision) provision=1 ;;
        *) printf 'run-godot-smoke: unknown argument "%s".\n' "${argument}" >&2; exit 2 ;;
    esac
done

if [[ "${PCDYNASTY_PROVISION_GODOT:-0}" == "1" ]]; then
    provision=1
fi

platform_archive() {
    case "$(uname -s)" in
        Linux) printf 'Godot_v%s_mono_linux_x86_64' "${version}" ;;
        Darwin) printf 'Godot_v%s_mono_macos.universal' "${version}" ;;
        *) return 1 ;;
    esac
}

platform_binary() {
    case "$(uname -s)" in
        Linux) printf '%s/Godot_v%s_mono_linux.x86_64' "$(platform_archive)" "${version}" ;;
        Darwin) printf '%s/Godot.app/Contents/MacOS/Godot' "$(platform_archive)" ;;
        *) return 1 ;;
    esac
}

provision_godot() {
    local archive binary url
    archive="$(platform_archive)" || {
        printf 'run-godot-smoke: no pinned Godot build for %s.\n' "$(uname -s)" >&2
        return 1
    }
    binary="${install_dir}/$(platform_binary)"
    if [[ -x "${binary}" ]]; then
        printf '%s\n' "${binary}"
        return 0
    fi

    url="https://github.com/godotengine/godot/releases/download/${version}/${archive}.zip"
    mkdir -p "${install_dir}"
    printf 'run-godot-smoke: downloading Godot %s ...\n' "${version}" >&2
    curl --fail --location --silent --show-error --output "${install_dir}/${archive}.zip" "${url}"
    unzip -q -o "${install_dir}/${archive}.zip" -d "${install_dir}"
    rm -f "${install_dir}/${archive}.zip"
    chmod +x "${binary}"
    printf '%s\n' "${binary}"
}

find_godot() {
    if [[ -n "${GODOT_BIN:-}" && -x "${GODOT_BIN}" ]]; then
        printf '%s\n' "${GODOT_BIN}"
        return 0
    fi

    local cached
    cached="${install_dir}/$(platform_binary 2>/dev/null || true)"
    if [[ -n "${cached}" && -x "${cached}" ]]; then
        printf '%s\n' "${cached}"
        return 0
    fi

    local command_name
    for command_name in godot godot4; do
        if command -v "${command_name}" >/dev/null 2>&1; then
            command -v "${command_name}"
            return 0
        fi
    done

    if [[ "${provision}" == "1" ]]; then
        provision_godot
        return $?
    fi

    return 1
}

if ! godot_bin="$(find_godot)"; then
    printf 'run-godot-smoke: Godot %s Mono was not found. Install it, set GODOT_BIN, or pass --provision.\n' "${version}" >&2
    exit 1
fi

printf 'run-godot-smoke: using %s\n' "${godot_bin}" >&2

# A stable link so tooling that needs one fixed path (the Godot MCP server in
# .vscode/mcp.json) does not have to hard-code a version or a machine.
mkdir -p "${tools_dir}/godot"
ln -sfn "${godot_bin}" "${tools_dir}/godot/current"

# Importing resources populates .godot/ so the headless run resolves res:// paths.
"${godot_bin}" --headless --path "${project_dir}" --import >/dev/null

# Godot loads the Debug assembly when it runs the project itself, and the
# export uses Release, so both have to be current: building only Release made
# the smoke run whatever Debug build happened to be lying around.
dotnet build "${project_dir}/PCDynasty.Presentation.csproj" -c Debug --nologo >/dev/null
dotnet build "${project_dir}/PCDynasty.Presentation.csproj" -c Release --nologo >/dev/null

smoke_log="$(mktemp)"
trap 'rm -f "${smoke_log}"' EXIT

set +e
PCDYNASTY_ALLOW_MULTIPLE=1 "${godot_bin}" --headless --path "${project_dir}" \
    --script "res://Tests/SmokeBoot.gd" >"${smoke_log}" 2>&1
smoke_status=$?
set -e

cat "${smoke_log}"

if [[ "${smoke_status}" -ne 0 ]]; then
    printf 'run-godot-smoke: SmokeBoot failed with exit code %s.\n' "${smoke_status}" >&2
    exit "${smoke_status}"
fi

if grep -qE '(SCRIPT ERROR|ERROR:|USER ERROR)' "${smoke_log}"; then
    printf 'run-godot-smoke: SmokeBoot reported engine errors.\n' >&2
    exit 1
fi

printf 'run-godot-smoke: SmokeBoot passed.\n'
