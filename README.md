<p align="center">
  <img src="Assets/logo.png" width="176"><br>
  <b>An utility to convert Unified Update Platform file sets into Windows Media files</b><br>
  <a href="./src">Source Code</a> |
  <a href="./docs">Documentation</a> |
  <a href="./thirdparty">Third party libraries</a> |
  <a href="https://github.com/BENDER35/UUPMediaCreator-lin/releases">Downloads</a>
  <br><br>
</p>

> 🌐 **Languages / Idiomas:** English (this file) · [Español](./README.es.md)
> 📖 **Technical guide / Guía técnica:** [English](./docs/TECHNICAL_GUIDE.md) · [Español](./docs/GUIA_TECNICA.md)

## Readme

UUP Media Creator is a set of tools designed to help you scan, fetch updates from Microsoft own Unified Update Platform (UUP), and allow you to create medium out of it when an appropriate tool does not exist in the wild.

Right now the tooling includes:

- UUPDownload: Allows you to scan, fetch and download updates from UUP (Windows Desktop, Windows 10X, Windows Holographic, Windows Server, Windows Mobile, Windows IoT etc...)
- UUPMediaConverter: Allows you to convert a downloaded UUP update for Windows Desktop into an usable ISO image to use in last decade DVD reader or simply mounted.

> ℹ️ **About this fork:** this repository is a Linux-focused fork. Both command line tools are built and verified to run **on Linux** (x64/arm64), Windows and macOS, with a special focus on being able to download **old Windows builds** (Windows 7/8.1/10 era). See the [Dependencies](#dependencies) and [Windows support lifecycle](#windows-10-support-lifecycle) sections.

## Dependencies

The tools are .NET applications, so a .NET runtime is required. During development the .NET **10** SDK is used, and the binaries are published for the platforms below. When running from source (or a framework-dependent build) install:

| Platform | Required |
|----------|----------|
| **Windows** | .NET 10 Runtime (or SDK). For base edition ISO creation **no administration privileges are needed**. |
| **Linux** | .NET 10 Runtime (or SDK) + `genisoimage` (provides `mkisofs`) for ISO creation + `libwim` (wimlib) — the Linux-native `libwim.so` is **bundled** in the published binaries. |

Install commands:

```bash
# Ubuntu / Debian
sudo apt-get install genisoimage
sudo apt-get install dotnet-runtime-10.0   # or the .NET 10 SDK
```

```bash
# Arch Linux
sudo pacman -S genisoimage dotnet-runtime
```

```bash
# Fedora
sudo dnf install genisoimage dotnet-runtime-10.0
```

```bash
# macOS
brew install cdrtools   # provides mkisofs/genisoimage
```

> **Note about `libfuse2`:** older versions of this project pointed to `libfuse2` for Linux VHD mounting. The current fork does **not** contain any FUSE/VHD mounting code for Linux (virtual disk mounting is Windows-only), so `libfuse2` is **not required** on Linux.

### Publishing for Linux

A convenience script is provided:

```bash
./publish_linux.sh            # linux-x64
./publish_linux.sh arm64      # linux-arm64
```

It publishes both `UUPDownload` and `UUPMediaConverter` as single-file executables into `publish/artifacts/linux-<arch>/CLI`.

### Publishing for Windows

On Windows, a `.cmd` helper script is provided per architecture. They require only the **.NET 10 SDK** (no Visual Studio); each one is a thin wrapper around the generic `publish.cmd` helper:

| Script | Architecture | Typical target |
|--------|--------------|----------------|
| `publish_win_x64.cmd` | 64-bit | Windows 10 1607+ / Windows 11 |
| `publish_win_x86.cmd` | 32-bit | Windows 10 1607+ |
| `publish_win_arm64.cmd` | ARM64 | Windows 11 |
| `publish_win_legacy.cmd` | 32-bit **and** 64-bit | **Windows 7 SP1 / 8 / 8.1** and newer |

Each script produces self-contained single-file executables:

- `publish_win_x64.cmd`, `publish_win_x86.cmd` and `publish_win_arm64.cmd` → `publish\artifacts\win-<arch>\CLI`
- `publish_win_legacy.cmd` → `publish\artifacts\win-legacy-x86\CLI` and `publish\artifacts\win-legacy-x64\CLI`

For any other runtime, call the helper directly: `publish.cmd <rid> [tfm]` — for example `publish.cmd linux-x64`, `publish.cmd osx-arm64` or `publish.cmd win-x64 net6.0`.

#### Legacy build (old Windows)

The default build targets **.NET 10**, which **only runs on Windows 10 1607 or later**. To run on **Windows 7 SP1, 8 or 8.1**, `publish_win_legacy.cmd` targets **.NET 6** (the last .NET runtime that runs on Windows 7 SP1) and produces self-contained executables, so **no .NET runtime has to be installed on the target machine**.

The target machine still needs the usual Windows 7 prerequisites: the Universal C Runtime (**KB2999226**), SHA-2 code signing support (**KB4474419**) and **KB2533623**.

> **Note:** .NET 6 is out of support upstream (its security fixes ended in November 2024). The legacy build exists only so the tool can run on operating systems that .NET 10 no longer supports; prefer the default (.NET 10) build whenever possible.

## Windows 10 support lifecycle

This tool can download and build media for **both Windows 10 and Windows 11**, including very old Windows 10 builds (1709, 1803, 1809, 1903…). Because of that, you should be aware of the current support status of the operating systems you are going to deploy:

| Product | Regular (ordinary) end of support | Extended Security Updates (ESU) end |
|---------|-----------------------------------|-------------------------------------|
| Windows 10 (Home / Pro / all retail editions) | **October 14, 2025** | **October 12, 2027** (consumer ESU, including enrollment until that date) · up to **October 2028** for commercial ESU (annual, paid) |
| Windows 8.1 | January 10, 2023 | January 10, 2026 (ESU ended) |
| Windows 7 | January 14, 2020 | January 10, 2023 (ESU ended) |
| Windows Server 2012 / 2012 R2 | October 10, 2023 | October 13, 2026 (ESU ended) |

Sources: [Microsoft – End of support for Windows](https://www.microsoft.com/en-us/windows/end-of-support), [Microsoft – Windows 10 Consumer ESU](https://www.microsoft.com/en-us/windows/extended-security-updates).

## ⚠️ Windows Update certificate rotation (2027) — read before deploying

Microsoft is **rotating the root certificates** used to establish trusted connections to Windows Update. Some of these certificates expire on **May 17, 2027** and **June 19, 2027**.

- Devices running supported Windows versions that keep receiving monthly updates will get the new certificates automatically and are **not** affected.
- Devices running **Windows 7, Windows 8.1 and Windows 10 without ESU** (or any already-out-of-support build) that **cannot receive the updated root certificates** will **stop connecting to Windows Update** and will no longer receive updates at all.
- If you are deploying or maintaining old Windows media with this tool, plan accordingly: either enroll the machine in ESU (Windows 8/10), migrate to a supported Windows version, or apply the manual trusted-root certificate update **before the May/June 2027 deadlines**.

Source: [Microsoft Tech Community – Prepare for Windows Update certificate rotation in 2027](https://techcommunity.microsoft.com/blog/windows-itpro-blog/prepare-for-windows-update-certificate-rotation-in-2027/4562463)

## Supported features by OS

| Feature                                                                                  | Windows | Linux (3) | macOS  |
|------------------------------------------------------------------------------------------|---------|--------|--------|
| Downloading files from UUP (UUPDownload)                                                 | ✅       | ✅       | ✅       |
| Replaying past downloads from UUP (UUPDownload)                                          | ✅       | ✅       | ✅       |
| Decrypting ESRP payloads from UUP (UUPDownload)                                          | ✅       | ✅       | ✅       |
| Verifying downloaded payloads from UUP (UUPDownload)                                     | ✅       | ✅       | ✅       |
| Sample available builds from UUP (get-builds argument) (UUPDownload)                     | ✅       | ✅       | ✅       |
| Converting Desktop UUP files to an ISO for one base edition (UUPMediaConverter)          | ✅       | ✅       | ✅       |
| Does not require administrative privileges for base edition ISO images                   | ✅       | ✅       | ✅       |
| CLI available                                                                            | ✅       | ✅       | ✅       |
| ARM64 Support (aka works on @sinclairinat0r device (TM))                                 | ✅       | ✅       | ✅       |
| Converting Desktop UUP files to an ISO for all possible editions (UUPMediaConverter)     | ✅       | ❌       | ❌       |
| ISO Preinstallation Environment close to original (UUPMediaConverter)                    | ✅       | ❌       | ❌       |
| ISO built by CDImage Mastering Utility with UDF and boot sector (UUPMediaConverter)      | ✅       | ❌       | ❌       |
| ISO built by mkisofs with UDF and boot sector (UUPMediaConverter)                        | ❌       | ✅   (1) | ✅   (2) |
| Update file integration (UUPMediaConverter)                                              | ❌       | ❌       | ❌       |
| Building Hololens, Mobile, 10X, IoT images                                               | ❌       | ❌       | ❌       |
| UUPDownload ease of use (will come soon, we promise)                                     | ❌       | ❌       | ❌       |

(1): requires ```apt-get install genisoimage```

(2): requires ```brew install cdrtools```

(3): the published binaries are self-contained single-file executables, so the .NET runtime is **not** required. The bundled ```libwim.so``` (wimlib) is used to build the ISO. `libfuse2` is **not** needed in this fork.

## Usage

UUPDownload might be a little too complicated to use for some people. For reference purposes, here's the current set of parameters you can use:

| Channel                               | Command |
|---------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Retail (Windows 10, Desktop)          | -s Professional -v 10.0.19043.1 -r Retail -b Retail -c vb_release -t [your architecture] |
| Retail (Windows 10 ESU, Desktop)      | -s Professional -v 10.0.19045.1 -r Retail -b Retail -c vb_release -u -t [your architecture] |
| Retail (Windows 11, Desktop)          | -s Professional -v 10.0.26100.1 -r Retail -b Retail -c ge_release -t [your architecture] |
| Retail (IoT)                          | -s IoTUAP -v 10.0.17763.1 -r Retail -b Retail -c rs5_release -t [your architecture] |
| Retail (Holographic)                  | -s Holographic -v 10.0.20346.1 -r Retail -b Retail -c fe_release -t [your architecture] |
| Retail (Mobile)                       | -s MobileCore -v 10.0.15254.1 -r Retail -b Retail -c feature2 -t [your architecture] |
| Retail (Team)                         | -s PPIPro -v 10.0.19043.1 -r Retail -b Retail -c vb_release -t [your architecture] |
| Retail (Server)                       | -s DatacenterServer -v 10.0.20348.1 -r Retail -b Retail -c fe_release -t [your architecture] |
| Release Preview (Windows 10, Desktop) | -s Professional -v 10.0.19043.1 -r External -b ReleasePreview -c vb_release -t [your architecture] |
| Release Preview (Windows 11, Desktop) | -s Professional -v 10.0.26100.1 -r External -b ReleasePreview -c ge_release -t [your architecture] |
| Release Preview (HCI)                 | -s AzureStackHCIServerCore -v 10.0.22000.1 -r External -b ReleasePreview -c rs_prerelease -t [your architecture] |
| Beta (Windows 10, Desktop)            | -s Professional -v 10.0.19045.1 -r External -b Beta -c vb_release -t [your architecture] |
| Beta (Windows 11, Desktop)            | -s Professional -v 10.0.26100.1 -r External -b Beta -c ge_release -t [your architecture] |
| Beta (Holographic)                    | -s Holographic -v 10.0.19043.1 -r External -b Beta -c vb_release -t [your architecture] |
| Beta (Team)                           | -s PPIPro -v 10.0.19043.1 -r External -b Beta -c vb_release -t [your architecture] |
| Dev (Desktop)                         | -s Professional -v 10.0.26100.1 -r External -b Dev -c rs_prerelease -t [your architecture] |
| Dev (Holographic)                     | -s Holographic -v 10.0.19043.1 -r External -b Dev -c vb_release -t [your architecture] |
| Dev (Team)                            | -s PPIPro -v 10.0.19043.1 -r External -b Dev -c vb_release -t [your architecture] |
| Canary (Desktop)                      | -s Professional -v 10.0.26100.1 -r External -b CanaryChannel -c rs_prerelease -t [your architecture] |

> **Windows 11 branches (2026):** Windows 11 now ships from the **GE (Germanium)** branch. Use `-c ge_release` for Windows 11 **Retail**, **Release Preview** and **Beta**; use `-c rs_prerelease` for **Dev** and **Canary**. The old `ni_release` branch (21H2/22H2/23H2) is no longer served by the Beta/Release Preview channels, so using it together with `-y` returns *"No updates found"*. This was verified against the live servers: Beta = `10.0.26220.9606 (ge_release_svc_betaflt_prod1)`, Dev = `10.0.26340.9616 (ge_prerelease_im)`, Canary = `10.0.28020.2207`.

> **Windows 10 Beta:** Microsoft still serves the Windows 10 **Beta** channel — it answers with the newest Windows 10 22H2 build (`10.0.19045.6396`, `vb_release_svc_prod3.250923-1751`). Use `-s Professional -v 10.0.19045.1 -r External -b Beta -c vb_release -t [your architecture]` **without `-y`** to always fetch the latest available Beta build. Windows 10 **Dev** now returns Windows 11 builds instead.

> **Windows 10 ESU (Extended Security Updates):** Windows 10 22H2 reached end of support on **2025-10-14**; the monthly updates published afterwards are only offered to devices enrolled in the ESU programme. Append `-u` (`--esu`) to `request-download` — or use the **`Retail (Windows 10 ESU)`** entry with `get-builds` — to report the device as ESU-enrolled. The servers then hand out the ESU build, e.g. `10.0.19045.7727 (vb_release_svc_im.260912-0526)`, for **x86**, **amd64** and **arm64**. Example: `-s Professional -v 10.0.19045.1 -r Retail -b Retail -c vb_release -u -e Professional -l en-US`. Without `-u` you only get the last regular 22H2 build (`10.0.19045.6456`).

> **Reliability:** the Windows Update servers occasionally answer with an empty set when many requests are fired in quick succession. `UUPDownload` now waits and **retries once** before reporting *"No updates found"*, for both `get-builds` and `request-download`.

> **Downloading old Windows 10 builds:** the oldest UUP set still served by Microsoft is **Windows 10 1709 (RS3, `rs3_release`)**, then 1803 (`rs4_release`), 1809 (`rs5_release`), 1903 (`19h1_release`) and 22H2 (`vb_release`). **Windows 10 1703 (RS2, `rs2_release`) and older (1607, 1511, 1507) are no longer available** — Microsoft has retired them from the update servers. Use the `get-builds` verb to see what is currently offered and `-y` to pin an exact version, e.g. 1809: `-s Professional -v 10.0.17763.1217 -r Retail -b Retail -c rs5_release -y -e Professional -l en-US`.

> **Windows Insider Preview ISOs:** the official [Microsoft Insider Preview download page](https://www.microsoft.com/software-download/windowsinsiderpreviewiso) now only offers **Windows 11** (the Windows 10 Insider ISOs were removed). To obtain Windows 10 media — including old builds — use `UUPDownload` as shown above.

**TIP 1:** You can append ```-e [Edition to download]``` to get the files needed only for a specific edition

**TIP 2:** You can append ```-l [Language to download]``` to get the files needed only for a specific language

### Important

The ```-v``` is __not__ used to specify which version of Windows you want to download! The parameters used by UUPDownload above are used to tell Windows Update **which** version of Windows you are currently running. Therefore, the ```-v``` parameter is used to tell Windows Update your current version of Windows. You will never get versions lower than the value specified in ```-v``` and some requests may also never get you the version specified in ```-v``` but only newer. If, however, you want to download a very specific version using ```-v``` __then__ you must use the -y option to tell Windows Update you only want this version.

## Contributing

You are free to contribute to this project, in fact we would welcome any form of help. But please keep in mind the following points if you do:

- Please try to maintain your PR with a reasonable set of commits
- Please try to not change functionality not concerned by your PR
- Avoid changing the entire app, if you feel like this is needed, please file an issue so we can discuss about it between contributors.

## Bugs

This project, as with anything in Computer Science, is not bug free. If you find anything wrong, please file an issue in the issue tracker. Please include the tool version, as well as the Operating System you are running on your computer, and any other applicable details helpful to reproduce the issue.