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
using Cabinet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate
{
    public static class StringExtensions
    {
        public static bool Contains(this string str, string substring,
                                    StringComparison comp)
        {
            if (substring == null)
            {
                throw new ArgumentNullException(nameof(substring),
                                             "substring cannot be null.");
            }
            else if (!Enum.IsDefined(typeof(StringComparison), comp))
            {
                throw new ArgumentException("comp is not a member of StringComparison",
                                         nameof(comp));
            }

            return str.Contains(substring, comp);
        }

        public static string Replace(this string originalString, string oldValue, string newValue, StringComparison comparisonType)
        {
            int startIndex = 0;
            while (true)
            {
                startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType);
                if (startIndex == -1)
                {
                    break;
                }

                originalString = originalString[..startIndex] + newValue + originalString[(startIndex + oldValue.Length)..];

                startIndex += newValue.Length;
            }

            return originalString;
        }
    }

    public static class BuildFetcher
    {
        public class AvailableBuild
        {
            public UpdateData UpdateData
            {
                get; set;
            }
            public string Title
            {
                get; set;
            }
            public string BuildString
            {
                get; set;
            }
            public string Description
            {
                get; set;
            }
            public string Created
            {
                get; set;
            }
        }

        public class AvailableBuildLanguages
        {
            public string Title
            {
                get; set;
            }
            public string LanguageCode
            {
                get; set;
            }
            public Uri FlagUri
            {
                get; set;
            }
        }

        public class AvailableEdition
        {
            public string Edition
            {
                get; set;
            }
        }

        public static async Task<AvailableBuild[]> GetAvailableBuildsAsync(MachineType machineType)
        {
            List<AvailableBuild> availableBuilds = new();

            IEnumerable<UpdateData> updates = await GetUpdates(machineType);

            foreach (UpdateData update in updates)
            {
                AvailableBuild availableBuild = new()
                {
                    Title = update.Xml.LocalizedProperties.Title,
                    Description = update.Xml.LocalizedProperties.Description,
                    UpdateData = update,
                    Created = update.UpdateInfo.Deployment.LastChangeTime
                };

                string BuildStr = await update.GetBuildStringAsync();
                if (!string.IsNullOrEmpty(BuildStr))
                {
                    availableBuild.Title += $" ({BuildStr})";
                    availableBuild.BuildString = BuildStr;
                }

                availableBuilds.Add(availableBuild);
            }

            return availableBuilds.OrderBy(x => x.Title).ToArray();
        }

        public static async Task<AvailableBuildLanguages[]> GetAvailableBuildLanguagesAsync(UpdateData UpdateData)
        {
            List<AvailableBuildLanguages> availableBuildLanguages = (await UpdateData.GetAvailableLanguagesAsync()).Select(lang =>
            {
                CultureInfo boundlanguageobject = CultureInfo.GetCultureInfoByIetfLanguageTag(lang);

                return new AvailableBuildLanguages() { LanguageCode = lang, Title = boundlanguageobject.DisplayName, FlagUri = new Uri($"ms-appx:///Assets/Flags/{lang.Split('-').Last()}.png") };
            }).ToList();

            availableBuildLanguages.Sort((x, y) => x.Title.CompareTo(y.Title));

            return availableBuildLanguages.ToArray();
        }

        public static async Task<AvailableEdition[]> GetAvailableEditions(UpdateData UpdateData, string languagecode)
        {
            List<AvailableEdition> availableEditions = new();

            List<CExtendedUpdateInfoXml.File> metadataCabs = new();

            foreach (CExtendedUpdateInfoXml.File file in UpdateData.Xml.Files.File)
            {
                if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                {
                    metadataCabs.Add(file);
                }
            }

            if (metadataCabs.Count == 0)
            {
                goto exit;
            }

            WebClient client = new();

            IEnumerable<string> cabinetFiles = metadataCabs.Select(x => x.FileName.Replace('\\', Path.DirectorySeparatorChar));

            IEnumerable<string> potentialFiles = cabinetFiles.Where(x =>
                x.Contains("desktoptargetcompdb_", StringComparison.CurrentCultureIgnoreCase) &&
                x.ToLower().Contains($"_{languagecode}") &&
                !x.Contains("lxp", StringComparison.CurrentCultureIgnoreCase) &&
                !x.ToLower().Contains($"desktoptargetcompdb_{languagecode}"));

            foreach (string file in potentialFiles)
            {
                string edition = file.Split('_').AsEnumerable().Reverse().Skip(1).First();

                if (availableEditions.Any(x => x.Edition == edition))
                {
                    continue;
                }

                availableEditions.Add(new AvailableEdition() { Edition = edition });
            }

            foreach (CExtendedUpdateInfoXml.File metadataCab in metadataCabs)
            {
                FileExchangeV3FileDownloadInformation fileDownloadInfo = await FE3Handler.GetFileUrl(UpdateData, metadataCabs.First().Digest);
                if (fileDownloadInfo == null)
                {
                    goto exit;
                }

                string metadataCabTemp = Path.GetTempFileName();

                try
                {
                    // Download the file
                    await client.DownloadFileTaskAsync(new Uri(fileDownloadInfo.DownloadUrl), metadataCabTemp);

                    // If the file is encrypted, decrypt it now
                    if (fileDownloadInfo.IsEncrypted)
                    {
                        if (!await fileDownloadInfo.DecryptAsync(metadataCabTemp, metadataCabTemp + ".decrypted"))
                        {
                            goto exit;
                        }

                        metadataCabTemp += ".decrypted";
                    }

                    // Create the required directory to expand the cabinet file
                    string tmp = Path.GetTempFileName();
                    File.Delete(tmp);
                    _ = Directory.CreateDirectory(tmp);

                    // Expand the cabinet file
                    CabinetExtractor.ExtractCabinet(metadataCabTemp, tmp);

                    // Two possibilities, we either have cabs inside our metadata, or xmls, handle both
                    foreach (string cabinetFile in Directory.EnumerateFiles(tmp, "*.cab", SearchOption.AllDirectories))
                    {
                        try
                        {
                            cabinetFiles = CabinetExtractor.EnumCabinetFiles(cabinetFile).Select(x => x.FileName);

                            potentialFiles = cabinetFiles.Where(x =>
                                    x.Contains("desktoptargetcompdb_", StringComparison.CurrentCultureIgnoreCase) &&
                                    x.ToLower().Contains($"_{languagecode}") &&
                                    !x.Contains("lxp", StringComparison.CurrentCultureIgnoreCase) &&
                                    !x.ToLower().Contains($"desktoptargetcompdb_{languagecode}"));

                            foreach (string file in potentialFiles)
                            {
                                string edition = file.Split('_').AsEnumerable().Reverse().Skip(1).First();

                                if (availableEditions.Any(x => x.Edition == edition))
                                {
                                    continue;
                                }

                                availableEditions.Add(new AvailableEdition() { Edition = edition });
                            }
                        }
                        catch { }
                    }
                }
                catch { }
            }

            availableEditions.Sort((x, y) => x.Edition.CompareTo(y.Edition));

        exit:
            return availableEditions.ToArray();
        }

        private static UpdateData TrimDeltasFromUpdateData(UpdateData update)
        {
            update.Xml.Files.File = update.Xml.Files.File.Where(x => !x.FileName.Replace('\\', Path.DirectorySeparatorChar).EndsWith(".psf", StringComparison.InvariantCultureIgnoreCase)
            && !x.FileName.Replace('\\', Path.DirectorySeparatorChar).StartsWith("Diff", StringComparison.InvariantCultureIgnoreCase)
             && !x.FileName.Replace('\\', Path.DirectorySeparatorChar).StartsWith("Baseless", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            return update;
        }

        private static void AddUpdatesIfNotPresentAlready(ICollection<UpdateData> updates, IEnumerable<UpdateData> data)
        {
            //UpdateData[] data = uncleanedData.Select(x => TrimDeltasFromUpdateData(x)).ToArray();

            foreach (UpdateData update in data)
            {
                if (updates.Any(x => x.Xml.Files.File.Length == update.Xml.Files.File.Length))
                {
                    IEnumerable<UpdateData> potentialDupes = updates.Where(x => x.Xml.Files.File.Length == update.Xml.Files.File.Length);

                    CExtendedUpdateInfoXml.File metadataCab = null;

                    foreach (CExtendedUpdateInfoXml.File file in update.Xml.Files.File)
                    {
                        if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                        {
                            metadataCab = file;
                            break;
                        }
                    }

                    if (metadataCab == null)
                    {
                        continue;
                    }

                    bool exists = false;

                    foreach (UpdateData potentialDupe in potentialDupes)
                    {
                        CExtendedUpdateInfoXml.File metadataCab2 = null;

                        foreach (CExtendedUpdateInfoXml.File file in potentialDupe.Xml.Files.File)
                        {
                            if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                            {
                                metadataCab2 = file;
                                break;
                            }
                        }

                        if (metadataCab2 == null)
                        {
                            continue;
                        }

                        if (metadataCab.Digest == metadataCab2.Digest)
                        {
                            exists = true;
                            continue;
                        }
                    }

                    if (!exists)
                    {
                        updates.Add(update);
                    }
                }
                else
                {
                    updates.Add(update);
                }
            }
        }

        private static Dictionary<CTAC, string> GetRingCTACs(MachineType machineType, OSSkuId osSkuId)
        {
            return new Dictionary<CTAC, string>()
            {
                // NOTE: Windows 10 1703 (rs2_release / 15063) and older are no longer
                // offered by the Windows Update servers, so the old "Insider Slow/Fast
                // (RS2)" entries have been removed.
                { new CTAC(osSkuId, "10.0.16299.15", machineType, "Retail", "", "CB", "rs3_release", "Production", true), "Retail (1709 - RS3)" },
                { new CTAC(osSkuId, "10.0.17134.1", machineType, "Retail", "", "CB", "rs4_release", "Production", true), "Retail (1803 - RS4)" },
                { new CTAC(osSkuId, "10.0.17763.1217", machineType, "Retail", "", "CB", "rs5_release", "Production", true), "Retail (1809 - RS5)" },
                { new CTAC(osSkuId, "10.0.18362.836", machineType, "Retail", "", "CB", "19h1_release", "Production", true), "Retail (1903 - 19H1)" },
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "Retail", "", "CB", "vb_release", "Production", false, false), "Retail (Windows 10 - VB)" },
                { new CTAC(osSkuId, "10.0.19041.84", machineType, "Retail", "", "CB", "vb_release", "Production", false), "Retail" },
                { new CTAC(osSkuId, "10.0.19045.1", machineType, "Retail", "", "CB", "vb_release", "Production", false, IsEsu: true), "Retail (Windows 10 ESU)" },
                { new CTAC(osSkuId, "10.0.22621.1", machineType, "Retail", "", "CB", "ni_release", "Production", false), "Retail (Windows 11 - NI)" },
                { new CTAC(osSkuId, "10.0.26100.1", machineType, "Retail", "", "CB", "ge_release", "Production", false), "Retail (Windows 11 - GE)" },
                { new CTAC(osSkuId, "10.0.26100.1", machineType, "External", "ReleasePreview", "CB", "ge_release", "Production", false, false), "Release Preview (Windows 11)" },
                { new CTAC(osSkuId, "10.0.26100.1", machineType, "External", "Beta", "CB", "ge_release", "Production", false, false), "Beta (Windows 11)" },
                { new CTAC(osSkuId, "10.0.26100.1", machineType, "External", "Dev", "CB", "rs_prerelease", "Production", false, false), "Dev" },
                { new CTAC(osSkuId, "10.0.26100.1", machineType, "External", "CanaryChannel", "CB", "rs_prerelease", "Production", false, false), "Canary" },
                { new CTAC(osSkuId, "10.0.19045.1", machineType, "External", "Beta", "CB", "vb_release", "Production", false, false), "Beta (Windows 10)" },
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "RP", "External", "CB", "vb_release", "Production", false, false, "Active"), "Insider Release Preview" },
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "WIS", "External", "CB", "vb_release", "Production", false, false, "Active"), "Insider Slow" },
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "WIF", "External", "CB", "vb_release", "Production", false, false, "Active"), "Insider Fast" },
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "WIF", "External", "CB", "vb_release", "Production", false, false, "Skip"), "Skip Ahead" },
            };
        }

        private static async Task<IEnumerable<UpdateData>> GetUpdates(MachineType MachineType)
        {
            HashSet<UpdateData> updates = new();

            CTAC[] ctacs = GetRingCTACs(MachineType, OSSkuId.Professional).Union(GetRingCTACs(MachineType, OSSkuId.PPIPro)).Select(x => x.Key).ToArray();

            List<Task<IEnumerable<UpdateData>>> tasks = new();
            foreach (CTAC ctac in ctacs)
            {
                tasks.Add(GetUpdatesFor(ctac));
            }

            IEnumerable<UpdateData>[] datas = await Task.WhenAll(tasks);
            foreach (IEnumerable<UpdateData> data in datas)
            {
                AddUpdatesIfNotPresentAlready(updates, data);
            }

            return updates;
        }

        private static async Task<IEnumerable<UpdateData>> GetUpdatesFor(CTAC ctac)
        {
            try
            {
                return await FE3Handler.GetUpdates(null, ctac, null, FileExchangeV3UpdateFilter.ProductRelease);
            }
            catch
            {
                return Array.Empty<UpdateData>();
            }
        }
    }
}