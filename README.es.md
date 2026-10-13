<p align="center">
  <img src="Assets/logo.png" width="176"><br>
  <b>Una utilidad para convertir conjuntos de archivos de la Plataforma Unificada de Actualizaciones (UUP) en archivos de medios de Windows</b><br>
  <a href="./src">Código fuente</a> |
  <a href="./docs">Documentación</a> |
  <a href="./thirdparty">Bibliotecas de terceros</a> |
  <a href="https://github.com/BENDER35/UUPMediaCreator-lin/releases">Descargas</a>
  <br><br>
</p>

> 🌐 **Idiomas / Languages:** [English](./README.md) · Español (este archivo)
> 📖 **Guía técnica / Technical guide:** [Español](./docs/GUIA_TECNICA.md) · [English](./docs/TECHNICAL_GUIDE.md)

## Readme

UUP Media Creator es un conjunto de herramientas diseñadas para ayudarte a escanear y descargar actualizaciones desde la Plataforma Unificada de Actualizaciones (UUP) de Microsoft, y para permitirte crear medios a partir de ellas cuando no exista una herramienta adecuada en el mundo.

Actualmente las herramientas incluyen:

- UUPDownload: te permite escanear, obtener y descargar actualizaciones desde UUP (Escritorio de Windows, Windows 10X, Windows Holographic, Windows Server, Windows Mobile, Windows IoT, etc...)
- UUPMediaConverter: te permite convertir una actualización UUP descargada de Windows Desktop en una imagen ISO utilizable, para un lector de DVD de la última década o simplemente para montarla.

> ℹ️ **Sobre este fork:** este repositorio es un fork enfocado en Linux. Ambas herramientas de línea de comandos se compilan y verifican para funcionar **en Linux** (x64/arm64), Windows y macOS, con especial atención a poder descargar **versiones antiguas de Windows** (era de Windows 7/8.1/10). Consulta [Dependencias](#dependencias) y [Ciclo de vida de soporte de Windows](#ciclo-de-vida-de-soporte-de-windows-10).

## Dependencias

Las herramientas son aplicaciones .NET, por lo que se requiere un runtime de .NET. Durante el desarrollo se usa el SDK de .NET **10**, y los binarios se publican para las plataformas indicadas abajo. Cuando ejecutes desde el código fuente (o una compilación dependiente del framework) instala:

| Plataforma | Requisito |
|----------|----------|
| **Windows** | Runtime de .NET 10 (o SDK). Para crear ISOs de edición base **no se necesitan privilegios de administrador**. |
| **Linux** | Runtime de .NET 10 (o SDK) + `genisoimage` (proporciona `mkisofs`) para la creación de ISOs + `libwim` (wimlib) — el `libwim.so` nativo para Linux se **incluye** en los binarios publicados. |

Comandos de instalación:

```bash
# Ubuntu / Debian
sudo apt-get install genisoimage
sudo apt-get install dotnet-runtime-10.0   # o el SDK de .NET 10
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
brew install cdrtools   # proporciona mkisofs/genisoimage
```

> **Nota sobre `libfuse2`:** las versiones antiguas de este proyecto indicaban `libfuse2` para el montaje de VHD en Linux. Este fork **no** contiene código de montaje VHD/FUSE para Linux (el montaje de discos virtuales es solo de Windows), por lo que `libfuse2` **no es necesario** en Linux.

### Publicar para Linux

Se proporciona un script de conveniencia:

```bash
./publish_linux.sh            # linux-x64
./publish_linux.sh arm64      # linux-arm64
```

Publica tanto `UUPDownload` como `UUPMediaConverter` como ejecutables de archivo único en `publish/artifacts/linux-<arch>/CLI`.

### Publicar para Windows

En Windows se proporciona un script `.cmd` por arquitectura. Solo necesitan el **SDK de .NET 10** (sin Visual Studio); cada uno es un envoltorio fino sobre el ayudante genérico `publish.cmd`:

| Script | Arquitectura | Destino habitual |
|--------|--------------|------------------|
| `publish_win_x64.cmd` | 64 bits | Windows 10 1607+ / Windows 11 |
| `publish_win_x86.cmd` | 32 bits | Windows 10 1607+ |
| `publish_win_arm64.cmd` | ARM64 | Windows 11 |
| `publish_win_legacy.cmd` | 32 bits **y** 64 bits | **Windows 7 SP1 / 8 / 8.1** y posteriores |

Cada script genera ejecutables de archivo único autocontenidos:

- `publish_win_x64.cmd`, `publish_win_x86.cmd` y `publish_win_arm64.cmd` → `publish\artifacts\win-<arch>\CLI`
- `publish_win_legacy.cmd` → `publish\artifacts\win-legacy-x86\CLI` y `publish\artifacts\win-legacy-x64\CLI`

Para cualquier otro runtime, llama directamente al ayudante: `publish.cmd <rid> [tfm]`; por ejemplo `publish.cmd linux-x64`, `publish.cmd osx-arm64` o `publish.cmd win-x64 net6.0`.

#### Compilación heredada (Windows antiguo)

La compilación predeterminada usa **.NET 10**, que **solo funciona en Windows 10 1607 o posterior**. Para ejecutarla en **Windows 7 SP1, 8 u 8.1**, `publish_win_legacy.cmd` usa **.NET 6** (el último runtime de .NET que funciona en Windows 7 SP1) y genera ejecutables autocontenidos, por lo que **no hace falta instalar ningún runtime de .NET en la máquina de destino**.

La máquina de destino sigue necesitando los requisitos habituales de Windows 7: el Universal C Runtime (**KB2999226**), el soporte de firma de código SHA-2 (**KB4474419**) y **KB2533623**.

> **Nota:** .NET 6 ya no tiene soporte (sus correcciones de seguridad finalizaron en noviembre de 2024). La compilación heredada existe únicamente para poder ejecutar la herramienta en sistemas operativos que .NET 10 ya no admite; siempre que sea posible, usa la compilación predeterminada (.NET 10).

## Ciclo de vida de soporte de Windows 10

Esta herramienta puede descargar y crear medios para **Windows 10 y Windows 11**, incluidas versiones antiguas de Windows 10 (1709, 1803, 1809, 1903...). Por ello, debes conocer el estado actual de soporte de los sistemas operativos que vayas a desplegar:

| Producto | Fin de soporte ordinario | Fin de las Extended Security Updates (ESU) |
|---------|-----------------------------------|-------------------------------------|
| Windows 10 (Home / Pro / todas las ediciones minoristas) | **14 de octubre de 2025** | **12 de octubre de 2027** (ESU de consumo, incluye la inscripción hasta esa fecha) · hasta **octubre de 2028** para las ESU comerciales (anuales, de pago) |
| Windows 8.1 | 10 de enero de 2023 | 10 de enero de 2026 (ESU finalizadas) |
| Windows 7 | 14 de enero de 2020 | 10 de enero de 2023 (ESU finalizadas) |
| Windows Server 2012 / 2012 R2 | 10 de octubre de 2023 | 13 de octubre de 2026 (ESU finalizadas) |

Fuentes: [Microsoft – Fin de soporte de Windows](https://www.microsoft.com/en-us/windows/end-of-support), [Microsoft – Windows 10 Consumer ESU](https://www.microsoft.com/en-us/windows/extended-security-updates).

## ⚠️ Rotación de certificados de Windows Update (2027) — lee antes de desplegar

Microsoft está **rotando los certificados raíz** utilizados para establecer conexiones de confianza con Windows Update. Algunos de estos certificados expiran el **17 de mayo de 2027** y el **19 de junio de 2027**.

- Los dispositivos con versiones de Windows compatibles que sigan recibiendo actualizaciones mensuales obtendrán los nuevos certificados automáticamente y **no** se verán afectados.
- Los dispositivos con **Windows 7, Windows 8.1 y Windows 10 sin ESU** (o cualquier compilación ya sin soporte) que **no puedan recibir los nuevos certificados raíz** **dejarán de conectarse a Windows Update** y dejarán de recibir actualizaciones por completo.
- Si vas a desplegar o mantener medios antiguos de Windows con esta herramienta, planifica en consecuencia: inscribe el equipo en ESU (Windows 8/10), migra a una versión de Windows compatible, o aplica la actualización manual de certificados raíz de confianza **antes de las fechas límite de mayo/junio de 2027**.

Fuente: [Microsoft Tech Community – Prepare for Windows Update certificate rotation in 2027](https://techcommunity.microsoft.com/blog/windows-itpro-blog/prepare-for-windows-update-certificate-rotation-in-2027/4562463)

## Funciones admitidas por sistema operativo

| Función                                                                                  | Windows | Linux (3) | macOS  |
|------------------------------------------------------------------------------------------|---------|--------|--------|
| Descargar archivos de UUP (UUPDownload)                                                 | ✅       | ✅       | ✅       |
| Reanudar descargas anteriores de UUP (UUPDownload)                                          | ✅       | ✅       | ✅       |
| Descifrar cargas útiles ESRP de UUP (UUPDownload)                                          | ✅       | ✅       | ✅       |
| Verificar cargas útiles descargadas de UUP (UUPDownload)                                     | ✅       | ✅       | ✅       |
| Mostrar compilaciones disponibles de UUP (parámetro get-builds) (UUPDownload)                     | ✅       | ✅       | ✅       |
| Convertir archivos UUP de escritorio a un ISO de una edición base (UUPMediaConverter)          | ✅       | ✅       | ✅       |
| No requiere privilegios administrativos para imágenes ISO de edición base                   | ✅       | ✅       | ✅       |
| CLI disponible                                                                            | ✅       | ✅       | ✅       |
| Soporte ARM64 (aka funciona en el dispositivo de @sinclairinat0r (TM))                                 | ✅       | ✅       | ✅       |
| Convertir archivos UUP de escritorio a un ISO de todas las ediciones posibles (UUPMediaConverter)     | ✅       | ❌       | ❌       |
| Entorno de preinstalación ISO cercano al original (UUPMediaConverter)                    | ✅       | ❌       | ❌       |
| ISO creada con CDImage Mastering Utility con UDF y sector de arranque (UUPMediaConverter)      | ✅       | ❌       | ❌       |
| ISO creada con mkisofs con UDF y sector de arranque (UUPMediaConverter)                        | ❌       | ✅   (1) | ✅   (2) |
| Integración de actualizaciones de archivos (UUPMediaConverter)                                              | ❌       | ❌       | ❌       |
| Crear imágenes Hololens, Mobile, 10X, IoT                                               | ❌       | ❌       | ❌       |
| UUPDownload de fácil uso (vendrá pronto, lo prometemos)                                     | ❌       | ❌       | ❌       |

(1): requiere ```sudo apt-get install genisoimage```

(2): requiere ```brew install cdrtools```

(3): los binarios publicados son ejecutables autocontenidos de archivo único, por lo que **no** se requiere el runtime de .NET. Se incluye el ```libwim.so``` (wimlib) que se usa para crear la ISO. `libfuse2` **no** es necesario en este fork.

## Uso

UUPDownload puede ser un poco complicado de usar para algunas personas. Como referencia, aquí está el conjunto actual de parámetros que puedes utilizar:

| Canal                               | Comando |
|---------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Retail (Windows 10, Escritorio)          | -s Professional -v 10.0.19043.1 -r Retail -b Retail -c vb_release -t [tu arquitectura] |
| Retail (Windows 10 ESU, Escritorio)      | -s Professional -v 10.0.19045.1 -r Retail -b Retail -c vb_release -u -t [tu arquitectura] |
| Retail (Windows 11, Escritorio)          | -s Professional -v 10.0.26100.1 -r Retail -b Retail -c ge_release -t [tu arquitectura] |
| Retail (IoT)                          | -s IoTUAP -v 10.0.17763.1 -r Retail -b Retail -c rs5_release -t [tu arquitectura] |
| Retail (Holographic)                  | -s Holographic -v 10.0.20346.1 -r Retail -b Retail -c fe_release -t [tu arquitectura] |
| Retail (Mobile)                       | -s MobileCore -v 10.0.15254.1 -r Retail -b Retail -c feature2 -t [tu arquitectura] |
| Retail (Team)                         | -s PPIPro -v 10.0.19043.1 -r Retail -b Retail -c vb_release -t [tu arquitectura] |
| Retail (Server)                       | -s DatacenterServer -v 10.0.20348.1 -r Retail -b Retail -c fe_release -t [tu arquitectura] |
| Release Preview (Windows 10, Escritorio) | -s Professional -v 10.0.19043.1 -r External -b ReleasePreview -c vb_release -t [tu arquitectura] |
| Release Preview (Windows 11, Escritorio) | -s Professional -v 10.0.26100.1 -r External -b ReleasePreview -c ge_release -t [tu arquitectura] |
| Release Preview (HCI)                 | -s AzureStackHCIServerCore -v 10.0.22000.1 -r External -b ReleasePreview -c rs_prerelease -t [tu arquitectura] |
| Beta (Windows 10, Escritorio)            | -s Professional -v 10.0.19045.1 -r External -b Beta -c vb_release -t [tu arquitectura] |
| Beta (Windows 11, Escritorio)            | -s Professional -v 10.0.26100.1 -r External -b Beta -c ge_release -t [tu arquitectura] |
| Beta (Holographic)                    | -s Holographic -v 10.0.19043.1 -r External -b Beta -c vb_release -t [tu arquitectura] |
| Beta (Team)                           | -s PPIPro -v 10.0.19043.1 -r External -b Beta -c vb_release -t [tu arquitectura] |
| Dev (Escritorio)                         | -s Professional -v 10.0.26100.1 -r External -b Dev -c rs_prerelease -t [tu arquitectura] |
| Dev (Holographic)                     | -s Holographic -v 10.0.19043.1 -r External -b Dev -c vb_release -t [tu arquitectura] |
| Dev (Team)                            | -s PPIPro -v 10.0.19043.1 -r External -b Dev -c vb_release -t [tu arquitectura] |
| Canary (Escritorio)                      | -s Professional -v 10.0.26100.1 -r External -b CanaryChannel -c rs_prerelease -t [tu arquitectura] |

> **Ramas de Windows 11 (2026):** Windows 11 se sirve ahora desde la rama **GE (Germanium)**. Usa `-c ge_release` para **Retail**, **Release Preview** y **Beta** de Windows 11; y `-c rs_prerelease` para **Dev** y **Canary**. La antigua rama `ni_release` (21H2/22H2/23H2) ya no se sirve en los canales Beta/Release Preview, por lo que usarla junto con `-y` devuelve *"No updates found"*. Verificado contra los servidores reales: Beta = `10.0.26220.9606 (ge_release_svc_betaflt_prod1)`, Dev = `10.0.26340.9616 (ge_prerelease_im)`, Canary = `10.0.28020.2207`.

> **Beta de Windows 10:** Microsoft todavía sirve el canal **Beta** de Windows 10 — responde con la compilación más nueva de Windows 10 22H2 (`10.0.19045.6396`, `vb_release_svc_prod3.250923-1751`). Usa `-s Professional -v 10.0.19045.1 -r External -b Beta -c vb_release -t [tu arquitectura]` **sin `-y`** para obtener siempre la última compilación Beta disponible. El canal **Dev** de Windows 10 ahora devuelve compilaciones de Windows 11.

> **Windows 10 ESU (Extended Security Updates):** Windows 10 22H2 llegó al fin de soporte el **14-10-2025**; las actualizaciones mensuales publicadas después solo se ofrecen a dispositivos inscritos en el programa ESU. Añade `-u` (`--esu`) a `request-download` — o usa la entrada **`Retail (Windows 10 ESU)`** de `get-builds` — para reportar el dispositivo como inscrito en ESU. Los servidores entregarán entonces la compilación ESU, p. ej. `10.0.19045.7727 (vb_release_svc_im.260912-0526)`, para **x86**, **amd64** y **arm64**. Ejemplo: `-s Professional -v 10.0.19045.1 -r Retail -b Retail -c vb_release -u -e Professional -l en-US`. Sin `-u` solo obtienes la última compilación 22H2 normal (`10.0.19045.6456`).

> **Fiabilidad:** los servidores de Windows Update a veces responden con un conjunto vacío cuando se hacen muchas peticiones seguidas. `UUPDownload` ahora espera y **reintenta una vez** antes de informar de *"No updates found"*, tanto en `get-builds` como en `request-download`.

> **Descargar versiones antiguas de Windows 10:** el conjunto UUP más antiguo que Microsoft sigue sirviendo es **Windows 10 1709 (RS3, `rs3_release`)**, luego 1803 (`rs4_release`), 1809 (`rs5_release`), 1903 (`19h1_release`) y 22H2 (`vb_release`). **Windows 10 1703 (RS2, `rs2_release`) y anteriores (1607, 1511, 1507) ya no están disponibles** — Microsoft los ha retirado de los servidores. Usa el verbo `get-builds` para ver qué se ofrece actualmente y `-y` para fijar una versión exacta, p. ej. 1809: `-s Professional -v 10.0.17763.1217 -r Retail -b Retail -c rs5_release -y -e Professional -l en-US`.

> **ISOs de Windows Insider Preview:** la [página oficial de descarga de Insider Preview](https://www.microsoft.com/software-download/windowsinsiderpreviewiso) de Microsoft ahora solo ofrece **Windows 11** (las ISOs Insider de Windows 10 se eliminaron). Para obtener medios de Windows 10 —incluidas versiones antiguas— usa `UUPDownload` como se muestra arriba.

**CONSEJO 1:** Puedes añadir ```-e [Edición a descargar]``` para obtener solo los archivos necesarios para una edición específica

**CONSEJO 2:** Puedes añadir ```-l [Idioma a descargar]``` para obtener solo los archivos necesarios para un idioma específico

### Importante

El parámetro ```-v``` __no__ se usa para especificar qué versión de Windows quieres descargar. Los parámetros que usa UUPDownload arriba se usan para decirle a Windows Update **qué** versión de Windows estás ejecutando actualmente. Por lo tanto, ```-v``` se usa para decirle a Windows Update tu versión actual de Windows. Nunca obtendrás versiones inferiores al valor especificado en ```-v``` y algunas solicitudes tampoco te darán la versión indicada en ```-v```, sino solo versiones más nuevas. Si, sin embargo, quieres descargar una versión muy específica con ```-v``` __entonces__ debes usar la opción -y para indicar a Windows Update que solo quieres esta versión.

## Cómo contribuir

Eres libre de contribuir a este proyecto; de hecho, agradeceríamos cualquier tipo de ayuda. Pero por favor ten en cuenta los siguientes puntos si lo haces:

- Intenta mantener tu PR con un conjunto razonable de commits
- Intenta no cambiar funcionalidades ajenas a tu PR
- Evita cambiar toda la aplicación; si crees que es necesario, abre un issue para que lo discutamos entre colaboradores.

## Errores

Este proyecto, como todo en ciencias de la computación, no está libre de errores. Si encuentras algo mal, por favor abre un issue en el rastreador. Incluye la versión de la herramienta, así como el sistema operativo en el que la ejecutas y cualquier otro detalle que ayude a reproducir el problema.