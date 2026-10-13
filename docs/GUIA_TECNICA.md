# UUP Media Creator — Guía Técnica

> 🌐 [English](./TECHNICAL_GUIDE.md) · Español

Este documento explica en profundidad cómo funciona el conjunto de herramientas UUP Media Creator internamente: cómo habla con los servidores de Windows Update (UUP) de Microsoft, cómo convierte un conjunto de archivos descargado en un ISO de arranque, y cómo compilar, ejecutar y verificar el proyecto en Linux, Windows y macOS.

- [1. Descripción general](#1-descripción-general)
- [2. Arquitectura y componentes](#2-arquitectura-y-componentes)
- [3. Cómo funciona la descarga](#3-cómo-funciona-la-descarga)
- [4. Cómo funciona la conversión de medios](#4-cómo-funciona-la-conversión-de-medios)
- [5. Referencia de la CLI](#5-referencia-de-la-cli)
- [6. Compilar y publicar](#6-compilar-y-publicar)
- [7. Plataformas admitidas](#7-plataformas-admitidas)
- [8. Verificación](#8-verificación)
- [9. Solución de problemas](#9-solución-de-problemas)
- [10. Referencias](#10-referencias)

---

## 1. Descripción general

UUP Media Creator es un conjunto de herramientas de línea de comandos .NET 10:

| Herramienta | Ensamblado | Propósito |
|------|----------|---------|
| `UUPDownload` | `src/Applications/UUPDownload` | Escanea, consulta y descarga conjuntos de archivos de la Plataforma Unificada de Actualizaciones (UUP) de Microsoft |
| `UUPMediaConverter` | `src/Applications/UUPMediaConverter` | Convierte un conjunto UUP de Windows Desktop descargado en un ISO de arranque |

Ambos objetivos son **multiplataforma** (`net10.0`, RIDs `win-*`, `linux-*`, `osx-*`). La compilación para Linux usa la biblioteca nativa **wimlib** (`libwim.so`, incluida vía el paquete NuGet `ManagedWimLib`) y la herramienta externa **`mkisofs`/`genisoimage`** para masterizar el ISO, mientras que Windows usa WIMGAPI/DISM y el `cdimage.exe` incluido.

---

## 2. Arquitectura y componentes

```
UUPMediaCreator-lin/
├── src/
│   ├── Applications/
│   │   ├── UUPDownload/                  # CLI: request-download, replay-download, get-builds
│   │   ├── UUPMediaConverter/            # CLI: desktop-convert
│   │   ├── UUPSort/                      # utilidad de post-procesado (TFM solo Windows)
│   │   └── Playground/                   # zona de pruebas (TFM solo Windows)
│   ├── UnifiedUpdatePlatform.Services.WindowsUpdate/     # CTAC + cliente SOAP FE3 + ESRP + auth (MBI)
│   ├── UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads/  # descargador de archivos + verificación + replay
│   ├── UnifiedUpdatePlatform.Services.Composition.Database/     # análisis de CompDB (.xml.cab)
│   ├── UnifiedUpdatePlatform.Media.Creator.Planning/            # planificación edición/idioma
│   ├── UnifiedUpdatePlatform.Media.Creator/                     # creación de ISO/medios, operaciones DISM, envoltorio CDImage
│   ├── UnifiedUpdatePlatform.Imaging/                           # WIMGAPI con respaldo wimlib (Linux/macOS)
│   ├── Cabinet/                          # descompresión CAB/LZX (Cabinet.Shared)
│   ├── TempManager/                      # gestor de directorio temporal multiplataforma
│   ├── CompDB/                           # modelos y servicios de bases de datos de composición
│   ├── VirtualHardDiskLib/               # montaje de VHD — SOLO WINDOWS (virtdisk.dll)
│   └── Windows.Internal.Flighting/       # sonda WinRT de vuelos (solo Windows, solo desarrollo)
└── thirdparty/
    ├── Microsoft.Wim/                    # envoltorio P/Invoke de WIMGAPI (usado en Windows)
    ├── Microsoft.Dism/                   # envoltorio P/Invoke de la API DISM (usado en Windows)
    └── Privileges/                       # herramienta de elevación de privilegios de Windows
```

Notas clave:

- **`TempManager`** (`src/TempManager/TempManager.cs`) resuelve el directorio temporal. En Windows es la variable de entorno `TEMP`; en Linux/macOS `TEMP` normalmente no está definida, por lo que se usa `Path.GetTempPath()` como respaldo (que respeta `TMPDIR`).
- **`VirtualHardDiskLib`** es solo Windows (P/Invoke de `virtdisk.dll`/`kernel32.dll`). Solo se alcanza en las rutas de *múltiples ediciones*, *actualización (upgrade)* y *slipstream de AppX*, **no** en la ruta de ISO de edición base única. Por eso la conversión de "todas las ediciones" está marcada como no admitida en Linux.
- **`thirdparty/Microsoft.Wim`** se compila en todas partes pero solo *funciona* en Windows (carga `WimgApi.dll`); en Linux/macOS la capa de imagen (`src/UnifiedUpdatePlatform.Imaging/WimLibImaging.cs`) comprueba `RuntimeInformation.IsOSPlatform` y usa **wimlib**.

---

## 3. Cómo funciona la descarga

### 3.1 La "suplantación" de un equipo Windows

El descargador nunca pregunta a Microsoft "dame el Windows X". En su lugar se hace pasar por un equipo Windows real y pregunta "¿qué tienes para un equipo como yo?". Es el significado de los famosos parámetros:

- `-s / --reporting-sku` — la SKU/producto a reportar (enum `OSSkuId`, ver `CTAC.cs:53-258`).
- `-v / --reporting-version` — la versión de SO que se afirma tener (`10.0.x.y`).
- `-t / --machine-type` — la arquitectura (enum `MachineType`: `x86`, `amd64`, `arm64`, …).
- `-r / --flight-ring` — anillo de servicio (`Retail`, `External`, `Internal`, …).
- `-c / --current-branch` — rama de lanzamiento (`vb_release`, `ge_release`, `ni_release`, `rs_prerelease`, `rs5_release`, …).
- `-b / --flighting-branch-name` — rama insider (`Retail`, `ReleasePreview`, `Beta`, `Dev`, `CanaryChannel`).
- `-y / --sync-current-version-only` — fijar la versión actual exacta en lugar de obtener las más nuevas.

Todos estos parámetros se ensamblan en un **CTAC** (Client Targeting Attribute Context) en `src/UnifiedUpdatePlatform.Services.WindowsUpdate/CTAC.cs`. Por cada SKU se envía un conjunto de atributos a Windows Update:

```
E:...&FlightRing=...&DeviceFamily=Windows.Desktop&OSSkuId=...&CurrentBranch=...&OSVersion=...&OSArchitecture=AMD64&...
```

y se construye una cadena de *producto* en `CTAC.cs:522-526`:

```
PN=Client.OS.rs2.amd64&Branch=vb_release&PrimaryOSProduct=1&Repairable=1&V=10.0.19041.200;
```

El mapeo SKU → familia de dispositivo está codificado (`CTAC.cs:317-378`): Desktop → `Windows.Desktop`, Server → `Windows.Server`, `IoTUAP` → `Windows.IoTUAP`, `MobileCore` → `Windows.Mobile`, `Holographic` → `Windows.Holographic`, `Lite`/`Andromeda`/`HubOS` → `Windows.Core` (10X/WCOS), `PPIPro` → `Windows.Team`.

### 3.2 El protocolo FE3

Todas las peticiones van al punto final FE3:

- Punto final: `https://fe3cr.delivery.mp.microsoft.com/ClientWebService/client.asmx` (`Constants.cs:28`), variante segura en `/secured`.
- Espacio de nombres SOAP: `http://www.microsoft.com/SoftwareDistribution/Server/ClientWebService`.
- Versión de protocolo: `2.50`.
- Acciones SOAP: `GetCookie`, `SyncUpdates`, `GetExtendedUpdateInfo`, `GetExtendedUpdateInfo2` (`FE3Handler.cs`).
- Filtro de actualización: siempre `ProductRelease` (`FE3Handler.cs:545-550`).

Opcionalmente se puede obtener un token de autorización Insider (MSA) con `-m`/`-p` a través de `MBIHelper.cs` (`login.live.com` OAuth, ámbito `service::dcat.update.microsoft.com::MBI_SSL`).

### 3.3 Enumerar compilaciones disponibles

`UUPDownload get-builds` recorre un conjunto fijo de CTAC (uno por anillo/rama, `RingCheck.cs`) e imprime la compilación más nueva que ofrece cada uno. Los presets cubren los conjuntos retail de Windows 10 que aún se sirven (1709 a 22H2) y los canales retail/insider actuales de Windows 11, para que puedas confirmar qué se está sirviendo:

```
"Retail (1709 - RS3)"[0]="10.0.16299.15 (rs3_release.170928-1534)"
"Retail (1803 - RS4)"[0]="10.0.17134.1 (rs4_release.180410-1804)"
"Retail (1809 - RS5)"[0]="10.0.17763.1217 (rs5_release_svc_prod1.200505-1609)"
"Retail (1903 - 19H1)"[0]="10.0.18362.836 (19h1_release_svc_prod1.200505-1604)"
"Retail (Windows 10 - VB)"[0]="10.0.19045.6456 (vb_release_svc_prod1.251010-0328)"
"Retail (Windows 10 ESU)"[0]="10.0.19045.7727 (vb_release_svc_im.260912-0526)"
"Retail (Windows 11 - GE)"[0]="10.0.26200.9457 (ge_release_svc_im.260912-0409)"
"Beta (Windows 11)"[0]="10.0.26220.9606 (ge_release_svc_betaflt_prod1.260929-1929)"
"Release Preview (Windows 11)"[0]="10.0.26100.9550 (ge_release_svc_prod3.260919-2247)"
"Dev"[0]="10.0.26340.9616 (ge_prerelease_im.260929-2016)"
"Canary"[0]="10.0.28020.2207 (br_release_svc_betaflt_prod1.260526-1854)"
"Beta (Windows 10)"[0]="10.0.19045.6396 (vb_release_svc_prod3.250923-1751)"
```

Notas:

- **Windows 10 1703 (RS2, `rs2_release` / 15063) y anteriores ya no se sirven** por los servidores de actualización. El conjunto retail más antiguo que sigue disponible es **1709 (RS3)**; 1607, 1511 y 1507 ya no están. Los presets de `get-builds` se corrigieron en consecuencia (las antiguas entradas engañosas "Insider Slow/Fast (RS2)" reportaban un dispositivo 15063 pero los servidores respondían con una compilación mucho más nueva).
- El **Beta** y el **Release Preview** de Windows 11 ahora están en la rama **GE (Germanium)** → usa `-c ge_release`. **Dev**/**Canary** usan `-c rs_prerelease`. La antigua rama `ni_release` (21H2/22H2/23H2) ya no devuelve contenido en esos canales, por lo que `-b Beta -c ni_release -y` da *"No updates found"*.
- `get-builds` consulta cada anillo en secuencia; los servidores a veces responden con un conjunto vacío cuando se consultan muchos anillos muy seguidos, por lo que la herramienta espera y **reintenta una vez** antes de informar de que no hay nada (`RingCheck.cs`).
- La **Beta de Windows 10** sigue sirviéndose: el canal Beta responde con la compilación más nueva de Windows 10 22H2 (`10.0.19045.6396`, `vb_release_svc_prod3`). Consúltala con `-v 10.0.19045.1 -b Beta -c vb_release` y **sin** `-y`, p. ej. `-s Professional -v 10.0.19045.1 -r External -b Beta -c vb_release -t amd64`. El canal **Dev** de Windows 10 ahora devuelve compilaciones de Windows 11 (verificado: `10.0.26340.9616`).
- **Windows 10 ESU** (`-u` / `--esu`, `CTAC.IsEsu`): Windows 10 22H2 llegó al fin de soporte el 14-10-2025 y las actualizaciones mensuales posteriores solo se ofrecen a dispositivos inscritos en ESU. El flag añade los atributos de elegibilidad/inscripción comercial ESU a `DeviceAttributes` (`Win10ConsumerESUStatus=3`, `Win10ConsumerESUAY=10`, `Win10CommercialAzureESUEligible=1`, `Win10CommercialKeybasedESUEligible=2`, `Win10CommercialW365ESUEligible=1`), que es lo que hace UUP dump. Los servidores devuelven entonces la compilación ESU (`10.0.19045.7727`, `vb_release_svc_im.260912-0526`) para **x86**, **amd64** y **arm64**; sin `-u` solo se ofrece la última compilación 22H2 normal (`10.0.19045.6456`). Ejemplo: `-s Professional -v 10.0.19045.1 -r Retail -b Retail -c vb_release -u -e Professional -l en-US`.

### 3.4 Descargar un conjunto de archivos

`UUPDownload request-download`:

1. Construye el CTAC y llama a `SyncUpdates` para descubrir las actualizaciones aplicables.
2. Selecciona la actualización UUP (`FileExchangeV3UpdateFilter.ProductRelease`).
3. Descarga los metadatos (archivos CompDB `.xml.cab`).
4. **Planifica la lista de archivos** para la edición/idioma solicitados (`CompDB` → `ConversionPlanBuilder.GetTargetedPlan`). Si se dan `-e`/`-l`, solo se conservan las cargas útiles de esa edición/idioma (`DownloadHelpers.cs`).
5. **Descarga cada carga útil** (`HttpDownloader`), descifrando opcionalmente los blobs **ESRP** (Encrypted Service Release Payload) y verificando los hashes SHA-256.
6. Escribe un archivo de metadatos `*.uupmcreplay` para el verbo `replay-download`.

Las líneas por archivo que imprime la herramienta usan este formato:

```
[HH:mm:ss][n/N][E:<errores>][C|D|V|F] <carga útil> (<tamaño>)
```

donde `C` = completado/verificado, `D` = descargando, `V` = verificado, `F` = fallido.

---

## 4. Cómo funciona la conversión de medios

`UUPMediaConverter desktop-convert` (punto de entrada `DesktopConvert.cs` → `MediaCreator.CreateISOMedia`):

1. **Analizar el conjunto UUP** — `FileLocator.GetCompDBsFromUUPFiles` abre los `.xml.cab` de CompDB y crea un área de trabajo temporal (`TempManager`).
2. **Planificar ediciones** — `ConversionPlanBuilder.GetTargetedPlan` produce `EditionTarget`s para el idioma solicitado. Con `-e` solo se construye esa edición (es la ruta compatible con Linux); sin él, se intentan "todas las ediciones", que en Linux **no** está soportado (requiere VHD/DISM).
3. **Construir los medios de instalación** — `SetupMediaCreator.CreateSetupMedia` copia los archivos de arranque, `boot.sdi`, `winre.wim`, los paquetes de idioma y los binarios del SO en un `MediaRoot`, y reconstruye la imagen de Windows RE (aquí es donde se usa wimlib en Linux para operaciones tipo `wimlib-imagex`).
4. **Construir `install.wim`/`install.esd`** — `HandleEditionPlan` captura la edición base en `sources\install.wim` (o `.esd` con `-c LZMS`).
   - **Windows:** WIMGAPI (`WimgApi.dll`) para operaciones WIM.
   - **Linux/macOS:** wimlib (`libwim.so`/`.dylib`).
5. **Masterizar el ISO** — `CDImageWrapper.GenerateISOImage`:
   - **Windows:** `cdimage.exe` incluido (en `CDImage/`) con `-bootdata` para eltorito + UDF.
   - **Linux/macOS:** ejecuta **`mkisofs`** (de `genisoimage`/`cdrtools`) con `--no-emul-boot --eltorito-alt-boot --udf` y los archivos de arranque `etfsboot.com`/`efisys.bin` correspondientes (`CDImageWrapper.cs:86-128`).

> **Nota:** la rama no Windows no valida que `mkisofs` exista en PATH; el proceso simplemente falla si falta. Instala `genisoimage` en Debian/Ubuntu (`apt-get install genisoimage`) o `cdrtools` en macOS (`brew install cdrtools`).

### Capacidades por plataforma

| Operación | Windows | Linux/macOS |
|-----------|---------|-------------|
| Descargar UUP, verificar, replay, descifrar ESRP | ✅ | ✅ |
| ISO de edición base (una edición) | ✅ | ✅ |
| ISO de todas las ediciones, upgrades, slipstream AppX | ✅ | ❌ (requiere VHD + DISM, solo Windows) |
| `--no-key-prompt` (prompt de arranque silencioso) | ✅ | ✅ |

---

## 5. Referencia de la CLI

### `UUPDownload`

```
UUPDownload request-download (predeterminado)
  -s, --reporting-sku        SKU a reportar (OSSkuId). p. ej. Professional  [obligatorio]
  -v, --reporting-version    Versión de SO a afirmar. p. ej. 10.0.19041.200  [obligatorio]
  -t, --machine-type         Arquitectura (MachineType). p. ej. amd64        [obligatorio]
  -r, --flight-ring          Anillo. p. ej. Retail / External / Internal     [obligatorio]
  -c, --current-branch       Rama. p. ej. vb_release / ge_release / rs_prerelease   [obligatorio]
  -b, --flighting-branch-name  Retail | ReleasePreview | Beta | Dev | CanaryChannel
  -y, --sync-current-version-only  fijar la versión actual exacta
  -a, --branch-readiness-level     (por defecto CB)
  -o, --output-folder             directorio de salida (por defecto .)
  -e, --edition                   filtro de edición (con -l)
  -l, --language                  filtro de idioma (con -e)
  -z, --releasetype               (por defecto Production)
  -n, --contenttype               (por defecto Mainline)
  -u, --esu                       reportar como Windows 10 inscrito en ESU (compilaciones ESU de Windows 10 22H2)
  -m, --mail / -p, --password     credenciales Insider (MSA) (opcional)

UUPDownload replay-download
  -r, --replay-metadata   archivo *.uupmcreplay [obligatorio]
  -t, --machine-type      [obligatorio]
  -o, --output-folder     (por defecto .)
  -e, --edition / -l, --language
  --fixup Appx            fixup de appx
  --appxroot / --cabsroot

UUPDownload get-builds
  -s, --reporting-sku     [obligatorio]
  -t, --machine-type      [obligatorio]
  -m / -p                 credenciales Insider (opcional)
  --preview-targeting-attribute   filtrar por nombre de anillo (opcional)
```

### `UUPMediaConverter`

```
UUPMediaConverter desktop-convert
  -u, --uup-path           ruta de los archivos UUP descargados      [obligatorio]
  -i, --iso-path           ruta del ISO de destino                   [obligatorio]
  -l, --language-code      idioma a construir, p. ej. en-US          [obligatorio]
  -e, --edition            edición a construir (omitir = todas las ediciones)
  -t, --temp-path          directorio temporal (por defecto %TEMP% / temporal del sistema)
  -c, --compression-format XPRESS | LZX | LZMS   (por defecto LZX)
  --no-key-prompt          usar implementación efisys/cdboot silenciosa
```

---

## 6. Compilar y publicar

Requisitos: [SDK de .NET 10](https://dotnet.microsoft.com/download), y para crear ISOs `genisoimage` (Linux) / `cdrtools` (macOS).

```bash
# Compilar (Linux/macOS)
dotnet build src/Applications/UUPDownload/UUPDownload.csproj -c Release
dotnet build src/Applications/UUPMediaConverter/UUPMediaConverter.csproj -c Release

# Publicar como archivo único para Linux
./publish_linux.sh            # linux-x64
./publish_linux.sh arm64      # linux-arm64
```

El `.sln` completo incluye proyectos solo Windows (`UUPSort`, `Playground`, `Windows.Internal.Flighting`) que apuntan a `net10.0-windows`; compila los dos proyectos CLI por separado (como arriba) o mediante la matriz del CI (`.github/workflows/ci.yml`, SDK de .NET 10), que publica los CLIs para todos los RIDs `win-*`/`linux-*`/`osx-*`, compila las variantes heredadas (`net6.0`) `win-legacy-x86`/`win-legacy-x64` y compila la herramienta solo Windows `UUPSort`. (`Playground` y `Windows.Internal.Flighting` no se compilan en el CI: necesitan la plataforma UAP del SDK de Windows 10, que los runners de GitHub no proporcionan.)

### 6.1 Scripts de publicación para Windows

En Windows, un script `.cmd` por arquitectura. Todos son envoltorios finos sobre el ayudante genérico `publish.cmd <rid> [tfm]` y solo necesitan el SDK de .NET 10 (sin Visual Studio / MSBuild de escritorio):

| Script | Arquitectura | Salida |
|--------|--------------|--------|
| `publish_win_x64.cmd` | x64 | `publish\artifacts\win-x64\CLI` |
| `publish_win_x86.cmd` | x86 (32 bits) | `publish\artifacts\win-x86\CLI` |
| `publish_win_arm64.cmd` | ARM64 | `publish\artifacts\win-arm64\CLI` |
| `publish_win_legacy.cmd` | x86 + x64 | `publish\artifacts\win-legacy-{x86,x64}\CLI` |

El framework de destino se selecciona mediante la propiedad `UUPMediaTargetFramework`, definida en el `Directory.Build.props` de la raíz del repositorio y con valor predeterminado `net10.0`. Todos los proyectos que forman parte de los CLIs usan `<TargetFramework>$(UUPMediaTargetFramework)</TargetFramework>`, por lo que la compilación predeterminada no cambia.

El script **heredado** la sobrescribe con **`net6.0`**, el último runtime de .NET que funciona en **Windows 7 SP1 / 8 / 8.1** (el `net10.0` predeterminado requiere Windows 10 1607+). También pasa `/p:LangVersion=latest` porque algunos archivos usan características del lenguaje más recientes que el valor predeterminado de C# 10 en `net6.0`. Los binarios publicados son ejecutables de archivo único autocontenidos, por lo que no se necesita ningún runtime de .NET en el destino:

```cmd
:: equivalente a publish_win_legacy.cmd
publish.cmd win-x86 net6.0
publish.cmd win-x64 net6.0
```

Internamente cada proyecto se publica con `dotnet msbuild` (no hace falta el MSBuild de escritorio):

```cmd
dotnet msbuild "src\Applications\UUPDownload\UUPDownload.csproj" "/t:restore;publish" ^
  /p:Configuration=Release /p:RuntimeIdentifier=win-x64 ^
  /p:UUPMediaTargetFramework=net6.0 /p:LangVersion=latest ^
  /p:SelfContained=true /p:PublishSingleFile=true /p:PublishTrimmed=false ^
  "/p:PublishDir=publish\artifacts\win-legacy-x64\CLI\"
```

Fueron necesarios pequeños ajustes de compatibilidad entre frameworks para que el código compile con `net10.0` y `net6.0`; son inocuos para la compilación predeterminada:

- Conversiones `IntPtr` → `long`/`char` (`WimMessage.cs`) y `new(-1)` (`NativeMethods.cs`), porque `IntPtr` solo ganó conversiones numéricas implícitas en .NET 7.
- `.AsEnumerable().Reverse()` donde se llamaba a `Reverse()` directamente sobre un array (`BuildFetcher.cs`, `PathUtils.cs`, `FolderUtilities.cs`, `DesktopConvert.cs`, `WimLibImaging.cs`): en `net6.0` el array se enlaza con la sobrecarga `void MemoryExtensions.Reverse(Span<T>)`.
- `[GeneratedRegex]` está envuelto en `#if NET7_0_OR_GREATER`, con un `Regex` compilado como alternativa para `net6.0` (`DownloadHelpers.cs`).

> **Nota:** .NET 6 dejó de recibir soporte en noviembre de 2024. La variante heredada existe únicamente para ejecutar la herramienta en sistemas operativos que .NET 10 ya no admite (Windows 7 SP1 / 8 / 8.1). Además, Windows 7 SP1 requiere el Universal C Runtime (KB2999226), el soporte de firma de código SHA-2 (KB4474419) y KB2533623.

### 6.2 Empaquetado de la release

`package_release.sh` compila todos los runtimes y escribe los archivos de la release en `publish/`:

```bash
./package_release.sh            # versión leída del AssemblyInfo.cs de UUPDownload
./package_release.sh 3.2.4.0    # versión explícita
```

- `UUPMediaCreator-lin-v<version>.zip` — compilaciones .NET 10 para `linux-x64`, `linux-arm64`, `win-x64`, `win-x86`, `win-arm64`, `osx-x64`, `osx-arm64`
- `UUPMediaCreator-lin-v<version>-legacy.zip` — compilaciones .NET 6 para Windows 7 SP1+ (`win-legacy-x86`, `win-legacy-x64`)
- `checksums.txt` — SHA-256 de ambos archivos

Cada archivo contiene `bin/<rid>/` con los dos ejecutables, además de los README y `docs/`.

---

## 7. Plataformas admitidas

Consulta la matriz en el [README](../README.es.md#funciones-admitidas-por-sistema-operativo). Resumen:

- **Descarga** (todos los verbos) funciona en Windows, Linux y macOS.
- **Conversión de ISO de edición base** funciona en las tres plataformas (mkisofs en Linux/macOS, cdimage en Windows).
- Los flujos de **todas las ediciones/upgrade/AppX** son solo Windows (VHD + DISM).
- `UUPSort`, `Playground`, `Windows.Internal.Flighting` son utilidades de desarrollo solo Windows, no forman parte de los CLIs publicados.

---

## 8. Verificación

Verificado en **Linux (Ubuntu 26.04, x86_64, SDK de .NET 10.0.112)**:

| Prueba | Resultado |
|------|--------|
| `dotnet build` de ambos CLIs (`net10.0`, Release) | ✅ 0 errores |
| `UUPDownload --help`, `UUPMediaConverter --help` | ✅ |
| `UUPDownload get-builds -s Professional -t amd64` — reporta compilaciones desde **Windows 10 1709 (RS3)** hasta Dev/Canary actuales | ✅ |
| `UUPDownload request-download` de **Windows 10 1809 (RS5)** (`rs5_release`, `-y`, `-e Professional -l en-US`) — 78 cargas útiles, conjunto multi-GB descargado, hashes verificados | ✅ |
| `UUPMediaConverter desktop-convert` → ISO vía wimlib + mkisofs | ✅ Windows 10 1809 (RS5) → ISO de arranque de 4,2 GB (`CPRA_X64FRE_EN-US_DV9`, El-Torito no-emulation, 945 archivos / 84 carpetas) |
| `request-download ... -c vb_release -u` (**Windows 10 ESU**, `-v 10.0.19045.1`, `-e Professional -l en-US`) — el servidor devuelve la compilación ESU `10.0.19045.7727 (vb_release_svc_im.260912-0526)` para x86, amd64 y arm64; las cargas útiles se descargan y verifican (p. ej. KB5122887) | ✅ |
| Publicación autocontenida predeterminada `net10.0` **win-x86** (`UUPDownload`/`UUPMediaConverter`) con `dotnet publish -r win-x86` | ✅ se generan ejecutables PE32 válidos |
| Publicación **heredada `net6.0`** (`/p:UUPMediaTargetFramework=net6.0 /p:LangVersion=latest`, el equivalente de `publish_win_legacy.cmd`) para win-x64 y win-x86, `dotnet msbuild '/t:restore;publish'` | ✅ se generan ejecutables de archivo único autocontenidos (63/59 MB); ambos CLIs compilan con 0 errores también en `net6.0` |

Nota: una compilación muy antigua puede dejar de servirse por Microsoft en cualquier momento (Microsoft caduca/retira contenido de WU); la herramienta lo muestra como "no hay cargas útiles disponibles", no como un error de la herramienta.

---

## 9. Solución de problemas

| Síntoma | Causa probable / solución |
|---------|--------------------|
| "The specified temporary directory cannot be accessed or does not exist." (Linux) | Corregido en este fork: ya no se necesita `TEMP`, se usa `/tmp` como respaldo (respeta `TMPDIR`). Las compilaciones antiguas sin el arreglo necesitan `-t /ruta` o `TEMP` definido. |
| "mkisofs: command not found" | Instala `genisoimage` (Debian/Ubuntu) / `cdrtools` (macOS). |
| "Disk quota exceeded"/`ENOSPC` durante la conversión | El directorio temporal por defecto en Linux es `/tmp` (a menudo un disco RAM pequeño, p. ej. tmpfs). Pasa `-t` apuntando a un directorio en un sistema de archivos grande (una conversión de Windows 10 necesita varios GB). |
| Porcentajes de progreso del ISO por encima del 100% (`[CreatingISO][9997%]`) | Cosmético: mkisofs informa porcentajes por archivo; el ISO se crea igualmente y termina con `[Done]`. |
| `DllNotFoundException: WimgApi.dll` en Linux | Inusual en la ruta normal — Linux usa wimlib. Asegúrate de que `runtimes/linux-x64/native/libwim.so` esté junto a los binarios (así se entrega en la salida publicada). |
| `PlatformNotSupportedException` de `WimLibImaging` | Falta `libwim.so` de tu arquitectura en la salida. |
| Errores de `virtdisk.dll`/`DISM` en Linux | Pediste una conversión de varias ediciones/upgrade. No está soportada en Linux; usa una edición base única (`-e`). |
| El build del `.sln` completo falla en Linux | Esperado: `UUPSort`/`Playground`/`Flighting` son solo Windows. Compila los dos proyectos CLI. |
| La compilación heredada (`publish_win_legacy.cmd`) referencia `net10.0` | Asegúrate de que exista `Directory.Build.props` y de pasar `/p:UUPMediaTargetFramework=net6.0`; la propiedad debe llegar a los proyectos referenciados (no sobrescribas `TargetFramework` directamente, ya que no se propaga a las referencias de proyecto). |
| La app compilada con el script predeterminado no arranca en Windows 7 / 8.1 | La compilación predeterminada usa .NET 10 (Windows 10 1607+). Usa `publish_win_legacy.cmd` e instala los requisitos de Windows 7 (KB2999226, KB4474419, KB2533623). |
| No hay cargas útiles para una compilación muy antigua | Microsoft ya no sirve esa compilación; prueba una versión cercana o consulta `get-builds`. |

---

## 10. Referencias

- [Protocolo de Windows Update (FE3) de Microsoft — referencias de documentación antigua](https://github.com/gus33000/UUPMediaCreator)
- [Microsoft — Fin de soporte de Windows](https://www.microsoft.com/en-us/windows/end-of-support)
- [Microsoft — Windows 10 Consumer ESU](https://www.microsoft.com/en-us/windows/extended-security-updates)
- [Microsoft Tech Community — Prepare for Windows Update certificate rotation in 2027](https://techcommunity.microsoft.com/blog/windows-itpro-blog/prepare-for-windows-update-certificate-rotation-in-2027/4562463)