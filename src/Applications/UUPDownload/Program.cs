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
using CommandLine;
using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using UUPDownload.Options;

namespace UUPDownload
{
    internal static class Program
    {
        private static void PrintLogo()
        {
            Logging.Log($"UnifiedUpdatePlatform.Media.Download {Assembly.GetExecutingAssembly().GetName().Version} - Download from the Microsoft Unified Update Platform");
            Logging.Log("Copyright (c) Gustave Monce and Contributors");
            Logging.Log("https://github.com/gus33000/UUPMediaCreator");
            Logging.Log("");
            Logging.Log("This program comes with ABSOLUTELY NO WARRANTY.");
            Logging.Log("This is free software, and you are welcome to redistribute it under certain conditions.");
            Logging.Log("");
            Logging.Log("This software contains work derived from libmspack licensed under the LGPL-2.1 license.");
            Logging.Log("(C) 2003-2004 Stuart Caie.");
            Logging.Log("(C) 2011 Ali Scissons.");
            Logging.Log("");
        }

        private static int WrapAction(Action a)
        {
            try
            {
                a();
            }
            catch (Exception ex)
            {
                Logging.Log("Something happened.", Logging.LoggingLevel.Error);
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

        private static int Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            return Parser.Default.ParseArguments<DownloadRequestOptions, BSPDownloadRequestOptions, BSPDownloadRequestOptions2, BSPDownloadRequestOptions3, DownloadReplayOptions, GetBuildsOptions>(args).MapResult(
              (DownloadRequestOptions opts) =>
              {
                  PrintLogo();
                  return WrapAction(() => DownloadRequest.Process.ParseDownloadOptions(opts));
              },
              (BSPDownloadRequestOptions opts) =>
              {
                  PrintLogo();
                  return WrapAction(() => DownloadRequest.Drivers.ProcessDrivers.ParseDownloadOptions(opts));
              },
              (BSPDownloadRequestOptions2 opts) =>
              {
                  PrintLogo();
                  return WrapAction(() => DownloadRequest.ReferenceDriversRepo.ProcessDrivers.ParseDownloadOptions(opts));
              },
              (BSPDownloadRequestOptions3 opts) =>
              {
                  PrintLogo();
                  return WrapAction(() => DownloadRequest.DriversAuto.ProcessDrivers.ParseDownloadOptions(opts));
              },
              (DownloadReplayOptions opts) =>
              {
                  PrintLogo();
                  return WrapAction(() => DownloadRequest.Process.ParseReplayOptions(opts));
              },
              (GetBuildsOptions opts) =>
              {
                  PrintLogo();
                  return WrapAction(() => RingCheck.ParseGetBuildsOptions(opts));
              },
              errs => 1);
        }
    }
}