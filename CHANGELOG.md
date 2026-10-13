# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to a `major.minor.patch.build` version scheme.

## [3.2.4.0] - 2026-10-13

### Added
- Windows 7 SP1 / 8 / 8.1 "legacy" builds targeting .NET 6 (`publish_win_legacy.cmd`),
  in 32-bit and 64-bit flavours. Together with the existing .NET 10 builds this
  covers every Windows version in use.
- A shared `publish.cmd <rid> [tfm]` helper used by all the Windows publish
  wrappers; it only needs the .NET SDK (no Visual Studio).
- Release packaging now also ships `win-arm64`, `osx-x64` and `osx-arm64`,
  writes a `checksums.txt` (SHA-256) and splits the legacy binaries into their
  own archive.
- `CHANGELOG.md` and `global.json` (pins the .NET 10 SDK feature band).

### Changed
- CI (`.github/workflows/ci.yml`) built on .NET 10, with native runners per
  platform, NuGet caching, a pinned SDK, action pinning by commit SHA, a job
  for the Windows-only tools (`UUPSort`) and an optional tag-driven release
  job. `Playground` and `Windows.Internal.Flighting` are not built in CI
  because they need the Windows 10 SDK UAP platform that GitHub-hosted
  runners do not provide.
- All Windows `.cmd` scripts now `pushd` to their own directory, quote paths
  (so they survive spaces in the path) and abort on the first error.
- `.gitattributes` forces CRLF line endings for `*.cmd` / `*.bat`.

## [3.2.3.0]

### Added
- 32-bit Windows build (`publish_win_x86.cmd`).
- Windows 10 ESU support (`-u` / `--esu`).

## [3.2.2.0] and earlier

### Added
- Linux support: the two command line tools (`UUPDownload`, `UUPMediaConverter`)
  build and run on Linux with `wimlib`.
