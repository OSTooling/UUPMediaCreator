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

namespace UnifiedUpdatePlatform.Services.Composition.Database
{
    public static class BaseManifestExtensions
    {
        public static string GetCommonlyUsedIncorrectFileName(this Package pkg)
        {
            return pkg.Payload.PayloadItem.First(x => !x.Path.EndsWith(".psf")).Path.Replace('\\', Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar).Last().Replace("~31bf3856ad364e35", "").Replace("~.", ".").Replace("~", "-").Replace("-.", ".");
        }

        public static BaseManifest GetNeutralCompDB(this IEnumerable<BaseManifest> BaseManifests)
        {
            foreach (BaseManifest BaseManifest in BaseManifests)
            {
                if (BaseManifest.Type != "Build")
                {
                    continue;
                }

                if (BaseManifest.Name?.Contains("Desktop_FOD") == true)
                {
                    continue;
                }

                //
                // Newer style BaseManifests have a tag attribute, make use of it.
                //
                if (BaseManifest.Tags != null)
                {
                    if (BaseManifest.Tags.Type.Equals("Neutral", StringComparison.InvariantCultureIgnoreCase) &&
                        BaseManifest.Tags.Tag?.Find(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true &&
                        BaseManifest.Features?.Feature != null &&
                        BaseManifest.Packages?.Package != null)
                    {
                        return BaseManifest;
                    }
                }
                //
                // Older style BaseManifests have no tag elements, we need to find out if it's a neutral BaseManifest using another way
                //
                else if (BaseManifest.Features?.Feature?.FirstOrDefault(x =>
                    x.Type?.Contains("BaseNeutral", StringComparison.InvariantCultureIgnoreCase) == true) != null)
                {
                    return BaseManifest;
                }
            }

            return null;
        }

        public static HashSet<BaseManifest> GetEditionCompDBsForLanguage(
            this IEnumerable<BaseManifest> BaseManifests,
            string LanguageCode)
        {
            HashSet<BaseManifest> filteredBaseManifests = [];

            foreach (BaseManifest BaseManifest in BaseManifests)
            {
                //
                // Newer style BaseManifests have a tag attribute, make use of it.
                // TODO: Do not do contains
                //
                if (BaseManifest.Tags != null)
                {
                    if (BaseManifest.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) &&
                        BaseManifest.Tags.Tag?.Count == 3 &&
                        BaseManifest.Tags.Tag.Find(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true &&
                        BaseManifest.Tags.Tag.Find(x => x.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true &&
                        BaseManifest.Tags.Tag.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)) &&
                        BaseManifest.Name?.EndsWith("~Desktop_Apps~~") != true &&
                        BaseManifest.Name?.EndsWith("~Desktop_Apps_Moment~~") != true)
                    {
                        _ = filteredBaseManifests.Add(BaseManifest);
                    }
                }
                //
                // Older style BaseManifests have no tag elements, we need to find out if it's an edition BaseManifest using another way
                //
                else if (BaseManifest.Features?.Feature?.FirstOrDefault(x =>
                        x.Type?.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase) == true &&
                        x.FeatureID?.Contains(LanguageCode, StringComparison.InvariantCultureIgnoreCase) == true) != null &&
                        BaseManifest.Name?.EndsWith("~Desktop_Apps~~") != true &&
                        BaseManifest.Name?.EndsWith("~Desktop_Apps_Moment~~") != true)
                {
                    _ = filteredBaseManifests.Add(BaseManifest);
                }
            }

            return filteredBaseManifests;
        }

        public static HashSet<BaseManifest> GetEditionCompDBs(this IEnumerable<BaseManifest> BaseManifests)
        {
            HashSet<BaseManifest> filteredBaseManifests = [];

            foreach (BaseManifest BaseManifest in BaseManifests)
            {
                //
                // Newer style BaseManifests have a tag attribute, make use of it.
                // TODO: Do not do contains
                //
                if (BaseManifest.Tags != null)
                {
                    if (BaseManifest.Tags.Type.Equals("Edition", StringComparison.InvariantCultureIgnoreCase) &&
                        BaseManifest.Tags.Tag?.Count == 3 &&
                        BaseManifest.Tags.Tag.Find(x => x.Name.Equals("UpdateType", StringComparison.InvariantCultureIgnoreCase))?.Value?.Equals("Canonical", StringComparison.InvariantCultureIgnoreCase) == true &&
                        BaseManifest.Tags.Tag.Any(x => x.Name.Equals("Edition", StringComparison.InvariantCultureIgnoreCase)) &&
                        BaseManifest.Name?.EndsWith("~Desktop_Apps~~") != true &&
                        BaseManifest.Name?.EndsWith("~Desktop_Apps_Moment~~") != true)
                    {
                        _ = filteredBaseManifests.Add(BaseManifest);
                    }
                }
                //
                // Older style BaseManifests have no tag elements, we need to find out if it's an edition BaseManifest using another way
                //
                else if (BaseManifest.Features?.Feature?.FirstOrDefault(x =>
                        x.Type?.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase) == true) != null &&
                        BaseManifest.Name?.EndsWith("~Desktop_Apps~~") != true &&
                        BaseManifest.Name?.EndsWith("~Desktop_Apps_Moment~~") != true)
                {
                    _ = filteredBaseManifests.Add(BaseManifest);
                }
            }

            return filteredBaseManifests;
        }

        public static IEnumerable<string> GetAvailableLanguages(this IEnumerable<BaseManifest> BaseManifests)
        {
            return BaseManifests.GetEditionCompDBs().Select(x =>
            {
                if (x.Tags != null)
                {
                    return x.Tags.Tag == null || x.Tags.Tag.Count == 0
                        ? null
                        : x.Tags.Tag.Find(y => y.Name.Equals("Language", StringComparison.InvariantCultureIgnoreCase)).Value;
                }
                else if (x.Features.Feature != null && Array.Find(x.Features.Feature, y =>
                       y.Type?.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase) == true) != null)
                {
                    return Array.Find(x.Features.Feature, y =>
                       y.Type?.Contains("DesktopMedia", StringComparison.InvariantCultureIgnoreCase) == true).FeatureID.Split('_')[1];
                }
                return null;
            }).Where(x => !string.IsNullOrEmpty(x)).Distinct();
        }

        public static Package GetEditionPackFromCompDBs(this IEnumerable<BaseManifest> BaseManifests)
        {
            HashSet<Package> pkgs = [];

            //
            // Get base editions that are available with all their files
            //
            HashSet<BaseManifest> filteredBaseManifests = BaseManifests.GetEditionCompDBs();

            if (filteredBaseManifests.Count > 0)
            {
                foreach (BaseManifest BaseManifest in filteredBaseManifests)
                {
                    foreach (Package feature in filteredBaseManifests.First().Features.Feature[0].Packages.Package)
                    {
                        Package pkg = filteredBaseManifests.First().Packages.Package.First(x => x.ID == feature.ID);

                        IEnumerable<string> files = pkg.Payload.PayloadItem.Select(x => x.Path.Replace('\\', Path.DirectorySeparatorChar));

                        if (files.Any(file => !file.EndsWith(".esd", StringComparison.InvariantCultureIgnoreCase) ||
                            !file.Contains("microsoft-windows-editionspecific", StringComparison.InvariantCultureIgnoreCase) ||
                            file.Contains("WOW64", StringComparison.InvariantCultureIgnoreCase) ||
                            file.Contains("arm64.arm", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            // We do not care about this file
                            continue;
                        }

                        _ = pkgs.Add(pkg);
                    }
                }
            }

            Package minpkg = null;
            if (pkgs.Count > 0)
            {
                foreach (Package pkg in pkgs)
                {
                    if (minpkg == null)
                    {
                        minpkg = pkg;
                    }
                    else
                    {
                        foreach (PayloadItem minitem in minpkg.Payload.PayloadItem)
                        {
                            foreach (PayloadItem item in minpkg.Payload.PayloadItem)
                            {
                                if (ulong.Parse(minitem.PayloadSize) > ulong.Parse(item.PayloadSize))
                                {
                                    minpkg = pkg;
                                }
                            }
                        }
                    }
                }
            }

            return minpkg;
        }
    }
}
