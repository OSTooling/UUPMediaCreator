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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Services.Composition.Database;
using UnifiedUpdatePlatform.Services.Composition.Database.Applications;
using UnifiedUpdatePlatform.Services.WindowsUpdate;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads
{
    public static partial class UpdateUtils
    {
        public static string[] GetFilenameForCEUIFile(Models.FE3.XML.ExtendedUpdateInfo.File extendedUpdateInfoFile, IEnumerable<PayloadItem> payloadItems)
        {
            string filename = extendedUpdateInfoFile.FileName.Replace('\\', Path.DirectorySeparatorChar);

            if (payloadItems.Any(x => x.PayloadHash == extendedUpdateInfoFile.AdditionalDigest.Text || x.PayloadHash == extendedUpdateInfoFile.Digest))
            {
                IEnumerable<PayloadItem> payloads = payloadItems.Where(x => x.PayloadHash == extendedUpdateInfoFile.AdditionalDigest.Text || x.PayloadHash == extendedUpdateInfoFile.Digest);
                return payloads.Select(p => p.Path.Replace('\\', Path.DirectorySeparatorChar)).DistinctBy(x => x.ToLower()).ToArray();
            }
            else if (!payloadItems.Any() && filename.Contains('_') && !filename.StartsWith("_") && (!filename.Contains('-') || filename.IndexOf('-') > filename.IndexOf('_')))
            {
                filename = filename[..filename.IndexOf('_')] + Path.DirectorySeparatorChar + filename[(filename.IndexOf('_') + 1)..];
                return [filename.TrimStart(Path.DirectorySeparatorChar)];
            }

            return [filename];
        }

        public static string FixFilePath((Models.FE3.XML.ExtendedUpdateInfo.File, FileExchangeV3FileDownloadInformation) boundFile, HashSet<string> pathList, HashSet<BaseManifest> compDBs, string path)
        {
            string newPath = path;

            MatchCollection guidMatches = guidRegex().Matches(newPath);

            // Some file packages (especially for device manifests) contain guids as part of their name and often the actual name of the file after, so trim that right away
            foreach (Match match in guidMatches.OrderByDescending(x => x.Index + x.Length))
            {
                newPath = string.Concat(newPath.AsSpan(0, match.Index), newPath.AsSpan(match.Index + match.Length));
            }

            // It is possible that above operation caused the file to go empty, check for that and if thats the case, revert now.
            if (newPath != path && (string.IsNullOrEmpty(newPath) || newPath.StartsWith('.')))
            {
                newPath = path;
            }

            try
            {
                foreach (BaseManifest compDb in compDBs)
                {
                    foreach (Package pkg in compDb.Packages.Package)
                    {
                        string payloadHash = pkg.Payload.PayloadItem[0].PayloadHash;
                        if (payloadHash == boundFile.Item1.AdditionalDigest.Text || payloadHash == boundFile.Item1.Digest)
                        {
                            if (pkg.ID.Contains('-') && pkg.ID.Contains(".inf", StringComparison.InvariantCultureIgnoreCase))
                            {
                                newPath = pkg.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.InvariantCultureIgnoreCase);
                            }
                            break;
                        }
                    }
                }
            }
            catch { }

            if (!pathList.Add(newPath.ToLower()))
            {
                string basePath, suffix;

                if (Path.HasExtension(newPath))
                {
                    basePath = Path.Combine(Path.GetDirectoryName(newPath), Path.GetFileNameWithoutExtension(newPath));
                    suffix = Path.GetExtension(newPath);
                }
                else
                {
                    basePath = Path.Combine(Path.GetDirectoryName(newPath), Path.GetFileName(newPath));
                    suffix = "";
                }

                uint i = 1;

                do
                {
                    newPath = $"{basePath} ({i++}){suffix}";
                }
                while (!pathList.Add(newPath.ToLower()));
            }

            return newPath;
        }

        public static bool ShouldFileGetDownloaded(Models.FE3.XML.ExtendedUpdateInfo.File file2, IEnumerable<PayloadItem> payloadItems)
        {
            string filename = file2.FileName.Replace('\\', Path.DirectorySeparatorChar);
            if (payloadItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest))
            {
                PayloadItem payload = payloadItems.First(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest);
                filename = payload.Path.Replace('\\', Path.DirectorySeparatorChar);

                if (payload.PayloadType.Equals("ExpressCab", StringComparison.InvariantCultureIgnoreCase))
                {
                    // This is a diff cab, skip it
                    return false;
                }
            }

            return !filename.Contains("Diff", StringComparison.InvariantCultureIgnoreCase) &&
                    !filename.Contains("Baseless", StringComparison.InvariantCultureIgnoreCase);
        }

        public static UpdateData TrimDeltasFromUpdateData(UpdateData update)
        {
            update.Xml.Files.File = update.Xml.Files.File.Where(x => !x.FileName.Replace('\\', Path.DirectorySeparatorChar).EndsWith(".psf", StringComparison.InvariantCultureIgnoreCase)
            && !x.FileName.Replace('\\', Path.DirectorySeparatorChar).StartsWith("Diff", StringComparison.InvariantCultureIgnoreCase)
             && !x.FileName.Replace('\\', Path.DirectorySeparatorChar).StartsWith("Baseless", StringComparison.InvariantCultureIgnoreCase)).ToArray();
            return update;
        }

        private static bool IsFileBanned(Models.FE3.XML.ExtendedUpdateInfo.File file2, IEnumerable<PayloadItem> bannedItems)
        {
            return bannedItems.Any(x => x.PayloadHash == file2.AdditionalDigest.Text || x.PayloadHash == file2.Digest);
        }

        private static
        (
            HashSet<BaseManifest> selectedCompDBs,
            HashSet<BaseManifest> discardedCompDBs,
            HashSet<BaseManifest> specificCompDBs
        )
        FilterCompDBs(HashSet<BaseManifest> compDBs, string Edition, string Language)
        {
            HashSet<BaseManifest> selectedCompDBs = [];
            HashSet<BaseManifest> discardedCompDBs = [];
            HashSet<BaseManifest> specificCompDBs = [];

            foreach (BaseManifest cdb in compDBs)
            {
                bool IsDiff = cdb.Tags?.Tag?.Any(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase) && (x.Value.Equals("Diff", StringComparison.InvariantCultureIgnoreCase) || x.Value.Equals("Baseless", StringComparison.InvariantCultureIgnoreCase))) == true;

                if (IsDiff)
                {
                    _ = discardedCompDBs.Add(cdb);
                }
                else
                {
                    bool hasLang = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase)) == true;
                    bool hasEdition = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)) == true;

                    bool editionMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Edition, StringComparison.InvariantCultureIgnoreCase)) == true;
                    bool langMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Language, StringComparison.InvariantCultureIgnoreCase)) == true;

                    bool IsNeutral = cdb.Tags?.Type.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase) ?? false;

                    switch (!string.IsNullOrEmpty(Language), !string.IsNullOrEmpty(Edition))
                    {
                        // Get everything
                        case (false, false):
                            {
                                _ = selectedCompDBs.Add(cdb);
                                break;
                            }
                        // Get edition
                        case (false, true):
                            {
                                if ((!hasEdition || editionMatching) && !IsNeutral)
                                {
                                    _ = specificCompDBs.Add(cdb);
                                    _ = selectedCompDBs.Add(cdb);
                                }
                                else
                                {
                                    _ = discardedCompDBs.Add(cdb);
                                }
                                break;
                            }
                        // Get language
                        case (true, false):
                            {
                                if (!hasLang || langMatching)
                                {
                                    _ = specificCompDBs.Add(cdb);
                                    _ = selectedCompDBs.Add(cdb);
                                }
                                else
                                {
                                    _ = discardedCompDBs.Add(cdb);
                                }
                                break;
                            }
                        // Get edition + language
                        case (true, true):
                            {
                                if (((!hasEdition && !hasLang) || (!hasEdition && hasLang && langMatching) || (!hasLang && hasEdition && editionMatching) || (hasEdition && hasLang && langMatching && editionMatching)) && !IsNeutral)
                                {
                                    _ = specificCompDBs.Add(cdb);
                                    _ = selectedCompDBs.Add(cdb);
                                }
                                else
                                {
                                    _ = discardedCompDBs.Add(cdb);
                                }
                                break;
                            }
                    }
                }
            }

            return (selectedCompDBs, discardedCompDBs, specificCompDBs);
        }

        private static
        (
            HashSet<PayloadItem> payloadItems,
            HashSet<PayloadItem> bannedPayloadItems
        )
        BuildListOfPayloads(HashSet<BaseManifest> compDBs, string Edition, string Language)
        {
            HashSet<PayloadItem> payloadItems = [];
            HashSet<PayloadItem> bannedPayloadItems = [];

            if (compDBs == null)
            {
                return (payloadItems, bannedPayloadItems);
            }

            IEnumerable<BaseManifest> AppCompDBs = null;

            (HashSet<BaseManifest> selectedCompDBs, HashSet<BaseManifest> discardedCompDBs, HashSet<BaseManifest> specificCompDBs) = FilterCompDBs(compDBs, Edition, Language);

            if (compDBs.Any(x => x.Name?.StartsWith("Build~") == true && (x.Name?.EndsWith("~Desktop_Apps~~") == true || x.Name?.EndsWith("~Desktop_Apps_Moment~~") == true)))
            {
                AppCompDBs = compDBs.Where(x => x.Name?.StartsWith("Build~") == true && (x.Name?.EndsWith("~Desktop_Apps~~") == true || x.Name?.EndsWith("~Desktop_Apps_Moment~~") == true));
            }

            foreach (BaseManifest cdb in selectedCompDBs)
            {
                if (AppCompDBs?.Contains(cdb) == true || cdb.Packages == null)
                {
                    continue;
                }

                foreach (Package pkg in cdb.Packages.Package)
                {
                    if (pkg.Payload == null)
                    {
                        continue;
                    }

                    foreach (PayloadItem item in pkg.Payload.PayloadItem)
                    {
                        _ = item.PayloadType.Equals("Diff", StringComparison.InvariantCultureIgnoreCase)
                            ? bannedPayloadItems.Add(item)
                            : payloadItems.Add(item);
                    }
                }

                if (cdb.AppX?.AppXPackages?.Package != null)
                {
                    foreach (AppxPackage pkg in cdb.AppX.AppXPackages.Package)
                    {
                        if (pkg.Payload == null)
                        {
                            continue;
                        }

                        foreach (PayloadItem item in pkg.Payload.PayloadItem)
                        {
                            _ = item.PayloadType.Equals("Diff", StringComparison.InvariantCultureIgnoreCase)
                                ? bannedPayloadItems.Add(item)
                                : payloadItems.Add(item);
                        }
                    }
                }
            }

            foreach (BaseManifest cdb in discardedCompDBs)
            {
                if (AppCompDBs?.Contains(cdb) == true || cdb.Packages == null)
                {
                    continue;
                }

                foreach (Package pkg in cdb.Packages.Package)
                {
                    if (pkg.Payload == null)
                    {
                        continue;
                    }

                    foreach (PayloadItem item in pkg.Payload.PayloadItem)
                    {
                        _ = bannedPayloadItems.Add(item);
                    }
                }

                if (cdb.AppX?.AppXPackages?.Package != null)
                {
                    foreach (AppxPackage pkg in cdb.AppX.AppXPackages.Package)
                    {
                        if (pkg.Payload == null)
                        {
                            continue;
                        }

                        foreach (PayloadItem item in pkg.Payload.PayloadItem)
                        {
                            _ = bannedPayloadItems.Add(item);
                        }
                    }
                }
            }

            if (AppCompDBs != null)
            {
                List<string> payloadHashesToKeep = [];

                switch (!string.IsNullOrEmpty(Language), !string.IsNullOrEmpty(Edition))
                {
                    // Get everything
                    case (false, false):
                        {
                            foreach (BaseManifest AppCompDB in AppCompDBs)
                            {
                                foreach (Package pkg in AppCompDB.Packages.Package)
                                {
                                    if (pkg.Payload == null)
                                    {
                                        continue;
                                    }

                                    foreach (PayloadItem item in pkg.Payload.PayloadItem)
                                    {
                                        if (!item.PayloadType.Equals("Diff", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            payloadHashesToKeep.Add(item.PayloadHash);
                                        }
                                    }
                                }

                                if (AppCompDB.AppX?.AppXPackages?.Package != null)
                                {
                                    foreach (AppxPackage pkg in AppCompDB.AppX.AppXPackages.Package)
                                    {
                                        if (pkg.Payload == null)
                                        {
                                            continue;
                                        }

                                        foreach (PayloadItem item in pkg.Payload.PayloadItem)
                                        {
                                            if (!item.PayloadType.Equals("Diff", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                payloadHashesToKeep.Add(item.PayloadHash);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    // Get edition
                    case (false, true):
                        {
                            foreach (BaseManifest cdb in compDBs.GetEditionCompDBs().Where(cdb =>
                            {
                                bool hasEdition = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)) == true;

                                bool editionMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Edition, StringComparison.InvariantCultureIgnoreCase)) == true;

                                return hasEdition && editionMatching;
                            }))
                            {
                                PackageProperties[] appxsToKeep = AppxSelectionEngine.GetAppxFilesToKeep(cdb, AppCompDBs, Language);

                                foreach (PackageProperties appxToKeep in appxsToKeep)
                                {
                                    payloadHashesToKeep.Add(appxToKeep.SHA256);
                                }
                            }
                            break;
                        }
                    // Get language
                    case (true, false):
                        {
                            foreach (BaseManifest cdb in compDBs.GetEditionCompDBs().Where(cdb =>
                            {
                                bool hasLang = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase)) == true;

                                bool langMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Language, StringComparison.InvariantCultureIgnoreCase)) == true;

                                return hasLang && langMatching;
                            }))
                            {
                                PackageProperties[] appxsToKeep = AppxSelectionEngine.GetAppxFilesToKeep(cdb, AppCompDBs, Language);

                                foreach (PackageProperties appxToKeep in appxsToKeep)
                                {
                                    payloadHashesToKeep.Add(appxToKeep.SHA256);
                                }
                            }
                            break;
                        }
                    // Get edition + language
                    case (true, true):
                        {
                            foreach (BaseManifest cdb in compDBs.GetEditionCompDBs().Where(cdb =>
                            {
                                bool hasLang = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase)) == true;
                                bool hasEdition = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)) == true;

                                bool editionMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Edition, StringComparison.InvariantCultureIgnoreCase)) == true;
                                bool langMatching = cdb.Tags?.Tag?.Any(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase) && x.Value.Equals(Language, StringComparison.InvariantCultureIgnoreCase)) == true;

                                return hasEdition && editionMatching && hasLang && langMatching;
                            }))
                            {
                                PackageProperties[] appxsToKeep = AppxSelectionEngine.GetAppxFilesToKeep(cdb, AppCompDBs, Language);

                                foreach (PackageProperties appxToKeep in appxsToKeep)
                                {
                                    payloadHashesToKeep.Add(appxToKeep.SHA256);
                                }
                            }
                            break;
                        }
                }

                foreach (BaseManifest AppCompDB in AppCompDBs)
                {
                    foreach (Package pkg in AppCompDB.Packages.Package)
                    {
                        if (pkg.Payload == null)
                        {
                            continue;
                        }

                        foreach (PayloadItem item in pkg.Payload.PayloadItem)
                        {
                            _ = payloadHashesToKeep.Any(x => x == item.PayloadHash) ? payloadItems.Add(item) : bannedPayloadItems.Add(item);
                        }
                    }
                }

                foreach (BaseManifest AppCompDB in AppCompDBs)
                {
                    if (AppCompDB.AppX?.AppXPackages?.Package != null)
                    {
                        foreach (AppxPackage pkg in AppCompDB.AppX.AppXPackages.Package)
                        {
                            if (pkg.Payload == null)
                            {
                                continue;
                            }

                            foreach (PayloadItem item in pkg.Payload.PayloadItem)
                            {
                                _ = payloadHashesToKeep.Any(x => x == item.PayloadHash) ? payloadItems.Add(item) : bannedPayloadItems.Add(item);
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(Edition) || !string.IsNullOrEmpty(Language))
            {
                if (specificCompDBs.Count == 0)
                {
                    throw new Exception("No update metadata matched the specified criteria");
                }
                else
                {
                    foreach (BaseManifest specificCompDB in specificCompDBs)
                    {
                        if (AppCompDBs?.Contains(specificCompDB) == true || specificCompDB.Packages == null)
                        {
                            continue;
                        }

                        foreach (Package pkg in specificCompDB.Packages.Package)
                        {
                            if (pkg.Payload == null)
                            {
                                continue;
                            }

                            foreach (PayloadItem item in pkg.Payload.PayloadItem)
                            {
                                _ = bannedPayloadItems.RemoveWhere(x => x.PayloadHash == item.PayloadHash);
                            }
                        }

                        if (specificCompDB.AppX?.AppXPackages?.Package != null)
                        {
                            foreach (AppxPackage pkg in specificCompDB.AppX.AppXPackages.Package)
                            {
                                if (pkg.Payload == null)
                                {
                                    continue;
                                }

                                foreach (PayloadItem item in pkg.Payload.PayloadItem)
                                {
                                    _ = bannedPayloadItems.RemoveWhere(x => x.PayloadHash == item.PayloadHash);
                                }
                            }
                        }
                    }
                }
            }

            return (payloadItems, bannedPayloadItems);
        }

        public static async Task<string> ProcessUpdateAsync(UpdateData update, string pOutputFolder, MachineType MachineType, IProgress<GeneralDownloadProgress> generalDownloadProgress, string Language = "", string Edition = "", bool WriteMetadata = true, bool UseAutomaticDownloadFolder = true, int downloadThreads = 4)
        {
            string buildstr = "";
            IEnumerable<string> languages = null;

            int returnCode = 0;
            IEnumerable<Models.FE3.XML.ExtendedUpdateInfo.File> filesToDownload = null;

            HashSet<BaseManifest> compDBs = await update.GetCompDBsAsync();

            await Task.WhenAll(
                Task.Run(async () => buildstr = await update.GetBuildStringAsync()),
                Task.Run(async () => languages = await update.GetAvailableLanguagesAsync()));

            buildstr ??= "";

            if (string.IsNullOrEmpty(buildstr) && update.Xml.LocalizedProperties.Title.Contains("(UUP-CTv2)"))
            {
                string unformattedBase = update.Xml.LocalizedProperties.Title.Split(" ")[0];
                buildstr = $"10.0.{unformattedBase.Split(".")[0]}.{unformattedBase.Split(".")[1]} ({unformattedBase.Split(".")[2]}.{unformattedBase.Split(".")[3]})";
            }
            else if (string.IsNullOrEmpty(buildstr))
            {
                buildstr = update.Xml.LocalizedProperties.Title;
            }

            string name = $"{buildstr.Replace(" ", ".").Replace("(", "").Replace(")", "")}_{MachineType.ToString().ToLower()}fre_{update.Xml.UpdateIdentity.UpdateID.Split("-").Last()}";
            Regex illegalCharacters = invalidCharactersRegex();
            name = illegalCharacters.Replace(name, "");
            string OutputFolder = pOutputFolder;
            if (UseAutomaticDownloadFolder)
            {
                OutputFolder = Path.Combine(pOutputFolder, name);
            }

            (HashSet<PayloadItem> payloadItems, HashSet<PayloadItem> bannedPayloadItems) =
                BuildListOfPayloads(compDBs, Edition, Language);

            JsonSerializerOptions jsonSerializerOptions = new()
            {
                WriteIndented = true,
                IncludeFields = true
            };

            do
            {
                IEnumerable<FileExchangeV3FileDownloadInformation> fileUrls = await FE3Handler.GetFileUrls(update) ?? throw new Exception("Getting file urls failed.");
                if (!Directory.Exists(OutputFolder))
                {
                    _ = Directory.CreateDirectory(OutputFolder);
                }

                string tmpname = $"{update.Xml.LocalizedProperties.Title} ({MachineType}).uupmcreplay";
                illegalCharacters = invalidCharactersRegex();
                tmpname = illegalCharacters.Replace(tmpname, "");
                string filename = Path.Combine(OutputFolder, tmpname);

                if (WriteMetadata && !File.Exists(filename))
                {
                    File.WriteAllText(filename, JsonSerializer.Serialize(update, jsonSerializerOptions));
                }

                using HttpDownloader helperDl = new(OutputFolder, downloadThreads);

                filesToDownload = update.Xml.Files.File.DistinctBy(x => $"{x.Digest}|{x.DigestAlgorithm}|{x.AdditionalDigest.Algorithm}|{x.AdditionalDigest.Text}").AsParallel().Where(x => !IsFileBanned(x, bannedPayloadItems));

                returnCode = 0;

                IEnumerable<(Models.FE3.XML.ExtendedUpdateInfo.File, FileExchangeV3FileDownloadInformation)> boundList = filesToDownload
                    .AsParallel()
                    .Select(x => (x, fileUrls.First(y => y.Digest == x.Digest)))
                    //.Where(x => UpdateUtils.ShouldFileGetDownloaded(x.x, payloadItems))
                    .OrderBy(x => x.Item2.ExpirationDate);

                List<UUPFile> fileList = [];
                HashSet<string> pathList = [];

                foreach ((Models.FE3.XML.ExtendedUpdateInfo.File, FileExchangeV3FileDownloadInformation) boundFile in boundList)
                {
                    string[] paths = GetFilenameForCEUIFile(boundFile.Item1, payloadItems);
                    foreach (string path in paths)
                    {
                        fileList.Add(new UUPFile(
                            boundFile.Item2,
                            FixFilePath(boundFile, pathList, compDBs, path),
                            long.Parse(boundFile.Item1.Size),
                            boundFile.Item1.AdditionalDigest.Text,
                            boundFile.Item1.AdditionalDigest.Algorithm));
                    }
                }

                returnCode = await helperDl.DownloadAsync([.. fileList], generalDownloadProgress) ? 0 : -1;

                if (returnCode != 0)
                {
                    continue;
                }
            }
            while (returnCode != 0);

            return OutputFolder;
        }

        [GeneratedRegex("[\\\\/:*?\"<>|]")]
        private static partial Regex invalidCharactersRegex();

        [GeneratedRegex("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}")]
        private static partial Regex guidRegex();
    }
}
