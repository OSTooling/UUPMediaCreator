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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Media.Creator.Planning;
using UnifiedUpdatePlatform.Services.Composition.Database;
using UnifiedUpdatePlatform.Services.Temp;
using UnifiedUpdatePlatform.Services.WindowsUpdate;

namespace UUPDownload
{
    public static partial class BuildTargets
    {
        public static async Task<EditionPlanningWithLanguage> GetTargetedPlanAsync(this UpdateData update, string LanguageCode)
        {
            HashSet<BaseManifest> compDBs = await update.GetCompDBsAsync();
            Package editionPackPkg = compDBs.GetEditionPackFromCompDBs();

            string editionPkg = await update.DownloadFileFromDigestAsync(editionPackPkg.Payload.PayloadItem.First(x => !x.Path.EndsWith(".psf")).PayloadHash);
            return await update.GetTargetedPlanAsync(LanguageCode, editionPkg);
        }

        public static async Task<EditionPlanningWithLanguage> GetTargetedPlanAsync(this UpdateData update, string LanguageCode, string editionPkg)
        {
            HashSet<BaseManifest> compDBs = await update.GetCompDBsAsync();
            if (string.IsNullOrEmpty(editionPkg))
            {
                return null;
            }

            _ = ConversionPlanBuilder.GetTargetedPlan(compDBs, editionPkg, LanguageCode, true, out List<EditionTarget> targets, new TempManager(), null);
            return new EditionPlanningWithLanguage() { EditionTargets = targets, LanguageCode = LanguageCode };
        }

        public static void PrintAvailablePlan(this List<EditionTarget> targets)
        {
            foreach (EditionTarget target in targets)
            {
                foreach (string str in ConversionPlanBuilder.PrintEditionTarget(target))
                {
                    Logging.Log(str);
                }
            }
        }
    }
}
