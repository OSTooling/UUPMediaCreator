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
using System.Collections.Generic;
using System.Threading;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads;

namespace UUPDownload.Downloading
{
    public class ReportProgress : IProgress<GeneralDownloadProgress>, IDisposable
    {
        private readonly Dictionary<string, FileStatus> files = [];

        private readonly Mutex mutex = new();

        private static string FormatBytes(double bytes)
        {
            string[] suffix = ["B", "KB", "MB", "GB", "TB"];
            int i;
            double dblSByte = bytes;
            for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return $"{dblSByte:0.##}{suffix[i]}";
        }

        private static string GetProgressBarString(int perc)
        {
            if (perc < 0)
            {
                perc = 0;
            }

            if (perc > 100)
            {
                perc = 100;
            }

            int eqsLength = (int)((double)perc / 100 * 55);
            string bases = new string('=', eqsLength) + new string(' ', 55 - eqsLength);
            bases = bases.Insert(28, perc + "%");
            if (perc == 100)
            {
                bases = bases[1..];
            }
            else if (perc < 10)
            {
                bases = bases.Insert(28, " ");
            }

            return "[" + bases + "]";
        }

        public void Dispose()
        {
            mutex.Dispose();
        }

        public void Report(GeneralDownloadProgress e)
        {
            _ = mutex.WaitOne();

            foreach (FileDownloadStatus status in e.DownloadedStatus)
            {
                if (status == null)
                {
                    continue;
                }

                bool shouldReport = !files.ContainsKey(status.File.FileName) || files[status.File.FileName] != status.FileStatus;

                if (!shouldReport)
                {
                    continue;
                }

                files[status.File.FileName] = status.FileStatus;

                string msg = "U";

                switch (status.FileStatus)
                {
                    case FileStatus.Completed:
                        msg = "C";
                        break;
                    case FileStatus.Downloading:
                        msg = "D";
                        break;
                    case FileStatus.Expired:
                        msg = "E";
                        break;
                    case FileStatus.Failed:
                        msg = "F";
                        break;
                    case FileStatus.Verifying:
                        msg = "V";
                        break;
                }

                Console.WriteLine($"{DateTime.Now:'['HH':'mm':'ss']'}[{e.NumFilesDownloadedSuccessfully}/{e.NumFiles}][E:{e.NumFilesDownloadedUnsuccessfully}][{msg}] {status.File.FileName} ({FormatBytes(status.File.FileSize)})");
            }

            mutex.ReleaseMutex();
        }
    }
}
