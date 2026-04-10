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
using System.IO;
using System.Linq;
using UnifiedUpdatePlatform.Services.Composition.Database;
using UnifiedUpdatePlatform.Services.Temp;


#nullable enable

namespace UnifiedUpdatePlatform.Media.Creator.Planning
{
    public static class FileLocator
    {
        public static List<BaseManifest> GetCompDBsFromUUPFiles(string UUPPath, TempManager tempManager)
        {
            List<BaseManifest> compDBs = [];

            try
            {
                IEnumerable<string>? enumeratedFiles = Directory.EnumerateFiles(UUPPath, "*aggregatedmetadata*", new EnumerationOptions() { MatchCasing = MatchCasing.CaseInsensitive });
                if (enumeratedFiles.Any())
                {
                    string? cabFile = enumeratedFiles.First();

                    foreach (string? file in CabinetExtractor.EnumCabinetFiles(cabFile).Where(x => x.FileName.EndsWith(".xml.cab", StringComparison.InvariantCultureIgnoreCase)).Select(x => x.FileName))
                    {
                        try
                        {
                            string tmp = tempManager.GetTempPath();
                            File.WriteAllBytes(tmp, CabinetExtractor.ExtractCabinetFile(cabFile, file));

                            string filename = CabinetExtractor.EnumCabinetFiles(tmp).First().FileName;
                            byte[] xmlfile = CabinetExtractor.ExtractCabinetFile(tmp, filename);

                            using Stream xmlstream = new MemoryStream(xmlfile);
                            compDBs.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(UUPPath).Select(x => Path.GetFileName(x)).Where(x => x.EndsWith(".xml.cab", StringComparison.InvariantCultureIgnoreCase));

                    foreach (string? file in files)
                    {
                        try
                        {
                            string? cabFile = Path.Combine(UUPPath, file);

                            byte[] xmlfile = CabinetExtractor.ExtractCabinetFile(cabFile, CabinetExtractor.EnumCabinetFiles(cabFile).First().FileName);

                            using Stream xmlstream = new MemoryStream(xmlfile);
                            compDBs.Add(CompDBXmlClass.DeserializeCompDB(xmlstream));
                        }
                        catch { }
                    }
                }
            }
            catch { }

            return compDBs;
        }

        public static (bool, HashSet<string>) VerifyFilesAreAvailableForCompDB(BaseManifest compDB, string UUPPath)
        {
            HashSet<string> missingPackages = [];

            foreach (Package feature in compDB.Features.Feature[0].Packages.Package)
            {
                Package pkg = compDB.Packages.Package.First(x => x.ID == feature.ID);

                (bool succeeded, string missingFile) = VerifyFileIsAvailableForPackage(pkg, UUPPath);
                if (!succeeded)
                {
                    _ = missingPackages.Add(missingFile);
                }
            }

            return (missingPackages.Count == 0, missingPackages);
        }

        public static (bool, string) VerifyFileIsAvailableForPackage(Package pkg, string UUPPath)
        {
            string missingPackage = "";

            //
            // Some download utilities that start with the letter U and finish with UPDump or start with the letter U and finish with UP.rg-adguard download files without respecting Microsoft filenames
            // We attempt to locate files based on what we think they use first.
            //
            string file = pkg.GetCommonlyUsedIncorrectFileName();

            if (!File.Exists(Path.Combine(UUPPath, file)))
            {
                //
                // Wow, someone actually downloaded UUP files using a tool that respects Microsoft paths, that's exceptional
                //
                file = pkg.Payload.PayloadItem.First(x => !x.Path.EndsWith(".psf")).Path.Replace('\\', Path.DirectorySeparatorChar);
                if (!File.Exists(Path.Combine(UUPPath, file)))
                {
                    //
                    // What a disapointment, they simply didn't download everything.. Oops.
                    // TODO: generate missing files out of thin air
                    //
                    missingPackage = file;
                }
            }

            return (string.IsNullOrEmpty(missingPackage), missingPackage);
        }
    }
}