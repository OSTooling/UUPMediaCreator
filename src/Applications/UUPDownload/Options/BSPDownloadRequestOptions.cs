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
    [Verb("request-bsp-download", HelpText = "Request a BSP download from zero using a number of different request parameters.")]
    internal class BSPDownloadRequestOptions
    {
        [Option('s', "reporting-sku", HelpText = "The sku to report to the Windows Update servers. Example: Professional", Required = true)]
        public OSSkuId ReportingSku
        {
            get; set;
        }

        [Option('v', "reporting-version", HelpText = "The version to report to the Windows Update servers. Example: 10.0.20152.1000", Required = true)]
        public string ReportingVersion
        {
            get; set;
        }

        [Option('t', "machine-type", HelpText = "The architecture to report to the Windows Update servers. Example: amd64", Required = true)]
        public MachineType MachineType
        {
            get; set;
        }

        [Option('r', "flight-ring", HelpText = "The ring to report to the Windows Update servers. Example: Retail, other example: External or Internal", Required = true)]
        public string FlightRing
        {
            get; set;
        }

        [Option('c', "current-branch", HelpText = "The branch to report to the Windows Update servers. Example: 19h1_release", Required = true)]
        public string CurrentBranch
        {
            get; set;
        }

        [Option('b', "flighting-branch-name", HelpText = "The flighting branch name to report to the Windows Update servers. Example: Retail, other example: CanaryChannel, Dev, Beta or ReleasePreview", Required = false, Default = "")]
        public string FlightingBranchName
        {
            get; set;
        }

        [Option('y', "sync-current-version-only", HelpText = "Only get updates for the current version, enables getting cumulative updates.", Required = false, Default = false)]
        public bool SyncCurrentVersionOnly
        {
            get; set;
        }

        [Option('a', "branch-readiness-level", HelpText = "The branch readiness level to report to the Windows Update servers. Example: CB", Required = false, Default = "CB")]
        public string BranchReadinessLevel
        {
            get; set;
        }

        [Option('o', "output-folder", HelpText = "The folder to use for downloading the update files.", Required = false, Default = ".")]
        public string OutputFolder
        {
            get; set;
        }

        [Option('z', "releasetype", HelpText = "The release type to report to the Windows Update servers. Example: Production", Required = false, Default = "Production")]
        public string ReleaseType
        {
            get; set;
        }

        [Option('n', "contenttype", HelpText = "The content type to report to the Windows Update servers. Example: Mainline, Custom", Required = false, Default = "Mainline")]
        public string ContentType
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

        // Used above:
        // a
        // b
        // c
        // m
        // n
        // o
        // p
        // r
        // s
        // t
        // v
        // y
        // z

        [Option('d', "bsp-product-version", HelpText = "", Required = false, Default = "0.0.0.0")]
        public string BSPProductVersion
        {
            get; set;
        }

        //
        // Targeting for the BSP Product, see the ComputerHardwareIds.Windows project for knowing what these map to, programmatically
        //

        [Option('e', "targeting-manufacturer", HelpText = "", Required = true)]
        public string Manufacturer
        {
            get; set;
        }

        [Option('f', "targeting-family", HelpText = "", Required = false, Default = null)]
        public string Family
        {
            get; set;
        }

        [Option('g', "targeting-productname", HelpText = "", Required = false, Default = null)]
        public string ProductName
        {
            get; set;
        }

        [Option('h', "targeting-skunumber", HelpText = "", Required = false, Default = null)]
        public string SKUNumber
        {
            get; set;
        }

        [Option('i', "targeting-biosvendor", HelpText = "", Required = false, Default = null)]
        public string BIOSVendor
        {
            get; set;
        }

        [Option('j', "targeting-baseboardmanufacturer", HelpText = "", Required = false, Default = null)]
        public string BaseboardManufacturer
        {
            get; set;
        }

        [Option('k', "targeting-baseboardproduct", HelpText = "", Required = false, Default = null)]
        public string BaseboardProduct
        {
            get; set;
        }

        [Option('l', "targeting-enclosuretype", HelpText = "", Required = false, Default = null)]
        public string EnclosureType
        {
            get; set;
        }

        [Option('q', "targeting-biosversion", HelpText = "", Required = false, Default = null)]
        public string BIOSVersion
        {
            get; set;
        }

        [Option('u', "targeting-biosmajorrelease", HelpText = "", Required = false, Default = null)]
        public string BIOSMajorRelease
        {
            get; set;
        }

        [Option('w', "targeting-biosminorrelease", HelpText = "", Required = false, Default = null)]
        public string BIOSMinorRelease
        {
            get; set;
        }
    }
}
