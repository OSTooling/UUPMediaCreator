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

namespace UUPMediaConverter
{
    [Verb("desktop-convert", isDefault: true, HelpText = "Converts an UUP set to a Desktop ISO Media")]
    internal class DesktopConvertOptions
    {
        [Option('u', "uup-path", HelpText = "The path to the downloaded files from UUP (Unified Update Platform)", Required = true)]
        public string UUPPath
        {
            get; set;
        }

        [Option('i', "iso-path", HelpText = "The destination ISO file path (includes the filename)", Required = true)]
        public string ISOPath
        {
            get; set;
        }

        [Option('l', "language-code", HelpText = "The language to make media for. e.g. en-US", Required = true)]
        public string LanguageCode
        {
            get; set;
        }

        [Option('e', "edition", HelpText = "The edition to make media for, if possible. Not specifying this argument results in all possible editions being made, if possible.", Required = false)]
        public string Edition
        {
            get; set;
        }

        [Option('t', "temp-path", HelpText = "The temp path for the tool to use. Defaults to %TEMP%.", Required = false)]
        public string TempPath
        {
            get; set;
        }

        [Option('c', "compression-format", HelpText = "The compression format to use. Valid values are: XPRESS, LZX, LZMS", Required = false, Default = UnifiedUpdatePlatform.Common.Messaging.Common.CompressionType.LZX)]
        public UnifiedUpdatePlatform.Common.Messaging.Common.CompressionType Compression
        {
            get; set;
        }

        [Option("no-key-prompt", HelpText = "Use the alternate efisys and cdboot implementations to suppress the 'press any key' prompt", Required = false)]
        public bool SuppressAnyKeyPrompt
        {
            get; set;
        }
    }
}
