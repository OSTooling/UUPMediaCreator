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
using UnifiedUpdatePlatform.Services.Imaging;
using UnifiedUpdatePlatform.Services.Temp;


#nullable enable

namespace UnifiedUpdatePlatform.Media.Creator.Planning
{
    public class EditionTarget
    {
        public PlannedEdition PlannedEdition { get; set; } = new PlannedEdition();
        public List<EditionTarget> NonDestructiveTargets = [];
        public List<EditionTarget> DestructiveTargets = [];
    }

    public class PlannedEdition
    {
        public string EditionName { get; set; } = "";
        public AvailabilityType AvailabilityType
        {
            get; set;
        }
        public AppxInstallWorkload[] AppXInstallWorkloads { get; set; } = [];
    }

    public enum AvailabilityType
    {
        Canonical,
        EditionUpgrade,
        VirtualEdition,
        EditionPackageSwap
    }

    public static class ConversionPlanBuilder
    {
        public delegate void ProgressCallback(string SubOperation);

        private static readonly IImaging imagingInterface = new WimLibImaging();

        private static EditionTarget BuildTarget(
            PlannedEdition edition,
            List<PlannedEdition> availableEditionsByDowngrading,
            List<EditionMappingXML.Edition> virtualWindowsEditions,
            List<EditionMappingXML.Edition> possibleEditionUpgrades,
            List<PlannedEdition>? availableEditionsByDowngradingInPriority,
            ref List<string> editionsAdded
        )
        {
            EditionTarget target = new()
            {
                PlannedEdition = edition
            };
            editionsAdded.Add(edition.EditionName);

            //
            // Handle edition downgrades that can be done using the current edition
            // We do these first because they potentially can be used for other downgrades, so they should be done first
            //
            if (availableEditionsByDowngradingInPriority != null)
            {
                foreach (KeyValuePair<string, string[]> element in Constants.EditionDowngradeDict)
                {
                    string editionThatCanBePackageSwapped = element.Key;

                    foreach (string? destinationEditionAfterPackageSwap in element.Value)
                    {
                        //
                        // Is the currently examined edition from the dictionary is possible to get with the current UUP set?
                        //
                        if (availableEditionsByDowngradingInPriority.Any(x => x.EditionName.Equals(destinationEditionAfterPackageSwap, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            //
                            // If the edition we want to target is one edition that can be used for package swapping
                            //
                            if (edition.EditionName.Equals(editionThatCanBePackageSwapped, StringComparison.InvariantCultureIgnoreCase))
                            {
                                //
                                // If we have not added this edition already
                                //
                                if (!editionsAdded.Any(x => x.Equals(destinationEditionAfterPackageSwap, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    //
                                    // Get the edition that can be targeted using package swap
                                    //
                                    PlannedEdition? planedition = availableEditionsByDowngradingInPriority.First(x => x.EditionName.Equals(destinationEditionAfterPackageSwap, StringComparison.InvariantCultureIgnoreCase));

                                    //
                                    // Remove it from the list of editions that can be targeted, as we are targeting it here so it doesn't get picked up again
                                    //
                                    _ = availableEditionsByDowngradingInPriority.Remove(planedition);

                                    //
                                    // Add the edition
                                    //
                                    target.DestructiveTargets.Add(BuildTarget(planedition, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, availableEditionsByDowngradingInPriority, ref editionsAdded));
                                }
                            }
                        }
                    }
                }
            }

            //
            // Handle editions that can be obtained by upgrading to a virtual edition
            // Loop through all editions we can upgrade to with the current examined edition
            //
            foreach (EditionMappingXML.Edition? ed in virtualWindowsEditions.Where(x => x.ParentEdition.Equals(edition.EditionName, StringComparison.InvariantCultureIgnoreCase)))
            {
                //
                // Verify that we have not added this edition already
                //
                if (editionsAdded.Any(x => x.Equals(ed.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                editionsAdded.Add(ed.Name);

                PlannedEdition plannedEdition = new()
                {
                    AvailabilityType = AvailabilityType.VirtualEdition,
                    EditionName = ed.Name
                };

                target.NonDestructiveTargets.Add(BuildTarget(plannedEdition, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
            }

            //
            // Sort editions by name
            //
            target.NonDestructiveTargets = [.. target.NonDestructiveTargets.OrderBy(x => x.PlannedEdition.EditionName)];

            //
            // Handle editions that we can upgrade to using a full upgrade process
            //
            foreach (EditionMappingXML.Edition? ed in possibleEditionUpgrades.Where(x => x.ParentEdition.Equals(edition.EditionName, StringComparison.InvariantCultureIgnoreCase)))
            {
                //
                // Verify that we have not added this edition already
                //
                if (editionsAdded.Any(x => x.Equals(ed.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                editionsAdded.Add(ed.Name);

                PlannedEdition plannedEdition = new()
                {
                    AvailabilityType = AvailabilityType.EditionUpgrade,
                    EditionName = ed.Name
                };

                target.DestructiveTargets.Add(BuildTarget(plannedEdition, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
            }

            //
            // Handle editions that can be obtained by doing a package swap
            //
            IEnumerable<PlannedEdition>? CoreEditionsToBeHacked = availableEditionsByDowngrading.Where(x =>
            x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            !(x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            IEnumerable<PlannedEdition>? CoreEditionsWithNoMediaTechnologiesToBeHacked = availableEditionsByDowngrading.Where(x =>
            x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            (x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            IEnumerable<PlannedEdition>? ProfessionalEditionsToBeHacked = availableEditionsByDowngrading.Where(x =>
            !x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            !(x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            IEnumerable<PlannedEdition>? ProfessionalEditionsWithNoMediaTechnologiesToBeHacked = availableEditionsByDowngrading.Where(x =>
            !x.EditionName.StartsWith("core", StringComparison.InvariantCultureIgnoreCase) &&
            (x.EditionName.EndsWith("n", StringComparison.InvariantCultureIgnoreCase) || x.EditionName.EndsWith("neval", StringComparison.InvariantCultureIgnoreCase)));

            if (edition.EditionName.Equals("professional", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (PlannedEdition? ed in ProfessionalEditionsToBeHacked)
                {
                    //
                    // Verify that we have not added this edition already
                    //
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
                }
            }
            else if (edition.EditionName.Equals("professionaln", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (PlannedEdition? ed in ProfessionalEditionsWithNoMediaTechnologiesToBeHacked)
                {
                    //
                    // Verify that we have not added this edition already
                    //
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
                }
            }
            else if (edition.EditionName.Equals("core", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (PlannedEdition? ed in CoreEditionsToBeHacked)
                {
                    //
                    // Verify that we have not added this edition already
                    //
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
                }
            }
            else if (edition.EditionName.Equals("coren", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (PlannedEdition? ed in CoreEditionsWithNoMediaTechnologiesToBeHacked)
                {
                    //
                    // Verify that we have not added this edition already
                    //
                    if (editionsAdded.Any(x => x.Equals(ed.EditionName, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        continue;
                    }

                    editionsAdded.Add(ed.EditionName);

                    target.DestructiveTargets.Add(BuildTarget(ed, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, null, ref editionsAdded));
                }
            }

            target.DestructiveTargets = [.. target.DestructiveTargets.OrderBy(x => x.PlannedEdition.EditionName)];

            return target;
        }

        private static (List<PlannedEdition>, List<PlannedEdition>) GetEditionsThatCanBeTargetedUsingPackageDowngrade(
            string UUPPath,
            IEnumerable<BaseManifest> compDBs,
            IEnumerable<PlannedEdition> availableCanonicalEditions,
            List<EditionMappingXML.Edition> possibleEditionUpgrades,
            ProgressCallback? progressCallback = null)
        {
            List<PlannedEdition> availableEditionsByDowngradingInPriority = [];
            List<PlannedEdition> availableEditionsByDowngrading = [];

            //
            // Attempt to get the neutral Composition Database listing all available files
            //
            BaseManifest? neutralCompDB = compDBs.GetNeutralCompDB();

            if (neutralCompDB != null &&
                neutralCompDB.Features.Feature.FirstOrDefault(x => x.FeatureID == "BaseNeutral")?
                .Packages.Package
                is List<Package> packages)
            {
                IEnumerable<Package>? editionSpecificPackages = packages.Where(x => x.ID.Count(y => y == '-') == 4 && x.ID.Contains("microsoft-windows-editionspecific", StringComparison.InvariantCultureIgnoreCase));
                IEnumerable<Package>? highPriorityPackages = editionSpecificPackages.Where(x => x.ID.Contains("starter", StringComparison.InvariantCultureIgnoreCase) ||
                                                                            x.ID.Contains("professional", StringComparison.InvariantCultureIgnoreCase) ||
                                                                            x.ID.Contains("core", StringComparison.InvariantCultureIgnoreCase));
                editionSpecificPackages = editionSpecificPackages.Except(highPriorityPackages);

                foreach (Package? file in highPriorityPackages)
                {
                    Package pkg = neutralCompDB.Packages.Package.First(x => x.ID == file.ID);

                    //
                    // First check if the file exists
                    //
                    if (!string.IsNullOrEmpty(UUPPath))
                    {
                        (bool Success, string MissingFile) = FileLocator.VerifyFileIsAvailableForPackage(pkg, UUPPath);
                        if (!Success)
                        {
                            progressCallback?.Invoke("One edition Composition Database failed file validation, below is highlighted the files that could not be found in the UUP path. This means that you will not get all possible editions.");
                            progressCallback?.Invoke($"Missing: {MissingFile}");
                            continue;
                        }
                    }

                    string? sku = file.ID.Split('-')[3];

                    //
                    // If the edition not available as canonical
                    //
                    if (!availableCanonicalEditions.Any(x => x.EditionName.Equals(sku, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (possibleEditionUpgrades?
                            .Where(x => availableCanonicalEditions.Any(y => y.EditionName.Equals(x.ParentEdition, StringComparison.InvariantCultureIgnoreCase)) ||
                                        availableEditionsByDowngrading.Any(y => y.EditionName.Equals(x.ParentEdition, StringComparison.InvariantCultureIgnoreCase)))
                            .Any(x => x.Name.Equals(sku, StringComparison.InvariantCultureIgnoreCase)) == false)
                        {
                            PlannedEdition edition = new()
                            {
                                EditionName = sku,
                                AvailabilityType = AvailabilityType.EditionPackageSwap
                            };

                            availableEditionsByDowngradingInPriority.Add(edition);
                        }
                    }
                }

                foreach (Package? file in editionSpecificPackages)
                {
                    Package pkg = neutralCompDB.Packages.Package.First(x => x.ID == file.ID);

                    //
                    // First check if the file exists
                    //
                    if (!string.IsNullOrEmpty(UUPPath))
                    {
                        (bool Success, string MissingFile) = FileLocator.VerifyFileIsAvailableForPackage(pkg, UUPPath);
                        if (!Success)
                        {
                            progressCallback?.Invoke("One edition Composition Database failed file validation, below is highlighted the files that could not be found in the UUP path. This means that you will not get all possible editions.");
                            progressCallback?.Invoke($"Missing: {MissingFile}");
                            continue;
                        }
                    }

                    string? sku = file.ID.Split('-')[3];

                    //
                    // If the edition not available as canonical
                    //
                    if (!availableCanonicalEditions.Any(x => x.EditionName.Equals(sku, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        if (possibleEditionUpgrades?
                            .Where(x => availableCanonicalEditions.Any(y => y.EditionName.Equals(x.ParentEdition, StringComparison.InvariantCultureIgnoreCase)) ||
                                        availableEditionsByDowngrading.Any(y => y.EditionName.Equals(x.ParentEdition, StringComparison.InvariantCultureIgnoreCase)))
                            .Any(x => x.Name.Equals(sku, StringComparison.InvariantCultureIgnoreCase)) == false)
                        {
                            PlannedEdition edition = new()
                            {
                                EditionName = sku,
                                AvailabilityType = AvailabilityType.EditionPackageSwap
                            };

                            availableEditionsByDowngrading.Add(edition);
                        }
                    }
                }
            }

            return (availableEditionsByDowngradingInPriority, availableEditionsByDowngrading);
        }

        public static bool GetTargetedPlan(
            IEnumerable<BaseManifest> compDBs,
            string EditionPack,
            string LanguageCode,
            bool IncludeServicingCapableOnlyTargets,
            out List<EditionTarget> EditionTargets,
            TempManager tempManager,
            ProgressCallback? progressCallback = null)
        {
            return GetTargetedPlan("", compDBs, EditionPack, LanguageCode, IncludeServicingCapableOnlyTargets, out EditionTargets, tempManager, progressCallback);
        }

        public static List<string> PrintEditionTarget(EditionTarget editionTarget, int padding = 0)
        {
            List<string> lines =
            [
                $"-> Name: {editionTarget.PlannedEdition.EditionName}, Availability: {editionTarget.PlannedEdition.AvailabilityType}"
            ];

            if (editionTarget.PlannedEdition.AppXInstallWorkloads?.Length > 0)
            {
                lines.Add($"-> Apps: ");
                foreach (AppxInstallWorkload app in editionTarget.PlannedEdition.AppXInstallWorkloads)
                {
                    lines.Add($"   " + app.AppXPath);
                }
            }

            if (editionTarget.NonDestructiveTargets.Count > 0)
            {
                lines.Add("   Non Destructive Edition Upgrade Targets:");
                foreach (EditionTarget? ed in editionTarget.NonDestructiveTargets)
                {
                    lines.AddRange(PrintEditionTarget(ed, padding + 1));
                }
            }
            if (editionTarget.DestructiveTargets.Count > 0)
            {
                lines.Add("   Destructive Edition Upgrade Targets:");
                foreach (EditionTarget? ed in editionTarget.DestructiveTargets)
                {
                    lines.AddRange(PrintEditionTarget(ed, padding + 1));
                }
            }

            for (int j = 0; j < lines.Count; j++)
            {
                for (int i = 0; i < padding; i++)
                {
                    lines[j] = "   " + lines[j];
                }
            }

            return lines;
        }

        public static bool GetTargetedPlan(
            string UUPPath,
            IEnumerable<BaseManifest> compDBs,
            string EditionPack,
            string LanguageCode,
            bool IncludeServicingCapableOnlyTargets,
            out List<EditionTarget> EditionTargets,
            TempManager tempManager,
            ProgressCallback? progressCallback = null)
        {
            bool VerifyFiles = !string.IsNullOrEmpty(UUPPath);

            EditionTargets = [];

            bool result = true;

            progressCallback?.Invoke("Acquiring Base Editions");

            //
            // Get base editions that are available with all their files
            //
            IEnumerable<BaseManifest> filteredCompDBs = compDBs.GetEditionCompDBsForLanguage(LanguageCode).Where(x =>
            {
                bool success = !VerifyFiles;
                if (!success)
                {
                    (bool success2, HashSet<string> missingfiles) = FileLocator.VerifyFilesAreAvailableForCompDB(x, UUPPath);
                    success = success2;

                    if (!success)
                    {
                        progressCallback?.Invoke("One edition Composition Database failed file validation, below is highlighted the files that could not be found in the UUP path. This means that you will not get all possible editions.");
                        foreach (string? file in missingfiles)
                        {
                            progressCallback?.Invoke($"Missing: {file}");
                        }
                    }
                }

                return success;
            });

            if (!filteredCompDBs.Any())
            {
                progressCallback?.Invoke("No edition CompDB validated or were found, this is a fatal error, as one edition CompDB is needed at the very least for creating an ISO.");
                goto error;
            }

            progressCallback?.Invoke("Adding Base Editions to the conversion plan");

            //
            // Add available canonical editions
            //
            IEnumerable<PlannedEdition> availableCanonicalEditions = filteredCompDBs.Select(compDB =>
            {
                PlannedEdition edition = new()
                {
                    AvailabilityType = AvailabilityType.Canonical
                };

                if (compDBs.Any(x => x.Name?.StartsWith("Build~") == true && (x.Name?.EndsWith("~Desktop_Apps~~") == true || x.Name?.EndsWith("~Desktop_Apps_Moment~~") == true)))
                {
                    IEnumerable<BaseManifest> AppCompDBs = compDBs.Where(x => x.Name?.StartsWith("Build~") == true && (x.Name?.EndsWith("~Desktop_Apps~~") == true || x.Name?.EndsWith("~Desktop_Apps_Moment~~") == true));
                    edition.AppXInstallWorkloads = AppxSelectionEngine.GetAppxInstallationWorkloads(compDB, AppCompDBs, LanguageCode);
                }

                edition.EditionName = compDB.Tags != null
                    ? compDB.Tags.Tag.First(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)).Value
                    : compDB.Features.Feature[0].FeatureID.Split('_')[0];

                return edition;
            }).OrderBy(x => x.EditionName);

            if (IncludeServicingCapableOnlyTargets)
            {
                progressCallback?.Invoke("Acquiring Edition Upgrades");

                //
                // This dictionary holds the possible virtual edition upgrades
                // Example: Professional -> ProfessionalEducation
                //
                List<EditionMappingXML.Edition> virtualWindowsEditions = [];

                //
                // This dictionary holds the possible edition upgrades
                // Example: Core -> Professional
                //
                List<EditionMappingXML.Edition> possibleEditionUpgrades = [];

                if (!string.IsNullOrEmpty(EditionPack) && File.Exists(EditionPack))
                {
                    //
                    // Attempt to get virtual editions from one compdb
                    //
                    if (virtualWindowsEditions.Count == 0)
                    {
                        try
                        {
                            string? tempHashMap = tempManager.GetTempPath();

                            string pathEditionMapping = "";
                            int index = 0;

                            result = imagingInterface.ExtractFileFromImage(EditionPack, 1, "$filehashes$.dat", tempHashMap);
                            if (result)
                            {
                                string[] hashmapcontent = File.ReadAllLines(tempHashMap);
                                File.Delete(tempHashMap);
                                pathEditionMapping = hashmapcontent.First(x => x.Contains("editionmappings.xml", StringComparison.CurrentCultureIgnoreCase)).Split('=')[0];
                                index = 1;
                            }
                            else
                            {
                                result = true;
                                pathEditionMapping = $"Windows{Path.DirectorySeparatorChar}Servicing{Path.DirectorySeparatorChar}Editions{Path.DirectorySeparatorChar}EditionMappings.xml";
                                index = 3;
                            }

                            try
                            {
                                result = imagingInterface.ExtractFileFromImage(EditionPack, index, pathEditionMapping, tempHashMap);
                                if (!result)
                                {
                                    goto error;
                                }

                                string editionmappingcontent = File.ReadAllText(tempHashMap);
                                File.Delete(tempHashMap);

                                EditionMappingXML.WindowsEditions? mapping = EditionMappingXML.Deserialize(editionmappingcontent);
                                IOrderedEnumerable<EditionMappingXML.Edition>? virtualeditions = mapping.Edition.Where(x =>
                                    !string.IsNullOrEmpty(x.Virtual) &&
                                    x.Virtual.Equals("true", StringComparison.InvariantCultureIgnoreCase)).OrderBy(x => x.ParentEdition);

                                virtualWindowsEditions = [.. virtualeditions];
                            }
                            catch { }
                        }
                        catch { }
                    }

                    //
                    // Attempt to get upgradable editions from one compdb
                    //
                    if (possibleEditionUpgrades.Count == 0)
                    {
                        try
                        {
                            string tempHashMap = tempManager.GetTempPath();

                            string pathEditionMatrix = "";
                            int index = 0;

                            result = imagingInterface.ExtractFileFromImage(EditionPack, 1, "$filehashes$.dat", tempHashMap);
                            if (result)
                            {
                                string[] hashmapcontent = File.ReadAllLines(tempHashMap);
                                File.Delete(tempHashMap);
                                pathEditionMatrix = hashmapcontent.First(x => x.Contains("editionmatrix.xml", StringComparison.CurrentCultureIgnoreCase)).Split('=')[0];
                                index = 1;
                            }
                            else
                            {
                                result = true;
                                pathEditionMatrix = $"Windows{Path.DirectorySeparatorChar}Servicing{Path.DirectorySeparatorChar}Editions{Path.DirectorySeparatorChar}EditionMatrix.xml";
                                index = 3;
                            }

                            try
                            {
                                result = imagingInterface.ExtractFileFromImage(EditionPack, index, pathEditionMatrix, tempHashMap);
                                if (!result)
                                {
                                    goto error;
                                }

                                string editionmatrixcontent = File.ReadAllText(tempHashMap);
                                File.Delete(tempHashMap);

                                EditionMatrixXML.TmiMatrix? mapping = EditionMatrixXML.Deserialize(editionmatrixcontent);
                                foreach (EditionMatrixXML.Edition? edition in mapping.Edition)
                                {
                                    if (edition.Target != null && edition.Target.Count != 0)
                                    {
                                        foreach (EditionMatrixXML.Target? target in edition.Target)
                                        {
                                            if (!availableCanonicalEditions.Any(x => x.EditionName.Equals(target.ID, StringComparison.InvariantCultureIgnoreCase)))
                                            {
                                                possibleEditionUpgrades.Add(new EditionMappingXML.Edition() { ParentEdition = edition.ID, Name = target.ID, Virtual = "false" });
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                        catch { }
                    }
                }

                progressCallback?.Invoke("Acquiring Edition Downgrades");

                (List<PlannedEdition> availableEditionsByDowngradingInPriority, List<PlannedEdition> availableEditionsByDowngrading) = GetEditionsThatCanBeTargetedUsingPackageDowngrade(UUPPath, compDBs, availableCanonicalEditions, possibleEditionUpgrades);

                progressCallback?.Invoke("Building Targets");

                List<string> editionsAdded = [];

                foreach (PlannedEdition? ed in availableCanonicalEditions)
                {
                    EditionTargets.Add(BuildTarget(ed, availableEditionsByDowngrading, virtualWindowsEditions, possibleEditionUpgrades, availableEditionsByDowngradingInPriority, ref editionsAdded));
                }
            }
            else
            {
                progressCallback?.Invoke("Building Targets");

                foreach (PlannedEdition? ed in availableCanonicalEditions)
                {
                    EditionTargets.Add(new EditionTarget() { PlannedEdition = ed });
                }
            }

        error:
            return result;
        }
    }
}