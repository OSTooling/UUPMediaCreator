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
using UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting;
using UUPDownload.Options;

namespace UUPDownload
{
    public static class RingCheck
    {
        private static Dictionary<CTAC, string> GetRingCTACs(MachineType machineType, OSSkuId osSkuId)
        {
            return new Dictionary<CTAC, string>()
            {
                { new CTAC(osSkuId, "10.0.15063.534", machineType, "WIS", "", "CB", "rs2_release", "Production", false), "Insider Slow (RS2)" },
                { new CTAC(osSkuId, "10.0.15063.534", machineType, "WIF", "", "CB", "rs2_release", "Production", false), "Insider Fast (RS2)" },
                { new CTAC(osSkuId, "10.0.16299.15", machineType, "Retail", "", "CB", "rs3_release", "Production", true), "Retail (RS3)" },
                { new CTAC(osSkuId, "10.0.17134.1", machineType, "Retail", "", "CB", "rs4_release", "Production", true), "Retail (RS4)" },
                { new CTAC(osSkuId, "10.0.17763.1217", machineType, "Retail", "", "CB", "rs5_release", "Production", true), "Retail (RS5)" },
                { new CTAC(osSkuId, "10.0.18362.836", machineType, "Retail", "", "CB", "19h1_release", "Production", true), "Retail (TI)" },
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "Retail", "", "CB", "vb_release", "Production", true, false), "Retail (VB)"},
                { new CTAC(osSkuId, "10.0.19041.84", machineType, "Retail", "", "CB", "vb_release", "Production", false), "Retail" },
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "External", "ReleasePreview", "CB", "vb_release", "Production", false, false), "Release Preview"},
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "External", "Beta", "CB", "vb_release", "Production", false, false), "Beta"},
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "External", "Dev", "CB", "vb_release", "Production", false, false), "Dev"},
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "RP", "External", "CB", "vb_release", "Production", false, false, "Active"), "Insider Release Preview"},
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "WIS", "External", "CB", "vb_release", "Production", false, false, "Active"), "Insider Slow"},
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "WIF", "External", "CB", "vb_release", "Production", false, false, "Active"), "Insider Fast"},
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "WIF", "External", "CB", "vb_release", "Production", false, false, "Skip"), "Skip Ahead"},
                { new CTAC(osSkuId, "10.0.19041.200", machineType, "External", "CanaryChannel", "CB", "vb_release", "Production", false, false), "Canary"},
            };
        }

        internal static void ParseGetBuildsOptions(GetBuildsOptions opts)
        {
            GetRingBuilds(opts).Wait();
        }

        private static async Task GetRingBuilds(GetBuildsOptions opts)
        {
            Dictionary<CTAC, string> CTACs = [];

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
            }
        }
    }
}
