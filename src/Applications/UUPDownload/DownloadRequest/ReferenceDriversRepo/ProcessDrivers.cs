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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Services.Composition.Database;
using UnifiedUpdatePlatform.Services.WindowsUpdate;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting;
using UUPDownload.Downloading;
using UUPDownload.Options;

namespace UUPDownload.DownloadRequest.ReferenceDriversRepo
{
    public partial class ProcessDrivers
    {
        internal static void ParseDownloadOptions(BSPDownloadRequestOptions2 opts)
        {
            CheckAndDownloadUpdates(
                opts.ReportingSku,
                opts.ReportingVersion,
                opts.MachineType,
                opts.FlightRing,
                opts.FlightingBranchName,
                opts.BranchReadinessLevel,
                opts.CurrentBranch,
                opts.ReleaseType,
                opts.SyncCurrentVersionOnly,
                opts.ContentType,
                opts.Mail,
                opts.Password,
                opts.OutputFolder,
                opts.Language,
                opts.Edition).Wait();
        }

        private static async Task CheckAndDownloadUpdates(OSSkuId ReportingSku,
                    string ReportingVersion,
                    MachineType MachineType,
                    string FlightRing,
                    string FlightingBranchName,
                    string BranchReadinessLevel,
                    string CurrentBranch,
                    string ReleaseType,
                    bool SyncCurrentVersionOnly,
                    string ContentType,
                    string Mail,
                    string Password,
                    string OutputFolder,
                    string Language,
                    string Edition)
        {
            if (!File.Exists("DriverConfig.json"))
            {
                Logging.Log("Driver Configuration file (DriverConfig.json) could not be found in the current working directory.", Logging.LoggingLevel.Error);
                return;
            }

            DriverPlan[] plans = System.Text.Json.JsonSerializer.Deserialize<DriverPlan[]>(File.ReadAllText("DriverConfig.json"));

            foreach (DriverPlan plan in plans)
            {
                Logging.Log(plan.outputFolder);
                await ProcessDriverPlan(plan, ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, ContentType, Mail, Password, Language, Edition, OutputFolder);
            }

            if (Debugger.IsAttached)
            {
                _ = Console.ReadLine();
            }

            Logging.Log("Completed.");
        }

        private static async Task ProcessDriverPlan(DriverPlan DriverPlan,
                    OSSkuId ReportingSku,
                    string ReportingVersion,
                    MachineType MachineType,
                    string FlightRing,
                    string FlightingBranchName,
                    string BranchReadinessLevel,
                    string CurrentBranch,
                    string ReleaseType,
                    bool SyncCurrentVersionOnly,
                    string ContentType,
                    string Mail,
                    string Password,
                    string Language,
                    string Edition,
                    string RepoLocation)
        {
            BaseManifest previousCompositionDatabase = null;

            Logging.Log("Checking for updates...");

            CTAC NewestDriverProductCTAC = new(ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, false, ContentType: ContentType, IsDriverCheck: true);
            string token = string.Empty;
            if (!string.IsNullOrEmpty(Mail) && !string.IsNullOrEmpty(Password))
            {
                token = await MBIHelper.GenerateMicrosoftAccountTokenAsync(Mail, Password);
            }

            string ProductGUID = DriverPlan.guid;
            if (string.IsNullOrEmpty(ProductGUID))
            {
                ProductGUID = ComputerHardwareID.GenerateHardwareId5(DriverPlan.Manufacturer, DriverPlan.Family, DriverPlan.Product, DriverPlan.Sku);
            }

            NewestDriverProductCTAC.Products += $"PN={ProductGUID}_{MachineType}&V=0.0.0.0&Source=SMBIOS;";

            IEnumerable<UpdateData> NewestDriverProductUpdateData = await FE3Handler.GetUpdates(null, NewestDriverProductCTAC, token, FileExchangeV3UpdateFilter.ProductRelease);

            string outputFolder = (RepoLocation + DriverPlan.outputFolder).Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

            if (!NewestDriverProductUpdateData.Any())
            {
                Logging.Log("No updates found that matched the specified criteria.", Logging.LoggingLevel.Error);
                return;
            }

            string newestDriverVersion = "0.0.0.0";

            for (int i = 0; i < NewestDriverProductUpdateData.Count(); i++)
            {
                UpdateData update = NewestDriverProductUpdateData.ElementAt(i);

                if (update.Xml.LocalizedProperties.Title.Contains("Windows"))
                {
                    continue;
                }

                Logging.Log($"{i}: Title: {update.Xml.LocalizedProperties.Title}");
                Logging.Log($"{i}: Description: {update.Xml.LocalizedProperties.Description}");

                Logging.Log("Gathering update metadata...");

                HashSet<BaseManifest> compDBs = await update.GetCompDBsAsync();

                newestDriverVersion = compDBs.First().UUPProductVersion;
            }

            if (SyncCurrentVersionOnly)
            {
                foreach (UpdateData update in NewestDriverProductUpdateData)
                {
                    if (update.Xml.LocalizedProperties.Title.Contains("Windows"))
                    {
                        continue;
                    }

                    Logging.Log("Title: " + update.Xml.LocalizedProperties.Title);
                    Logging.Log("Description: " + update.Xml.LocalizedProperties.Description);

                    _ = await UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads.UpdateUtils.ProcessUpdateAsync(update, outputFolder, MachineType, new ReportProgress(), Language: Language, Edition: Edition);
                }
            }
            else
            {
                string newestDriverOutput = $"{outputFolder}{Path.DirectorySeparatorChar}{newestDriverVersion}";
                if (Directory.Exists(newestDriverOutput) && Directory.EnumerateFiles(newestDriverOutput).Any())
                {
                    return;
                }

                string changelogOutput = $"{outputFolder}{Path.DirectorySeparatorChar}CHANGELOG.md";

                if (File.Exists(changelogOutput))
                {
                    File.Delete(changelogOutput);
                }

                int newestDriverBuildNumber = int.Parse(newestDriverVersion.Split(".")[2]);

                for (int driverBuildNumber = 0; driverBuildNumber <= newestDriverBuildNumber; driverBuildNumber++)
                {
                    if (DriverPlan.filteredIds.Length != 0 && !DriverPlan.filteredIds.Contains(driverBuildNumber))
                    {
                        continue;
                    }

                    if (DriverPlan.excludedIds.Length != 0 && DriverPlan.excludedIds.Contains(driverBuildNumber))
                    {
                        continue;
                    }

                    CTAC PreciseDriverProductVersionCTAC = new(ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, ContentType: ContentType, IsDriverCheck: true);

                    PreciseDriverProductVersionCTAC.Products += $"PN={ProductGUID}_{MachineType}&V=200.0.{driverBuildNumber}.0&Source=SMBIOS;";
                    PreciseDriverProductVersionCTAC.SyncCurrentVersionOnly = true;

                    Logging.Log($"Checking for updates... 200.0.{driverBuildNumber}.0 / {newestDriverVersion}");
                    IEnumerable<UpdateData> PreciseDriverProductVersionUpdateData = await FE3Handler.GetUpdates(null, PreciseDriverProductVersionCTAC, token, FileExchangeV3UpdateFilter.ProductRelease);

                    if (!PreciseDriverProductVersionUpdateData.Any())
                    {
                        Logging.Log("No updates found that matched the specified criteria.", Logging.LoggingLevel.Error);
                    }
                    else
                    {
                        foreach (UpdateData update in PreciseDriverProductVersionUpdateData)
                        {
                            if (update.Xml.LocalizedProperties.Title.Contains("Windows"))
                            {
                                continue;
                            }

                            Logging.Log("Title: " + update.Xml.LocalizedProperties.Title);
                            Logging.Log("Description: " + update.Xml.LocalizedProperties.Description);

                            string fwOutput = $"{outputFolder}{Path.DirectorySeparatorChar}200.0.{driverBuildNumber}.0";
                            if (!Directory.Exists(fwOutput) || !Directory.EnumerateFiles(fwOutput).Any())
                            {
                                _ = await UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads.UpdateUtils.ProcessUpdateAsync(update, fwOutput, MachineType, new ReportProgress(), Language, Edition, false, false);
                            }

                            previousCompositionDatabase = await GenerateChangelog(update, changelogOutput, driverBuildNumber, previousCompositionDatabase);
                        }
                    }
                }
            }
        }
    }
}
