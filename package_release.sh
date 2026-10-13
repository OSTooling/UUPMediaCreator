#!/usr/bin/env bash
#
# Build the release archives containing the UUPMediaCreator command line tools
# and the documentation.
#
# Produces (under publish/):
#   UUPMediaCreator-lin-v<version>.zip          .NET 10: linux-*, win-*, osx-*
#   UUPMediaCreator-lin-v<version>-legacy.zip   .NET 6:  win-legacy-x86/x64
#   checksums.txt                               SHA-256 of both archives
#
# Layout inside both archives:
#   bin/<rid>/        UUPDownload[.exe], UUPMediaConverter[.exe]
#   README.md, README.es.md
#   docs/             TECHNICAL_GUIDE.md, GUIA_TECNICA.md
#
# The binaries are self-contained single-file executables: they do NOT require
# the .NET runtime to be installed on the target machine.
#
# Usage:
#   ./package_release.sh [version]
#
# If no version is given, it is read from
# src/Applications/UUPDownload/Properties/AssemblyInfo.cs.
#
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PUBLISH_DIR="${SCRIPT_DIR}/publish"
VERSION="${1:-$(grep -oP 'AssemblyVersion\("\K[0-9.]+' "${SCRIPT_DIR}/src/Applications/UUPDownload/Properties/AssemblyInfo.cs")}"

# .NET 10 (Windows 10 1607+, Linux, macOS)
MAIN_RIDS=(linux-x64 linux-arm64 win-x64 win-x86 win-arm64 osx-x64 osx-arm64)
# .NET 6 (Windows 7 SP1 / 8 / 8.1)
LEGACY_ARCHS=(x86 x64)

CLI_PROJECTS=(
    "src/Applications/UUPDownload/UUPDownload.csproj"
    "src/Applications/UUPMediaConverter/UUPMediaConverter.csproj"
)
MAIN_STAGE="${PUBLISH_DIR}/release"
LEGACY_STAGE="${PUBLISH_DIR}/release-legacy"
MAIN_ZIP="${PUBLISH_DIR}/UUPMediaCreator-lin-v${VERSION}.zip"
LEGACY_ZIP="${PUBLISH_DIR}/UUPMediaCreator-lin-v${VERSION}-legacy.zip"
CHECKSUMS="${PUBLISH_DIR}/checksums.txt"

for tool in dotnet zip unzip sha256sum; do
    if ! command -v "${tool}" >/dev/null 2>&1; then
        echo "Missing required tool: ${tool}" >&2
        exit 1
    fi
done

# Standard (.NET 10) publish for a given runtime.
publish_std() {
    local rid="$1" out="$2"
    mkdir -p "${out}"
    for proj in "${CLI_PROJECTS[@]}"; do
        echo "    - ${proj}"
        dotnet publish "${SCRIPT_DIR}/${proj}" \
            --configuration Release \
            --runtime "${rid}" \
            -p:UUPMediaTargetFramework=net10.0 \
            -p:SelfContained=true \
            -p:PublishSingleFile=true \
            -p:PublishTrimmed=false \
            --output "${out}"
    done
    find "${out}" -maxdepth 1 -name '*.pdb' -delete
}

# Legacy (.NET 6) publish, used for the Windows 7 SP1+ flavour.
# "dotnet msbuild '/t:restore;publish'" is used instead of "dotnet publish"
# because the target framework is overridden through UUPMediaTargetFramework
# and this keeps the restore and the publish consistent.
publish_legacy() {
    local arch="$1" out="$2" rid="win-${1}"
    mkdir -p "${out}"
    for proj in "${CLI_PROJECTS[@]}"; do
        echo "    - ${proj}"
        dotnet msbuild "${SCRIPT_DIR}/${proj}" '/t:restore;publish' \
            /p:Configuration=Release \
            /p:RuntimeIdentifier="${rid}" \
            /p:UUPMediaTargetFramework=net6.0 \
            /p:LangVersion=latest \
            /p:SelfContained=true \
            /p:PublishSingleFile=true \
            /p:PublishTrimmed=false \
            /p:PublishDir="${out}/"
    done
    find "${out}" -maxdepth 1 -name '*.pdb' -delete
}

stage_docs() {
    local stage="$1"
    cp "${SCRIPT_DIR}/README.md" "${SCRIPT_DIR}/README.es.md" "${stage}/"
    rm -rf "${stage}/docs"
    mkdir -p "${stage}/docs"
    cp "${SCRIPT_DIR}"/docs/*.md "${stage}/docs/"
}

echo "==> Packaging UUPMediaCreator-lin v${VERSION}"

rm -rf "${MAIN_STAGE}" "${LEGACY_STAGE}"
mkdir -p "${MAIN_STAGE}/bin" "${LEGACY_STAGE}/bin"

for rid in "${MAIN_RIDS[@]}"; do
    echo "==> Publishing ${rid}"
    publish_std "${rid}" "${MAIN_STAGE}/bin/${rid}"
done

# Legacy flavour: targets .NET 6, the last .NET runtime that runs on
# Windows 7 SP1 / 8 / 8.1 (the default net10.0 requires Windows 10 1607+).
for arch in "${LEGACY_ARCHS[@]}"; do
    echo "==> Publishing legacy (net6.0) win-${arch}"
    publish_legacy "${arch}" "${LEGACY_STAGE}/bin/win-legacy-${arch}"
done

stage_docs "${MAIN_STAGE}"
stage_docs "${LEGACY_STAGE}"

echo "==> Creating ${MAIN_ZIP}"
rm -f "${MAIN_ZIP}"
( cd "${MAIN_STAGE}" && zip -r -q -X "${MAIN_ZIP}" . )

echo "==> Creating ${LEGACY_ZIP}"
rm -f "${LEGACY_ZIP}"
( cd "${LEGACY_STAGE}" && zip -r -q -X "${LEGACY_ZIP}" . )

echo "==> Creating ${CHECKSUMS}"
( cd "${PUBLISH_DIR}" && sha256sum "$(basename "${MAIN_ZIP}")" "$(basename "${LEGACY_ZIP}")" > "$(basename "${CHECKSUMS}")" )

echo
echo "==> Done:"
ls -la "${MAIN_ZIP}" "${LEGACY_ZIP}" "${CHECKSUMS}"
echo
cat "${CHECKSUMS}"
echo
echo "==> Contents of ${MAIN_ZIP}:"
unzip -l "${MAIN_ZIP}"
