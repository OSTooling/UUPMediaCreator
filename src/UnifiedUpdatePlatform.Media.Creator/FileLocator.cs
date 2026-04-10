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
using UnifiedUpdatePlatform.Services.Composition.Database;
using UnifiedUpdatePlatform.Services.Composition.Database.Applications;

#nullable enable

namespace UnifiedUpdatePlatform.Media.Creator
{
    internal static class FileLocator
    {
        internal static (bool, HashSet<string>) VerifyFilesAreAvailableForCompositionDatabases(HashSet<BaseManifest> CompositionDatabases, string UUPPath)
        {
            HashSet<string> missingPackages = [];

            foreach (BaseManifest compDB in CompositionDatabases)
            {
                (bool succeeded, HashSet<string> missingFiles) = Planning.FileLocator.VerifyFilesAreAvailableForCompDB(compDB, UUPPath);
                foreach (string? missingFile in missingFiles)
                {
                    if (!missingPackages.Contains(missingFile))
                    {
                        _ = missingPackages.Add(missingFile);
                    }
                }
            }

            return (missingPackages.Count == 0, missingPackages);
        }

        internal static BaseManifest? GetEditionCompDBForLanguage(
            IEnumerable<BaseManifest> CompositionDatabases,
            string Edition,
            string LanguageCode)
        {
            foreach (BaseManifest? compDB in CompositionDatabases)
            {
                //
                // Newer style compdbs have a tag attribute, make use of it.
                //
                if (compDB.Tags != null)
                {
                    if (compDB.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) &&
                       compDB.Tags.Tag?.Count == 3 &&
                       compDB.Tags.Tag.Find(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true &&
                       compDB.Tags.Tag.Find(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true &&
                       compDB.Tags.Tag.Find(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals(Edition, StringComparison.InvariantCultureIgnoreCase) == true &&
                       compDB.Name?.EndsWith("~Desktop_Apps~~") != true &&
                       compDB.Name?.EndsWith("~Desktop_Apps_Moment~~") != true)
                    {
                        return compDB;
                    }
                }
                //
                // Older style compdbs have no tag elements, we need to find out if it's an edition compdb using another way
                // TODO: Do not do contains
                //
                else if (compDB.Features?.Feature?.FirstOrDefault(x =>
                        x.Type?.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase) == true &&
                        x.FeatureID?.Contains(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true &&
                        x.FeatureID?.Contains(Edition, StringComparison.InvariantCultureIgnoreCase) == true) != null &&
                       compDB.Name?.EndsWith("~Desktop_Apps~~") != true &&
                       compDB.Name?.EndsWith("~Desktop_Apps_Moment~~") != true)
                {
                    return compDB;
                }
            }

            return null;
        }

        internal static (bool Succeeded, string BaseESD) LocateFilesForSetupMediaCreation(
            string UUPPath,
            string LanguageCode,
            IEnumerable<BaseManifest> CompositionDatabases,
            ProgressCallback? progressCallback = null)
        {
            progressCallback?.Invoke(Common.Messaging.Common.ProcessPhase.ReadingMetadata, true, 0, "Looking up Composition Database in order to find a Base ESD image appropriate for building windows setup files.");

            HashSet<BaseManifest> filteredCompositionDatabases = CompositionDatabases.GetEditionCompDBsForLanguage(LanguageCode);
            if (filteredCompositionDatabases.Count > 0)
            {
                foreach (BaseManifest? currentCompDB in filteredCompositionDatabases)
                {
                    foreach (Package feature in currentCompDB.Features.Feature[0].Packages.Package)
                    {
                        Package pkg = currentCompDB.Packages.Package.First(x => x.ID == feature.ID);

                        string file = pkg.GetCommonlyUsedIncorrectFileName();

                        if (feature.PackageType == "MetadataESD")
                        {
                            if (!File.Exists(Path.Combine(UUPPath, file)))
                            {
                                file = pkg.Payload.PayloadItem[0].Path.Replace('\\', Path.DirectorySeparatorChar);
                                if (!File.Exists(Path.Combine(UUPPath, file)))
                                {
                                    break;
                                }
                            }

                            return (true, Path.Combine(UUPPath, file));
                        }
                    }
                }
            }

            progressCallback?.Invoke(Common.Messaging.Common.ProcessPhase.Error, true, 0, "While looking up the Composition Database, we couldn't find an edition composition for the specified language. This error is fatal.");
            return (false, null);
        }

        internal static bool GenerateAppXLicenseFiles(
            string UUPPath,
            string LanguageCode,
            string EditionID,
            IEnumerable<BaseManifest> CompositionDatabases,
            ProgressCallback? progressCallback = null)
        {
            bool success = true;

            progressCallback?.Invoke(Common.Messaging.Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            BaseManifest? compDB = GetEditionCompDBForLanguage(CompositionDatabases, EditionID, LanguageCode);

            if (compDB == null)
            {
                progressCallback?.Invoke(Common.Messaging.Common.ProcessPhase.ReadingMetadata, true, 0, "No compDB found");
                goto error;
            }

            if (CompositionDatabases.Any(x => x.Name?.StartsWith("Build~") == true && (x.Name?.EndsWith("~Desktop_Apps~~") == true || x.Name?.EndsWith("~Desktop_Apps_Moment~~") == true)))
            {
                IEnumerable<BaseManifest> AppCompDBs = CompositionDatabases.Where(x => x.Name?.StartsWith("Build~") == true && (x.Name?.EndsWith("~Desktop_Apps~~") == true || x.Name?.EndsWith("~Desktop_Apps_Moment~~") == true));
                AppxSelectionEngine.GenerateLicenseXmlFiles(compDB, AppCompDBs, UUPPath);
            }

            goto exit;

        error:
            success = false;

        exit:
            return success;
        }

        internal static (bool Succeeded, string BaseESD, HashSet<string> ReferencePackages, HashSet<string> ReferencePackagesToConvert) LocateFilesForBaseEditionCreation(
            string UUPPath,
            string LanguageCode,
            string EditionID,
            IEnumerable<BaseManifest> CompositionDatabases,
            ProgressCallback? progressCallback = null)
        {
            bool success = true;

            HashSet<string> ReferencePackages = [];
            HashSet<string> referencePackagesToConvert = [];
            string? BaseESD = null;
            progressCallback?.Invoke(Common.Messaging.Common.ProcessPhase.ReadingMetadata, true, 0, "Enumerating files");

            BaseManifest? compDB = GetEditionCompDBForLanguage(CompositionDatabases, EditionID, LanguageCode);

            if (compDB == null)
            {
                progressCallback?.Invoke(Common.Messaging.Common.ProcessPhase.ReadingMetadata, true, 0, "No compDB found");
                goto error;
            }

            foreach (Package feature in compDB.Features.Feature[0].Packages.Package)
            {
                Package pkg = compDB.Packages.Package.First(x => x.ID == feature.ID);

                string file = pkg.GetCommonlyUsedIncorrectFileName();

                if (!File.Exists(Path.Combine(UUPPath, file)))
                {
                    file = pkg.Payload.PayloadItem[0].Path.Replace('\\', Path.DirectorySeparatorChar);
                    if (!File.Exists(Path.Combine(UUPPath, file)))
                    {
                        progressCallback?.Invoke(Common.Messaging.Common.ProcessPhase.ReadingMetadata, true, 0, $"File {file} is missing");
                        goto error;
                    }
                }

                if (feature.PackageType == "MetadataESD")
                {
                    BaseESD = Path.Combine(UUPPath, file);
                    continue;
                }

                if (file.EndsWith(".esd", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ = ReferencePackages.Add(Path.Combine(UUPPath, file));
                }
                else if (file.EndsWith(".cab", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ = referencePackagesToConvert.Add(Path.Combine(UUPPath, file));
                }
            }

            if (BaseESD == null)
            {
                progressCallback?.Invoke(Common.Messaging.Common.ProcessPhase.ReadingMetadata, true, 0, "Base ESD not found");
                goto error;
            }

            goto exit;

        error:
            success = false;

        exit:
            return (success, BaseESD, ReferencePackages, referencePackagesToConvert);
        }
    }
}
