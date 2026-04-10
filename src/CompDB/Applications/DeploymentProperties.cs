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
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnifiedUpdatePlatform.Services.Composition.Database.Applications
{
    public class DeploymentProperties
    {
        public HashSet<string> Dependencies
        {
            get; set;
        }
        public bool PreferStub
        {
            get; set;
        }
        public bool IsFramework
        {
            get; set;
        }
        public HashSet<string> PackageIDs
        {
            get; set;
        }
        public string MainPackageID
        {
            get; set;
        }
        public bool HasLicense
        {
            get; set;
        }

        public void AddApplicablePackages(IEnumerable<Package> packageElements, IEnumerable<string> applicableLanguageTags)
        {
            PackageIDs = [];
            Dictionary<int, string> scaleDictionary = null;
            foreach (Package package in packageElements)
            {
                string packageId = package.ID;
                switch (package.PackageType)
                {
                    case "MSIXBundlePackage":
                        _ = PackageIDs.Add(packageId);
                        MainPackageID = packageId;
                        break;
                    case "MSIXFrameworkPackage":
                        _ = PackageIDs.Add(packageId);
                        break;
                    case "StubMSIXResourcePackage":
                        if (PreferStub)
                        {
                            HandleResourcePackage(packageId, applicableLanguageTags, ref scaleDictionary);
                        }

                        break;
                    case "StubMSIXMainPackage":
                        if (PreferStub)
                        {
                            _ = PackageIDs.Add(packageId);
                        }

                        break;
                    case "MSIXResourcePackage":
                        if (!PreferStub)
                        {
                            HandleResourcePackage(packageId, applicableLanguageTags, ref scaleDictionary);
                        }

                        break;
                    case "MSIXMainPackage":
                        if (!PreferStub)
                        {
                            _ = PackageIDs.Add(packageId);
                        }

                        break;
                    default:
                        throw new Exception();
                }
            }
            MainPackageID ??= PackageIDs.First();

            if (scaleDictionary != null)
            {
                foreach (int ds in new[] { 100, 140, 180 })
                {
                    int bestCandidate = 0;
                    double bestCandidateScore = 0.0;
                    foreach (KeyValuePair<int, string> availableScale in scaleDictionary)
                    {
                        double score = AppxApplicabilityEngine.GetScaleFactorScore(ds, availableScale.Key);
                        if (score > bestCandidateScore)
                        {
                            bestCandidate = availableScale.Key;
                            bestCandidateScore = score;
                            if (score == 1.0)
                            {
                                break;
                            }
                        }
                    }
                    string pickedScale = scaleDictionary[bestCandidate];
                    if (pickedScale != null)
                    {
                        _ = PackageIDs.Add(pickedScale);
                    }
                }
            }
        }

        private void HandleResourcePackage(string packageId, IEnumerable<string> applicableLanguageTags, ref Dictionary<int, string> scaleDictionary)
        {
            scaleDictionary ??= new Dictionary<int, string>()
                {
                    { 100, null },
                    { 200, null }
                };

            (ResourceType, string) resourceInfo = GetResourcePackageInfo(packageId);
            switch (resourceInfo.Item1)
            {
                case ResourceType.Language:
                    if (applicableLanguageTags.Any(x => resourceInfo.Item2 == x))
                    {
                        _ = PackageIDs.Add(packageId);
                    }

                    break;
                case ResourceType.Scale:
                    scaleDictionary[int.Parse(resourceInfo.Item2)] = packageId;
                    break;
            }
        }

        private static (ResourceType, string) GetResourcePackageInfo(string packageId)
        {
            int lastUnderscore = packageId.LastIndexOf('_');
            int prevUnderscore = packageId.LastIndexOf('_', lastUnderscore - 1) + 1;
            string resourceType = packageId[prevUnderscore..lastUnderscore];
            int typeDot = resourceType.IndexOf('.');
            if (typeDot != -1)
            {
                typeDot++;
                int typeDash = resourceType.IndexOf('-');
                string resourceVariant = resourceType[(typeDash + 1)..];
                resourceType = resourceType[typeDot..typeDash];
                return resourceType switch
                {
                    "language" => (ResourceType.Language, resourceVariant),
                    "scale" => (ResourceType.Scale, resourceVariant),
                    _ => throw new Exception(),
                };
            }
            else
            {
                return (ResourceType.Language, resourceType);
            }
        }
    }
}
