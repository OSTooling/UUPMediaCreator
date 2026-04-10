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
using Microsoft.Wim;
using System.Collections.Generic;
using UnifiedUpdatePlatform.Media.Creator.Utils;
using UnifiedUpdatePlatform.Services.Composition.Database;
using UnifiedUpdatePlatform.Services.Temp;

namespace UnifiedUpdatePlatform.Media.Creator.Installer
{
    public static class SetupMediaCreator
    {
        public static bool CreateSetupMedia(
            string UUPPath,
            string LanguageCode,
            string OutputMediaPath,
            string OutputWindowsREPath,
            Common.Messaging.Common.CompressionType CompressionType,
            IEnumerable<BaseManifest> CompositionDatabases,
            TempManager tempManager,
            ProgressCallback progressCallback = null)
        {
            bool result = true;
            string BaseESD = null;

            (result, BaseESD) = FileLocator.LocateFilesForSetupMediaCreation(UUPPath, LanguageCode, CompositionDatabases, progressCallback);
            if (!result)
            {
                goto exit;
            }

            WimCompressionType compression = WimCompressionType.None;
            switch (CompressionType)
            {
                case Common.Messaging.Common.CompressionType.LZMS:
                    compression = WimCompressionType.Lzms;
                    break;

                case Common.Messaging.Common.CompressionType.LZX:
                    compression = WimCompressionType.Lzx;
                    break;

                case Common.Messaging.Common.CompressionType.XPRESS:
                    compression = WimCompressionType.Xpress;
                    break;
            }

            //
            // Build installer
            //
            result = WindowsInstallerBuilder.BuildSetupMedia(BaseESD, OutputWindowsREPath, OutputMediaPath, compression, PlatformUtilities.RunsAsAdministrator, LanguageCode, tempManager, progressCallback);
            if (!result)
            {
                goto exit;
            }

        exit:
            return result;
        }
    }
}