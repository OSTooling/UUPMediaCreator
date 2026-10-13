/*
 * Copyright (c) Gustave Monce and Contributors
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using UnifiedUpdatePlatform.Media.Creator;

namespace UUPMediaConverter
{
    internal static class DesktopConvert
    {
        public static string GetExecutableDirectory()
        {
            string fileName = Process.GetCurrentProcess().MainModule.FileName;
            return fileName.Contains(Path.DirectorySeparatorChar) ? string.Join(Path.DirectorySeparatorChar, fileName.Split(Path.DirectorySeparatorChar).AsEnumerable().Reverse().Skip(1).Reverse()) : "";
        }

        public static string GetParentExecutableDirectory()
        {
            string runningDirectory = GetExecutableDirectory();
            return runningDirectory.Contains(Path.DirectorySeparatorChar) ? string.Join(Path.DirectorySeparatorChar, runningDirectory.Split(Path.DirectorySeparatorChar).AsEnumerable().Reverse().Skip(1).Reverse()) : "";
        }

        public static int ProcessDesktopConvert(DesktopConvertOptions opt)
        {
            if (!string.IsNullOrEmpty(opt.UUPPath))
            {
                opt.UUPPath = Path.GetFullPath(opt.UUPPath);
            }

            if (!string.IsNullOrEmpty(opt.ISOPath))
            {
                opt.ISOPath = Path.GetFullPath(opt.ISOPath);
            }

            if (!string.IsNullOrEmpty(opt.TempPath))
            {
                opt.TempPath = Path.GetFullPath(opt.TempPath);
            }

            if (GetOperatingSystem() == OSPlatform.OSX)
            {
                Logging.Log("WARNING: For successful ISO creation, please install cdrtools via brew", Logging.LoggingLevel.Warning);
            }
            else if (GetOperatingSystem() == OSPlatform.Linux)
            {
                Logging.Log("WARNING: For successful ISO creation, please install genisoimage", Logging.LoggingLevel.Warning);
            }

            Logging.Log("WARNING: This tool does NOT currently integrate updates into the finished media file. Any UUP set with updates (KBXXXXX).MSU/.CAB will not have the update integrated.", severity: Logging.LoggingLevel.Warning);
            if (!IsAdministrator())
            {
                Logging.Log("WARNING: This tool is NOT currently running under Windows as administrator. The resulting image will be less clean/proper compared to Microsoft original.", severity: Logging.LoggingLevel.Warning);

                if (string.IsNullOrEmpty(opt.Edition))
                {
                    Logging.Log("WARNING: You are attempting to create an ISO media with potentially all editions available. Due to the tool not running under Windows as administrator, this request might not be fullfilled.", severity: Logging.LoggingLevel.Warning);
                }
            }
            else
            {
                string parentDirectory = GetParentExecutableDirectory();
                string toolpath = Path.Combine(parentDirectory, "Settings", "Settings.ini");

                if (!File.Exists(toolpath))
                {
                    parentDirectory = GetExecutableDirectory();
                    toolpath = Path.Combine(parentDirectory, "Settings", "Settings.ini");
                }

                if (!File.Exists(toolpath))
                {
                    parentDirectory = GetExecutableDirectory();
                    toolpath = Path.Combine(parentDirectory, "Settings.ini");
                }

                if (!File.Exists(toolpath))
                {
                    Logging.Log("ERROR: Could not find: " + toolpath, severity: Logging.LoggingLevel.Error);
                    return 1;
                }
            }

            int prevperc = -1;
            UnifiedUpdatePlatform.Common.Messaging.Common.ProcessPhase prevphase = UnifiedUpdatePlatform.Common.Messaging.Common.ProcessPhase.ReadingMetadata;
            string prevop = "";

            void callback(UnifiedUpdatePlatform.Common.Messaging.Common.ProcessPhase phase, bool IsIndeterminate, int ProgressInPercentage, string SubOperation)
            {
                if (phase == prevphase && prevperc == ProgressInPercentage && SubOperation == prevop)
                {
                    return;
                }

                prevphase = phase;
                prevop = SubOperation;
                prevperc = ProgressInPercentage;

                if (phase == UnifiedUpdatePlatform.Common.Messaging.Common.ProcessPhase.Error)
                {
                    Logging.Log("An error occured!", severity: Logging.LoggingLevel.Error);
                    Logging.Log(SubOperation, severity: Logging.LoggingLevel.Error);
                    if (Debugger.IsAttached)
                    {
                        _ = Console.ReadLine();
                    }

                    return;
                }
                string progress = IsIndeterminate ? "" : $"[{ProgressInPercentage}%]";
                Logging.Log($"[{phase}]{progress} {SubOperation}");
            }

            try
            {
                MediaCreator.CreateISOMedia(
                    opt.ISOPath,
                    opt.UUPPath,
                    opt.Edition,
                    opt.LanguageCode,
                    false,
                    opt.SuppressAnyKeyPrompt,
                    opt.Compression,
                    callback,
                    opt.TempPath);
            }
            catch (Exception ex)
            {
                Logging.Log("An error occured!", severity: Logging.LoggingLevel.Error);
                while (ex != null)
                {
                    Logging.Log(ex.Message, Logging.LoggingLevel.Error);
                    Logging.Log(ex.StackTrace, Logging.LoggingLevel.Error);
                    ex = ex.InnerException;
                }
                if (Debugger.IsAttached)
                {
                    _ = Console.ReadLine();
                }
                return 1;
            }

            return 0;
        }

        private static bool IsAdministrator()
        {
            if (GetOperatingSystem() == OSPlatform.Windows)
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            else
            {
                return false;
            }
        }

        public static OSPlatform GetOperatingSystem()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? OSPlatform.OSX
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                ? OSPlatform.Linux
                : RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? OSPlatform.Windows
                : RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD)
                ? OSPlatform.FreeBSD
                : throw new Exception("Cannot determine operating system!");
        }
    }
}