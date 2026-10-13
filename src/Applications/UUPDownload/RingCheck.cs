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
using System.Linq;
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Services.WindowsUpdate;

namespace UUPDownload
{
    public static class RingCheck
    {
        private static Dictionary<CTAC, string> GetRingCTACs(MachineType machineType, OSSkuId osSkuId)
        {
            return new Dictionary<CTAC, string>()
            {
                // --- Windows 10: retail UUP sets that Microsoft still serves ---
                // NOTE: Windows 10 1703 (rs2_release / 15063) and older (1607, 1511, 1507)
                // are no longer offered by the Windows Update servers. The old
                // "Insider Slow/Fast (RS2)" entries were misleading: they reported a
                // 15063 device but the servers answered with a much newer build.
                { new CTAC(osSkuId, "10.0.16299.15", machineType, "Retail", "", "CB", "rs3_release", "Production", true), "Retail (1709 - RS3)" },
                { new CTAC(osSkuId, "10.0.17134.1", machineType, "Retail", "", "CB", "rs4_release", "Production", true), "Retail (1803 - RS4)" },
                { new CTAC(osSkuId, "10.0.17763.1217", machineType, "Retail", "", "CB", "rs5_release", "Production", true), "Retail (1809 - RS5)" },
                { new CTAC(osSkuId, "10.0.18362.836", machineType, "Retail", "", "CB", "19h1_release", "Production", true), "Retail (1903 - 19H1)" },
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "Retail", "", "CB", "vb_release", "Production", false, false), "Retail (Windows 10 - VB)" },
                { new CTAC(osSkuId, "10.0.19041.84", machineType, "Retail", "", "CB", "vb_release", "Production", false), "Retail" },

                // --- Windows 10 Extended Security Updates (ESU) ---
                // Windows 10 22H2 is past end of support, so its monthly updates are
                // only offered to ESU-enrolled devices. This CTAC reports ESU
                // enrollment, making the servers hand out the ESU build
                // (for example 10.0.19045.7727).
                { new CTAC(osSkuId, "10.0.19045.1", machineType, "Retail", "", "CB", "vb_release", "Production", false, IsEsu: true), "Retail (Windows 10 ESU)" },

                // --- Windows 11: retail branches (NI = 21H2/22H2/23H2, GE = 24H2/25H2) ---
                { new CTAC(osSkuId, "10.0.22621.1", machineType, "Retail", "", "CB", "ni_release", "Production", false), "Retail (Windows 11 - NI)" },
                { new CTAC(osSkuId, "10.0.26100.1", machineType, "Retail", "", "CB", "ge_release", "Production", false), "Retail (Windows 11 - GE)" },

                // --- Insider channels: current branches ---
                // Beta / Release Preview now live on the GE (Germanium) branch; using
                // ni_release here (as older docs did) returns nothing for Windows 11.
                { new CTAC(osSkuId, "10.0.26100.1", machineType, "External", "ReleasePreview", "CB", "ge_release", "Production", false, false), "Release Preview (Windows 11)" },
                { new CTAC(osSkuId, "10.0.26100.1", machineType, "External", "Beta", "CB", "ge_release", "Production", false, false), "Beta (Windows 11)" },
                { new CTAC(osSkuId, "10.0.26100.1", machineType, "External", "Dev", "CB", "rs_prerelease", "Production", false, false), "Dev" },
                { new CTAC(osSkuId, "10.0.26100.1", machineType, "External", "CanaryChannel", "CB", "rs_prerelease", "Production", false, false), "Canary" },

                // --- Windows 10 Beta: still served, answers with the latest 22H2 build ---
                { new CTAC(osSkuId, "10.0.19045.1", machineType, "External", "Beta", "CB", "vb_release", "Production", false, false), "Beta (Windows 10)" },

                // --- Legacy Windows 10 insider rings (kept for completeness) ---
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "RP", "External", "CB", "vb_release", "Production", false, false), "Insider Release Preview" },
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "WIS", "External", "CB", "vb_release", "Production", false, false), "Insider Slow" },
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "WIF", "External", "CB", "vb_release", "Production", false, false), "Insider Fast" },
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "WIF", "External", "CB", "vb_release", "Production", false, false, "Skip"), "Skip Ahead" },
            };
        }

        internal static void ParseGetBuildsOptions(GetBuildsOptions opts)
        {
            GetRingBuilds(opts).Wait();
        }

        private static async Task GetRingBuilds(GetBuildsOptions opts)
        {
            Dictionary<CTAC, string> CTACs = new();

            CTACs = string.IsNullOrWhiteSpace(opts.TargetingAttribute)
                ? GetRingCTACs(opts.MachineType, opts.ReportingSku)
                : GetRingCTACs(opts.MachineType, opts.ReportingSku)
                    .Where(c => string.Equals(c.Value, opts.TargetingAttribute, StringComparison.Ordinal))
                    .ToDictionary(c => c.Key, c => c.Value);

            foreach (KeyValuePair<CTAC, string> CTAC in CTACs)
            {
                CTAC ctac = CTAC.Key;

                string token = string.Empty;
                if (!string.IsNullOrEmpty(opts.Mail) && !string.IsNullOrEmpty(opts.Password))
                {
                    token = await MBIHelper.GenerateMicrosoftAccountTokenAsync(opts.Mail, opts.Password);
                }

                IEnumerable<UpdateData> data = await FE3Handler.GetUpdates(null, ctac, token, FileExchangeV3UpdateFilter.ProductRelease);

                if (!data.Any())
                {
                    // The Windows Update servers occasionally answer with an empty set
                    // when several rings are queried in quick succession (rate limiting).
                    // Give it a moment and retry once before reporting "nothing".
                    await Task.Delay(1500);
                    data = await FE3Handler.GetUpdates(null, ctac, token, FileExchangeV3UpdateFilter.ProductRelease);
                }

                //data = data.Select(x => UpdateUtils.TrimDeltasFromUpdateData(x));

                for (int i = 0; i < data.Count(); i++)
                {
                    string buildStr = await data.ToList()[i].GetBuildStringAsync();
                    if (string.IsNullOrEmpty(buildStr))
                    {
                        buildStr = data.ToList()[i].Xml.LocalizedProperties.Title;
                    }

                    Console.WriteLine($"\"{CTAC.Value}\"[{i}]=\"{buildStr}\"");
                }

                // Small courtesy delay: querying every ring back-to-back can make the
                // Windows Update servers silently drop some responses (rate limiting),
                // which made some rings appear as if they had no builds.
                await Task.Delay(750);
            }
        }
    }
}
