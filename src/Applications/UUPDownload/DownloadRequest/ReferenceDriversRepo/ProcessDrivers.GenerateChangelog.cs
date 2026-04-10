using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Services.Composition.Database;
using UnifiedUpdatePlatform.Services.WindowsUpdate;

namespace UUPDownload.DownloadRequest.ReferenceDriversRepo
{
    public partial class ProcessDrivers
    {
        private static async Task<BaseManifest> GenerateChangelog(UpdateData update, string changelogOutput, int i, BaseManifest pevCompDB)
        {
            File.AppendAllLines(changelogOutput, ["", $"## {update.Xml.LocalizedProperties.Title} - 200.0.{i}.0"]);

            HashSet<BaseManifest> compDBs = await update.GetCompDBsAsync();
            BaseManifest curCompDB = compDBs.First();

            List<string> added = [];
            List<string> updated = [];
            List<string> removed = [];
            List<string> modified = [];

            if (pevCompDB != null)
            {
                IEnumerable<string> prevPackageList = pevCompDB.Packages.Package.Select(pkg => $"| {pkg.Version} | {pkg.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.InvariantCultureIgnoreCase)} |");
                IEnumerable<string> curPackageList = curCompDB.Packages.Package.Select(pkg => $"| {pkg.Version} | {pkg.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.InvariantCultureIgnoreCase)} |");

                foreach (string pkg in prevPackageList)
                {
                    string version = pkg.Split("|")[1];
                    string id = pkg.Split("|")[2];

                    bool existsInNewer = curPackageList.Any(x => id.Equals(x.Split("|")[2], StringComparison.InvariantCultureIgnoreCase));

                    if (!existsInNewer)
                    {
                        removed.Add(pkg);
                    }
                }

                foreach (Package package in curCompDB.Packages.Package)
                {
                    string pkg = $"| {package.Version} | {package.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.InvariantCultureIgnoreCase)} |";

                    string version = pkg.Split("|")[1];
                    string id = pkg.Split("|")[2];

                    bool existsInOlder = prevPackageList.Any(x => id.Equals(x.Split("|")[2], StringComparison.InvariantCultureIgnoreCase));

                    if (existsInOlder)
                    {
                        bool hasSameVersion = prevPackageList.Any(x => version.Equals(x.Split("|")[1], StringComparison.InvariantCultureIgnoreCase) && id.Equals(x.Split("|")[2], StringComparison.InvariantCultureIgnoreCase));

                        if (!hasSameVersion)
                        {
                            updated.Add(pkg);
                        }
                        else
                        {
                            bool hasSameHash = pevCompDB.Packages.Package.Any(x => x.Payload.PayloadItem[0].PayloadHash == package.Payload.PayloadItem[0].PayloadHash);
                            if (!hasSameHash)
                            {
                                modified.Add(pkg);
                            }
                        }
                    }
                    else
                    {
                        added.Add(pkg);
                    }
                }
            }
            else
            {
                foreach (Package pkg in curCompDB.Packages.Package)
                {
                    added.Add($"| {pkg.Version} | {pkg.ID.Split("-")[1].Replace(".inf", ".cab", StringComparison.CurrentCultureIgnoreCase)} |");
                }
            }

            added.Sort();
            updated.Sort();
            modified.Sort();
            removed.Sort();

            if (added.Count > 0)
            {
                File.AppendAllLines(changelogOutput, ["", $"### Added", ""]);
                File.AppendAllLines(changelogOutput, ["| Driver version | Package |"]);
                File.AppendAllLines(changelogOutput, ["|----------------|---------|"]);
                File.AppendAllLines(changelogOutput, added);
            }


            if (updated.Count > 0)
            {
                File.AppendAllLines(changelogOutput, ["", $"### Updated", ""]);
                File.AppendAllLines(changelogOutput, ["| Driver version | Package |"]);
                File.AppendAllLines(changelogOutput, ["|----------------|---------|"]);
                File.AppendAllLines(changelogOutput, updated);
            }


            if (modified.Count > 0)
            {
                File.AppendAllLines(changelogOutput, ["", $"### Modified", ""]);
                File.AppendAllLines(changelogOutput, ["| Driver version | Package |"]);
                File.AppendAllLines(changelogOutput, ["|----------------|---------|"]);
                File.AppendAllLines(changelogOutput, modified);
            }


            if (removed.Count > 0)
            {
                File.AppendAllLines(changelogOutput, ["", $"### Removed", ""]);
                File.AppendAllLines(changelogOutput, ["| Driver version | Package |"]);
                File.AppendAllLines(changelogOutput, ["|----------------|---------|"]);
                File.AppendAllLines(changelogOutput, removed);
            }

            return curCompDB;
        }
    }
}
