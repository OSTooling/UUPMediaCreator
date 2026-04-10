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
using UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting;

namespace UUPDownload.Options
{
    [Verb("replay-download", isDefault: false, HelpText = "Replay a download from zero using a *.uupmcreplay file.")]
    internal class DownloadReplayOptions
    {
        [Option('r', "replay-metadata", HelpText = @"The path to a *.uupmcreplay file to replay an older update and resume the download process. Example: D:\20236.1005.uupmcreplay", Required = true)]
        public string ReplayMetadata
        {
            get; set;
        }

        [Option('t', "machine-type", HelpText = "The architecture to report to the Windows Update servers. Example: amd64", Required = true)]
        public MachineType MachineType
        {
            get; set;
        }

        [Option('o', "output-folder", HelpText = "The folder to use for downloading the update files.", Required = false, Default = ".")]
        public string OutputFolder
        {
            get; set;
        }

        [Option('e', "edition", HelpText = "The edition to get. Must be used with the language parameter. Omit either of these to download everything. Example: Professional", Required = false, Default = "")]
        public string Edition
        {
            get; set;
        }

        [Option('l', "language", HelpText = "The language to get. Must be used with the edition parameter. Omit either of these to download everything. Example: en-US", Required = false, Default = "")]
        public string Language
        {
            get; set;
        }

        [Option("fixup", HelpText = @"Applies a fixup to files in output folder. Example: Appx", Required = false)]
        public Fixup? Fixup
        {
            get; set;
        }

        [Option("appxroot", HelpText = @"The folder containing the appx files for use with the Appx fixup", Required = false)]
        public string AppxRoot
        {
            get; set;
        }

        [Option("cabsroot", HelpText = @"The folder containing the cab files for use with the Appx fixup", Required = false)]
        public string CabsRoot
        {
            get; set;
        }
    }
}
