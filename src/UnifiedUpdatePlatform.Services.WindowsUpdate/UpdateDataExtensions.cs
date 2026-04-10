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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Services.Composition.Database;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate
{
    public static class UpdateDataExtensions
    {
        private static readonly WebClient client = new();

        public static async Task<string> DownloadFileFromDigestAsync(this UpdateData update, string Digest)
        {
            string metadataCabTemp = Path.GetTempFileName();
            await DownloadFileFromDigestAsync(update, Digest, metadataCabTemp);
            return !File.Exists(metadataCabTemp) || new FileInfo(metadataCabTemp).Length == 0 ? null : metadataCabTemp;
        }

        public static async Task DownloadFileFromDigestAsync(this UpdateData update, string Digest, string Destination)
        {
            FileExchangeV3FileDownloadInformation fileDownloadInfo = await update.GetFileUrl(Digest);
            if (fileDownloadInfo == null)
            {
                // TODO: notify of result
                return;
            }

            // Download the file
            await client.DownloadFileTaskAsync(new Uri(fileDownloadInfo.DownloadUrl), Destination);

            if (fileDownloadInfo.IsEncrypted)
            {
                if (!await fileDownloadInfo.DecryptAsync(Destination, Destination + ".decrypted"))
                {
                    return;
                }

                File.Delete(Destination);
                File.Move(Destination + ".decrypted", Destination);
            }
        }

        public static async Task<FileExchangeV3FileDownloadInformation> GetFileUrl(this UpdateData update, string Digest)
        {
            return await FE3Handler.GetFileUrl(update, Digest);
        }

        public static async Task<string> GetBuildStringAsync(this UpdateData update)
        {
            string result = null;

            try
            {
                Models.FE3.XML.ExtendedUpdateInfo.File deploymentCab = null;

                foreach (Models.FE3.XML.ExtendedUpdateInfo.File file in update.Xml.Files.File)
                {
                    if (file.FileName.Replace('\\', Path.DirectorySeparatorChar).EndsWith("deployment.cab", StringComparison.InvariantCultureIgnoreCase))
                    {
                        deploymentCab = file;
                        break;
                    }
                }

                if (deploymentCab == null)
                {
                    goto exit;
                }

                FileExchangeV3FileDownloadInformation fileDownloadInfo = await FE3Handler.GetFileUrl(update, deploymentCab.Digest);
                if (fileDownloadInfo == null)
                {
                    goto exit;
                }

                string deploymentCabTemp = Path.GetTempFileName();
                await client.DownloadFileTaskAsync(new Uri(fileDownloadInfo.DownloadUrl), deploymentCabTemp);

                if (fileDownloadInfo.IsEncrypted)
                {
                    if (!await fileDownloadInfo.DecryptAsync(deploymentCabTemp, deploymentCabTemp + ".decrypted"))
                    {
                        goto exit;
                    }

                    File.Delete(deploymentCabTemp);
                    File.Move(deploymentCabTemp + ".decrypted", deploymentCabTemp);
                }

                try
                {
                    byte[] buffer = CabinetExtractor.ExtractCabinetFile(deploymentCabTemp, "UpdateAgent.dll");
                    result = GetBuildStringFromUpdateAgent(buffer);
                }
                catch { }

                string reportedBuildNumberFromService = update.Xml.ExtendedProperties.ReleaseVersion.Split('.')[2];
                if (!string.IsNullOrEmpty(result) && result.Count(x => x == '.') >= 2)
                {
                    string[] elements = result.Split('.');
                    elements[2] = reportedBuildNumberFromService;
                    result = string.Join(".", elements);
                }

                File.Delete(deploymentCabTemp);
            }
            catch
            {
            }

        exit:

            // For some reason we couldn't get the build string, so attempt to get it from CompDB metadata instead
            // This is less reliable, but it is the best we can actually do.
            if (string.IsNullOrEmpty(result))
            {
                try
                {
                    HashSet<BaseManifest> compDBs = await update.GetCompDBsAsync();
                    BaseManifest firstCompDB = compDBs.First();

                    // example:
                    // BuildInfo="co_release.21382.1.210511-1416" OSVersion="10.0.21382.1" TargetBuildInfo="co_release.21382.1.210511-1416" TargetOSVersion="10.0.21382.1"

                    string buildInfo = firstCompDB.TargetBuildInfo ?? firstCompDB.BuildInfo;
                    string osVersion = firstCompDB.TargetOSVersion ?? firstCompDB.OSVersion;

                    if (buildInfo == null)
                    {
                        return result;
                    }

                    string[] splitBI = buildInfo.Split(".");

                    result = $"{osVersion} ({splitBI[0]}.{splitBI[3]})";
                }
                catch { }
            }

            return result;
        }

        private static string GetBuildStringFromUpdateAgent(byte[] updateAgentFile)
        {
            byte[] sign = [
                0x46, 0x00, 0x69, 0x00, 0x6c, 0x00, 0x65, 0x00, 0x56, 0x00, 0x65, 0x00, 0x72,
                0x00, 0x73, 0x00, 0x69, 0x00, 0x6f, 0x00, 0x6e, 0x00, 0x00, 0x00, 0x00, 0x00
            ];

            int fIndex = IndexOf(updateAgentFile, sign) + sign.Length;
            int lIndex = IndexOf(updateAgentFile, [0x00, 0x00, 0x00], fIndex) + 1;

            byte[] sliced = SliceByteArray(updateAgentFile, lIndex - fIndex, fIndex);

            return Encoding.Unicode.GetString(sliced);
        }

        private static byte[] SliceByteArray(byte[] source, int length, int offset)
        {
            byte[] destfoo = new byte[length];
            Array.Copy(source, offset, destfoo, 0, length);
            return destfoo;
        }

        private static int IndexOf(byte[] searchIn, byte[] searchFor, int offset = 0)
        {
            if (searchIn is not null and not null)
            {
                if (searchFor.Length > searchIn.Length)
                {
                    return 0;
                }

                for (int i = offset; i < searchIn.Length; i++)
                {
                    int startIndex = i;
                    bool match = true;
                    for (int j = 0; j < searchFor.Length; j++)
                    {
                        if (searchIn[startIndex] != searchFor[j])
                        {
                            match = false;
                            break;
                        }
                        else if (startIndex < searchIn.Length)
                        {
                            startIndex++;
                        }
                    }
                    if (match)
                    {
                        return startIndex - searchFor.Length;
                    }
                }
            }
            return -1;
        }

        public static async Task<IEnumerable<string>> GetAvailableLanguagesAsync(this UpdateData update)
        {
            return (await update.GetCompDBsAsync()).GetAvailableLanguages();
        }

        private static async Task<HashSet<BaseManifest>> GetCompDBs(UpdateData update)
        {
            HashSet<BaseManifest> neutralCompDB = [];
            HashSet<Models.FE3.XML.ExtendedUpdateInfo.File> metadataCabs = [];

            foreach (Models.FE3.XML.ExtendedUpdateInfo.File file in update.Xml.Files.File)
            {
                if (file.PatchingType.Equals("metadata", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ = metadataCabs.Add(file);
                }
            }

            if (metadataCabs.Count == 0)
            {
                return neutralCompDB;
            }

            foreach (Models.FE3.XML.ExtendedUpdateInfo.File metadataCab in metadataCabs)
            {
                FileExchangeV3FileDownloadInformation fileDownloadInfo = await FE3Handler.GetFileUrl(update, metadataCab.Digest);
                if (fileDownloadInfo == null)
                {
                    return neutralCompDB;
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
                            return neutralCompDB;
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
                            foreach (CabinetFile cabinetXmlFile in CabinetExtractor.EnumCabinetFiles(cabinetFile))
                            {
                                try
                                {
                                    // We need to expand the xml here
                                    byte[] xmlFileBuffer = CabinetExtractor.ExtractCabinetFile(cabinetFile, cabinetXmlFile.FileName);
                                    try
                                    {
                                        using Stream xmlStream = new MemoryStream(xmlFileBuffer);
                                        _ = neutralCompDB.Add(CompDBXmlClass.DeserializeCompDB(xmlStream));
                                    }
                                    catch
                                    {
                                        using Stream xmlStream = new MemoryStream(xmlFileBuffer);
                                        _ = neutralCompDB.Add(CompDBXmlClass.DeserializeDeviceManifest(xmlStream));
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                    }

                    // We just parse xmls here
                    foreach (string xmlFile in Directory.EnumerateFiles(tmp, "*.xml", SearchOption.AllDirectories))
                    {
                        try
                        {
                            using Stream xmlStream = File.OpenRead(xmlFile);
                            _ = neutralCompDB.Add(CompDBXmlClass.DeserializeCompDB(xmlStream));
                        }
                        catch
                        {
                            using Stream xmlStream = File.OpenRead(xmlFile);
                            _ = neutralCompDB.Add(CompDBXmlClass.DeserializeDeviceManifest(xmlStream));
                        }
                    }
                }
                catch { }
            }

            return neutralCompDB;
        }

        public static async Task<HashSet<BaseManifest>> GetCompDBsAsync(this UpdateData update)
        {
            return update.CompDBs ??= await GetCompDBs(update);
        }
    }
}
