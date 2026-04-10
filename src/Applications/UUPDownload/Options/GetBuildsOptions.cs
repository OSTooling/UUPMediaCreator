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
    [Verb("get-builds", isDefault: false, HelpText = "Get builds in all rings matching the request type")]
    internal class GetBuildsOptions
    {
        [Option('s', "reporting-sku", HelpText = "The sku to report to the Windows Update servers. Example: Professional", Required = true)]
        public OSSkuId ReportingSku
        {
            get; set;
        }

        [Option('t', "machine-type", HelpText = "The architecture to report to the Windows Update servers. Example: amd64", Required = true)]
        public MachineType MachineType
        {
            get; set;
        }

        [Option('m', "mail", HelpText = "Email for the Windows Insider account to use to generate authorization tokens (Optional)", Required = false, Default = "")]
        public string Mail
        {
            get; set;
        }

        [Option('p', "password", HelpText = "Password for the Windows Insider account to use to generate authorization tokens (If 2FA, must be generated app password) (Optional)", Required = false, Default = "")]
        public string Password
        {
            get; set;
        }

        [Option("preview-targeting-attribute", HelpText = "The name of the set of targeting attributes to use. (Optional, Preview)", Required = false, Default = "")]
        public string TargetingAttribute
        {
            get; set;
        }
    }
}
