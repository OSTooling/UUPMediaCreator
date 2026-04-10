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
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Services.WindowsUpdate.ESRP;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.JSON.ESRP;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.GetExtendedUpdateInfo2.Response;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate
{
    public class FileExchangeV3FileDownloadInformation
    {
        public string DownloadUrl
        {
            get;
        }

        public bool IsEncrypted => EsrpDecryptionInformation != null;

        public DateTime ExpirationDate
        {
            get
            {
                DateTime dateTime = DateTime.MaxValue;
                try
                {
                    long value = long.Parse(DownloadUrl.Split("P1=")[1].Split("&")[0]);
                    dateTime = DateTimeOffset.FromUnixTimeSeconds(value).ToLocalTime().DateTime;
                }
                catch { }
                return dateTime;
            }
        }

        public bool IsDownloadable => ExpirationDate > DateTime.Now;

        public TimeSpan TimeLeft => IsDownloadable ? DateTime.Now - ExpirationDate : new TimeSpan(0);

        public EsrpDecryptionInformation EsrpDecryptionInformation { get; set; } = null;

        public string Digest
        {
            get;
        }

        internal FileExchangeV3FileDownloadInformation(FileLocation fileLocation)
        {
            DownloadUrl = fileLocation.Url;
            if (!string.IsNullOrEmpty(fileLocation.EsrpDecryptionInformation))
            {
                EsrpDecryptionInformation = EsrpDecryptionInformation.DeserializeFromJson(fileLocation.EsrpDecryptionInformation);
            }
            Digest = fileLocation.FileDigest;
        }

        public override bool Equals(object obj)
        {
            return obj is FileExchangeV3FileDownloadInformation info && info.Digest == Digest;
        }

        public override int GetHashCode()
        {
            return Digest.GetHashCode();
        }

        public async Task<bool> DecryptAsync(string InputFile, string OutputFile)
        {
            if (!IsEncrypted)
            {
                return false;
            }

            try
            {
                using ESRPCryptography esrp = new(EsrpDecryptionInformation);
                await esrp.DecryptFileAsync(InputFile, OutputFile);
                return true;
            }
            catch { }

            return false;
        }
    }
}