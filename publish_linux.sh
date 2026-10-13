#!/usr/bin/env bash
#
# Publish the UUPMediaCreator command line tools for Linux.
#
# Usage:
#   ./publish_linux.sh [x64|arm64] [output-dir]
#
# Examples:
#   ./publish_linux.sh                 # linux-x64, output under ./publish/artifacts/linux-x64/CLI
#   ./publish_linux.sh arm64           # linux-arm64
#   ./publish_linux.sh x64 /tmp/out    # custom output directory
#
set -euo pipefail

ARCH="${1:-x64}"
RID="linux-${ARCH}"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUT_DIR="${2:-${SCRIPT_DIR}/publish/artifacts/${RID}/CLI}"

if [[ "${ARCH}" != "x64" && "${ARCH}" != "arm64" ]]; then
    echo "Unsupported architecture '${ARCH}'. Use 'x64' or 'arm64'." >&2
    exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
    echo "The .NET SDK was not found in PATH. Install the .NET 10 SDK first." >&2
    exit 1
fi

echo "==> Publishing for ${RID}"
echo "==> Output directory: ${OUT_DIR}"

publish_project() {
    local project="$1"
    echo "--> ${project}"
    dotnet publish "${SCRIPT_DIR}/${project}" \
        --configuration Release \
        --runtime "${RID}" \
        -p:SelfContained=true \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=false \
        --output "${OUT_DIR}"
}

publish_project "src/Applications/UUPDownload/UUPDownload.csproj"
publish_project "src/Applications/UUPMediaConverter/UUPMediaConverter.csproj"

echo
echo "Done. Binaries are in: ${OUT_DIR}"
ls -la "${OUT_DIR}" || true
