/*
 * Copyright (c) The UUP Media Creator authors and Contributors
 * 
 * Written by and used with permission from
 * @thebookisclosed (https://github.com/thebookisclosed)
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnifiedUpdatePlatform.Services.Composition.Database.Applications
{
    public static class AppxSelectionEngine
    {
        /// <summary>
        /// Internal Variable setup function
        /// </summary>
        /// <param name="editionCdb"></param>
        /// <param name="appsCdb"></param>
        /// <param name="repositoryPath"></param>
        /// <returns></returns>
        private static (Dictionary<string, DeploymentProperties> preinstalledApps, Feature[] appsFeatures) SetupVariables(BaseManifest editionCdb, IEnumerable<BaseManifest> appsCdbs)
        {
            Dictionary<string, DeploymentProperties> preinstalledApps = editionCdb.Features.Feature
                .First(x => x.Type == "DesktopMedia")
                .Dependencies
                .Feature
                .Where(x => x.Group == "PreinstalledApps")
                .Select(x => x.FeatureID)
                .Distinct()
                .ToDictionary(x => x, _ => new DeploymentProperties());

            List<Feature> appsFeatures = [];

            foreach (BaseManifest appsCdb in appsCdbs)
            {
                appsFeatures.AddRange(appsCdb.Features.Feature);
            }

            // Load dependencies & intents
            foreach (Feature ftr in appsFeatures)
            {
                string appFeatureId = ftr.FeatureID;
                if (!preinstalledApps.ContainsKey(appFeatureId))
                {
                    continue;
                }

                DeploymentProperties deployProps = preinstalledApps[appFeatureId];

                bool preferStub = ftr.InitialIntents?.InitialIntent
                    .Any(x => x.Value == "PREFERSTUB") ?? false;

                if (preferStub)
                {
                    deployProps.PreferStub = true;
                }

                List<Feature> dependencies = ftr.Dependencies?.Feature;
                if (dependencies == null)
                {
                    continue;
                }

                HashSet<string> depsForApp = [];
                foreach (Feature dep in dependencies)
                {
                    string depAppId = dep.FeatureID;
                    if (!preferStub)
                    {
                        preinstalledApps[depAppId] = new DeploymentProperties();
                        _ = depsForApp.Add(depAppId);
                    }
                    else if (depAppId.StartsWith("Microsoft.VCLibs.140.00_"))
                    {
                        preinstalledApps[depAppId] = new DeploymentProperties();
                        _ = depsForApp.Add(depAppId);
                        break;
                    }
                }
                preinstalledApps[appFeatureId].Dependencies = depsForApp;
            }

            return (preinstalledApps, appsFeatures.ToArray());
        }

        /// <summary>
        /// Generates License XML files for AppX packages
        /// </summary>
        /// <param name="editionCdb">The edition Composition Database to generate licenses for</param>
        /// <param name="appsCdb">The application Composition Database</param>
        /// <param name="repositoryPath">The path to the repository file set</param>
        public static void GenerateLicenseXmlFiles(BaseManifest editionCdb, IEnumerable<BaseManifest> appsCdbs, string repositoryPath)
        {
            (Dictionary<string, DeploymentProperties> preinstalledApps, Feature[] appsFeatures) = SetupVariables(editionCdb, appsCdbs);

            // Pick packages and dump licenses
            foreach (Feature ftr in appsFeatures)
            {
                string appFeatureId = ftr.FeatureID;
                if (!preinstalledApps.ContainsKey(appFeatureId))
                {
                    continue;
                }

                string licenseData = ftr.CustomInformation?.CustomInfo
                    .FirstOrDefault(x => x.Key == "licensedata")?.Value;
                if (licenseData != null)
                {
                    string LicenseDirectory = Path.Combine(repositoryPath, "Licenses");
                    if (!Directory.Exists(LicenseDirectory))
                    {
                        _ = Directory.CreateDirectory(LicenseDirectory);
                    }
                    File.WriteAllText(Path.Combine(LicenseDirectory, appFeatureId + "_License.xml"), licenseData);
                }
            }
        }

        /// <summary>
        /// Gathers a list of AppX files to install for a target edition
        /// </summary>
        /// <param name="editionCdb">The edition Composition Database to generate workloads for</param>
        /// <param name="appsCdb">The application Composition Database</param>
        /// <param name="repositoryPath">The path to the repository file set</param>
        /// <returns></returns>
        public static AppxInstallWorkload[] GetAppxInstallationWorkloads(BaseManifest editionCdb, IEnumerable<BaseManifest> appsCdbs, string editionLanguage)
        {
            List<AppxInstallWorkload> workloads = [];

            IEnumerable<string> applicableLanguageTags = GetAllPossibleLanguageCombinations(editionLanguage);

            (Dictionary<string, DeploymentProperties> preinstalledApps, Feature[] appsFeatures) = SetupVariables(editionCdb, appsCdbs);

            HashSet<string> allPackageIDs = [];
            // Pick packages and dump licenses
            foreach (Feature ftr in appsFeatures)
            {
                string appFeatureId = ftr.FeatureID;
                if (!preinstalledApps.ContainsKey(appFeatureId))
                {
                    continue;
                }

                DeploymentProperties deployProps = preinstalledApps[appFeatureId];

                bool isFramework = ftr.Type == "MSIXFramework";
                if (isFramework)
                {
                    deployProps.IsFramework = true;
                }

                string licenseData = ftr.CustomInformation?.CustomInfo
                    .FirstOrDefault(x => x.Key == "licensedata")?.Value;
                if (licenseData != null)
                {
                    deployProps.HasLicense = true;
                }

                List<Package> appPackages = ftr.Packages.Package;
                deployProps.AddApplicablePackages(appPackages, applicableLanguageTags);

                foreach (string pid in deployProps.PackageIDs)
                {
                    _ = allPackageIDs.Add(pid);
                }
            }

            Dictionary<string, PackageProperties> packageHashDict = [];
            foreach (BaseManifest appsCdb in appsCdbs)
            {
                foreach (Package p in appsCdb.Packages.Package)
                {
                    string packageId = p.ID;
                    if (!allPackageIDs.Contains(packageId))
                    {
                        continue;
                    }

                    PayloadItem pCanonical = p
                        .Payload
                        .PayloadItem
                        .First(x => x.PayloadType == "Canonical");
                    packageHashDict[packageId] = new PackageProperties()
                    {
                        Path = pCanonical.Path,
                        SHA256 = pCanonical.PayloadHash
                    };
                }
            }

            foreach (KeyValuePair<string, DeploymentProperties> deployKvp in preinstalledApps.Where(x => !x.Value.IsFramework))
            {
                DeploymentProperties deployProps = deployKvp.Value;
                AppxInstallWorkload workload = new()
                {
                    AppXPath = packageHashDict[deployProps.MainPackageID].Path
                };

                if (deployProps.Dependencies != null)
                {
                    List<string> dependencies = [];
                    foreach (string dependency in deployProps.Dependencies)
                    {
                        DeploymentProperties dependProps = preinstalledApps[dependency];
                        dependencies.Add(packageHashDict[dependProps.MainPackageID].Path);
                    }
                    workload.DependenciesPath = [.. dependencies];
                }

                if (deployProps.HasLicense)
                {
                    workload.LicensePath = Path.Combine("Licenses", deployKvp.Key + "_License.xml");
                }

                if (deployProps.PreferStub)
                {
                    workload.StubPackageOption = "installstub";
                }

                workloads.Add(workload);
            }

            return [.. workloads];
        }

        private static readonly Dictionary<string, string[]> languageMap = new()
        {
            { "af-za", new[] { "af", "af-za" } },
            { "af", new[] { "af" } },
            { "am-et", new[] { "am", "am-et" } },
            { "am", new[] { "am" } },
            { "ar-sa", new[] { "ar", "ar-sa" } },
            { "ar", new[] { "ar" } },
            { "as-in", new[] { "as" } },
            { "as", new[] { "as" } },
            { "az-latn-az", new[] { "az-latn", "az-latn-az" } },
            { "az-latn", new[] { "az-latn" } },
            { "be-by", new[] { "be", "be-by" } },
            { "be", new[] { "be" } },
            { "bg-bg", new[] { "bg", "bg-bg" } },
            { "bg", new[] { "bg" } },
            { "bn-bd", new[] { "bn", "bn-bd" } },
            { "bn-in", new[] { "bn" } },
            { "bn", new[] { "bn" } },
            { "bs-latn-ba", new[] { "bs", "bs-latn-ba" } },
            { "bs", new[] { "bs" } },
            { "ca-es", new[] { "ca", "ca-es" } },
            { "ca", new[] { "ca" } },
            { "chr-cher-us", new[] { "chr-cher" } },
            { "chr-cher", new[] { "chr-cher" } },
            { "cs-cz", new[] { "cs", "cs-cz" } },
            { "cs", new[] { "cs" } },
            { "cy-gb", new[] { "cy" } },
            { "cy", new[] { "cy" } },
            { "da-dk", new[] { "da", "da-dk" } },
            { "da", new[] { "da" } },
            { "de-de", new[] { "de", "de-de" } },
            { "de", new[] { "de" } },
            { "el-gr", new[] { "el", "el-gr" } },
            { "el", new[] { "el" } },
            { "es-019", new[] { "es" } },
            { "es-es", new[] { "es", "es-es" } },
            { "es-mx", new[] { "es-mx" } },
            { "es", new[] { "es" } },
            { "et-ee", new[] { "et", "et-ee" } },
            { "et", new[] { "et" } },
            { "eu-es", new[] { "eu", "eu-es" } },
            { "eu", new[] { "eu" } },
            { "fa-ir", new[] { "fa", "fa-ir" } },
            { "fa", new[] { "fa" } },
            { "fi-fi", new[] { "fi", "fi-fi" } },
            { "fi", new[] { "fi" } },
            { "fil-ph", new[] { "fil-latn", "fil-ph" } },
            { "fil", new[] { "fil-latn" } },
            { "fr-ca", new[] { "fr", "fr-ca" } },
            { "fr-fr", new[] { "fr-fr", "fr" } },
            { "fr", new[] { "fr" } },
            { "ga-ie", new[] { "ga" } },
            { "ga", new[] { "ga" } },
            { "gd-gb", new[] { "gd-latn" } },
            { "gd-latn", new[] { "gd-latn" } },
            { "gl-es", new[] { "gl", "gl-es" } },
            { "gl", new[] { "gl" } },
            { "gu-in", new[] { "gu" } },
            { "gu", new[] { "gu" } },
            { "ha-latn-ng", new[] { "ha-latn", "ha-latn-ng" } },
            { "ha-latn", new[] { "ha-latn" } },
            { "he-il", new[] { "he", "he-il" } },
            { "he", new[] { "he" } },
            { "hi-in", new[] { "hi", "hi-in" } },
            { "hi", new[] { "hi" } },
            { "hr-hr", new[] { "hr", "hr-hr" } },
            { "hr", new[] { "hr" } },
            { "hu-hu", new[] { "hu", "hu-hu" } },
            { "hu", new[] { "hu" } },
            { "hy-am", new[] { "hy" } },
            { "hy", new[] { "hy" } },
            { "id-id", new[] { "id", "id-id" } },
            { "id", new[] { "id" } },
            { "ig-latn", new[] { "ig-latn" } },
            { "ig-ng", new[] { "ig-latn" } },
            { "is-is", new[] { "is", "is-is" } },
            { "is", new[] { "is" } },
            { "it-it", new[] { "it", "it-it" } },
            { "it", new[] { "it" } },
            { "ja-jp", new[] { "ja", "ja-jp" } },
            { "ja", new[] { "ja" } },
            { "ka-ge", new[] { "ka" } },
            { "ka", new[] { "ka" } },
            { "kk-kz", new[] { "kk", "kk-kz" } },
            { "kk", new[] { "kk" } },
            { "km-kh", new[] { "km", "km-kh" } },
            { "km", new[] { "km" } },
            { "kn-in", new[] { "kn", "kn-in" } },
            { "kn", new[] { "kn" } },
            { "ko-kr", new[] { "ko", "ko-kr" } },
            { "ko", new[] { "ko" } },
            { "kok-in", new[] { "kok" } },
            { "kok", new[] { "kok" } },
            { "ku-arab-iq", new[] { "ku-arab" } },
            { "ku-arab", new[] { "ku-arab" } },
            { "ky-cyrl", new[] { "ky-cyrl" } },
            { "ky-kg", new[] { "ky-cyrl" } },
            { "lb-lu", new[] { "lb" } },
            { "lb", new[] { "lb" } },
            { "lo-la", new[] { "lo", "lo-la" } },
            { "lo", new[] { "lo" } },
            { "lt-lt", new[] { "lt", "lt-lt" } },
            { "lt", new[] { "lt" } },
            { "lv-lv", new[] { "lv", "lv-lv" } },
            { "lv", new[] { "lv" } },
            { "mi-latn", new[] { "mi-latn" } },
            { "mi-nz", new[] { "mi-latn" } },
            { "mi", new[] { "mi-latn" } },
            { "mk-mk", new[] { "mk", "mk-mk" } },
            { "mk", new[] { "mk" } },
            { "ml-in", new[] { "ml", "ml-in" } },
            { "ml", new[] { "ml" } },
            { "mn-cyrl", new[] { "mn-cyrl" } },
            { "mn-mn", new[] { "mn-cyrl" } },
            { "mr-in", new[] { "mr" } },
            { "mr", new[] { "mr" } },
            { "ms-my", new[] { "ms", "ms-my" } },
            { "ms", new[] { "ms" } },
            { "mt-mt", new[] { "mt" } },
            { "mt", new[] { "mt" } },
            { "nb-no", new[] { "nb", "nb-no" } },
            { "nb", new[] { "nb" } },
            { "ne-np", new[] { "ne" } },
            { "ne", new[] { "ne" } },
            { "nl-nl", new[] { "nl", "nl-nl" } },
            { "nl", new[] { "nl" } },
            { "nn-no", new[] { "nn", "nn-no" } },
            { "nn", new[] { "nn" } },
            { "nso-za", new[] { "nso" } },
            { "nso", new[] { "nso" } },
            { "or-in", new[] { "or" } },
            { "or", new[] { "or" } },
            { "pa-arab-pk", new[] { "pa-arab" } },
            { "pa-arab", new[] { "pa-arab" } },
            { "pa-in", new[] { "pa" } },
            { "pa", new[] { "pa" } },
            { "pl-pl", new[] { "pl", "pl-pl" } },
            { "pl", new[] { "pl" } },
            { "prs-af", new[] { "prs-arab" } },
            { "prs-arab", new[] { "prs-arab" } },
            { "prs", new[] { "prs-arab" } },
            { "pt-br", new[] { "pt", "pt-br" } },
            { "pt-pt", new[] { "pt-pt" } },
            { "pt", new[] { "pt" } },
            { "qps-ploc", new[] { "qps-ploc" } },
            { "qps-ploca", new[] { "qps-ploca" } },
            { "qps-plocm", new[] { "qps-plocm" } },
            { "quc-latn", new[] { "quc-latn" } },
            { "quz-pe", new[] { "quz-latn" } },
            { "quz", new[] { "quz-latn" } },
            { "ro-ro", new[] { "ro", "ro-ro" } },
            { "ro", new[] { "ro" } },
            { "ru-ru", new[] { "ru", "ru-ru" } },
            { "ru", new[] { "ru" } },
            { "rw-rw", new[] { "rw" } },
            { "rw", new[] { "rw" } },
            { "sd-arab-pk", new[] { "sd-arab" } },
            { "sd-arab", new[] { "sd-arab" } },
            { "si-lk", new[] { "si" } },
            { "si", new[] { "si" } },
            { "sk-sk", new[] { "sk", "sk-sk" } },
            { "sk", new[] { "sk" } },
            { "sl-si", new[] { "sl", "sl-si" } },
            { "sl", new[] { "sl" } },
            { "sq-al", new[] { "sq", "sq-al" } },
            { "sq", new[] { "sq" } },
            { "sr-cyrl-ba", new[] { "sr-cyrl" } },
            { "sr-cyrl-rs", new[] { "sr-cyrl-rs", "sr-cyrl" } },
            { "sr-latn-rs", new[] { "sr-latn", "sr-latn-rs" } },
            { "sr-latn", new[] { "sr-latn" } },
            { "sv-se", new[] { "sv", "sv-se" } },
            { "sv", new[] { "sv" } },
            { "sw-ke", new[] { "sw", "sw-ke" } },
            { "sw", new[] { "sw" } },
            { "ta-in", new[] { "ta", "ta-in" } },
            { "ta", new[] { "ta" } },
            { "te-in", new[] { "te", "te-in" } },
            { "te", new[] { "te" } },
            { "tg-cyrl-tj", new[] { "tg-cyrl" } },
            { "tg-cyrl", new[] { "tg-cyrl" } },
            { "th-th", new[] { "th", "th-th" } },
            { "th", new[] { "th" } },
            { "ti-et", new[] { "ti" } },
            { "ti", new[] { "ti" } },
            { "tk-latn", new[] { "tk-latn" } },
            { "tk-tm", new[] { "tk-latn" } },
            { "tn-za", new[] { "tn" } },
            { "tn", new[] { "tn" } },
            { "tr-tr", new[] { "tr", "tr-tr" } },
            { "tr", new[] { "tr" } },
            { "tt-cyrl", new[] { "tt-cyrl" } },
            { "tt-ru", new[] { "tt-cyrl" } },
            { "ug-arab", new[] { "ug-arab" } },
            { "ug-cn", new[] { "ug-arab" } },
            { "uk-ua", new[] { "uk", "uk-ua" } },
            { "uk", new[] { "uk" } },
            { "ur-pk", new[] { "ur" } },
            { "ur", new[] { "ur" } },
            { "uz-latn-uz", new[] { "uz-latn", "uz-latn-uz" } },
            { "uz-latn", new[] { "uz-latn" } },
            { "vi-vn", new[] { "vi", "vi-vn" } },
            { "vi", new[] { "vi" } },
            { "wo-sn", new[] { "wo-latn" } },
            { "wo", new[] { "wo-latn" } },
            { "xh-za", new[] { "xh" } },
            { "xh", new[] { "xh" } },
            { "yo-latn", new[] { "yo-latn" } },
            { "yo-ng", new[] { "yo-latn" } },
            { "zh-cn", new[] { "zh-hans", "zh-cn" } },
            { "zh-hans", new[] { "zh-hans" } },
            { "zh-hant-hk", new[] { "zh-hant" } },
            { "zh-hant", new[] { "zh-hant" } },
            { "zh-hk", new[] { "zh-hant" } },
            { "zh-tw", new[] { "zh-hant", "zh-tw" } },
            { "zu-za", new[] { "zu" } },
            { "zu", new[] { "zu" } },
        };

        // TODO: Extract AppX bundle manifest from the main package, and get the language this way.
        private static IEnumerable<string> GetAllPossibleLanguageCombinations(string editionLanguage)
        {
            IEnumerable<string> applicableLanguageTags = editionLanguage.Split('-').Combinations().Select(x => string.Join("-", x)).Where(x => !string.IsNullOrEmpty(x)).OrderBy(x => x.Length);

            if (editionLanguage.Equals("zh-cn"))
            {
                applicableLanguageTags = applicableLanguageTags.Append("zh-hans");
            }

            if (editionLanguage.Equals("zh-tw"))
            {
                applicableLanguageTags = applicableLanguageTags.Append("zh-hant");
            }

            if (editionLanguage.Equals("fil-latn"))
            {
                applicableLanguageTags = applicableLanguageTags.Append("fil-ph");
            }

            if (languageMap.ContainsKey(editionLanguage.ToLower()))
            {
                applicableLanguageTags = applicableLanguageTags.Union(languageMap[editionLanguage.ToLower()]);
            }

            return applicableLanguageTags.Reverse();
        }

        /// <summary>
        /// Gathers a list of AppX files to install for a target edition
        /// </summary>
        /// <param name="editionCdb">The edition Composition Database to generate workloads for</param>
        /// <param name="appsCdb">The application Composition Database</param>
        /// <param name="repositoryPath">The path to the repository file set</param>
        /// <returns></returns>
        public static PackageProperties[] GetAppxFilesToKeep(BaseManifest editionCdb, IEnumerable<BaseManifest> appsCdbs, string editionLanguage)
        {
            IEnumerable<string> applicableLanguageTags = GetAllPossibleLanguageCombinations(editionLanguage);

            (Dictionary<string, DeploymentProperties> preinstalledApps, Feature[] appsFeatures) = SetupVariables(editionCdb, appsCdbs);

            HashSet<string> allPackageIDs = [];
            // Pick packages and dump licenses
            foreach (Feature ftr in appsFeatures)
            {
                string appFeatureId = ftr.FeatureID;
                if (!preinstalledApps.ContainsKey(appFeatureId))
                {
                    continue;
                }

                DeploymentProperties deployProps = preinstalledApps[appFeatureId];

                bool isFramework = ftr.Type == "MSIXFramework";
                if (isFramework)
                {
                    deployProps.IsFramework = true;
                }

                string licenseData = ftr.CustomInformation?.CustomInfo
                    .FirstOrDefault(x => x.Key == "licensedata")?.Value;
                if (licenseData != null)
                {
                    deployProps.HasLicense = true;
                }

                List<Package> appPackages = ftr.Packages.Package;
                deployProps.AddApplicablePackages(appPackages, applicableLanguageTags);

                foreach (string pid in deployProps.PackageIDs)
                {
                    _ = allPackageIDs.Add(pid);
                }
            }

            Dictionary<string, PackageProperties> packageHashDict = [];
            foreach (BaseManifest appsCdb in appsCdbs)
            {
                foreach (Package p in appsCdb.Packages.Package)
                {
                    string packageId = p.ID;
                    if (!allPackageIDs.Contains(packageId))
                    {
                        continue;
                    }

                    PayloadItem pCanonical = p
                        .Payload
                        .PayloadItem
                        .First(x => x.PayloadType == "Canonical");
                    packageHashDict[packageId] = new PackageProperties()
                    {
                        Path = pCanonical.Path,
                        SHA256 = pCanonical.PayloadHash
                    };
                }
            }

            return preinstalledApps.SelectMany(x => x.Value.PackageIDs).Select(x => packageHashDict[x]).ToArray();
        }
    }
}
