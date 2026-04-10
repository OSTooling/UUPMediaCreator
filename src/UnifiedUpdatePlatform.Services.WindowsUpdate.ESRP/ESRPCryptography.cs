/*
 * Copyright (c) ADeltaX and Contributors
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
using System.Security.Cryptography;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.JSON.ESRP;

//
// Source: https://raw.githubusercontent.com/ADeltaX/ProtoBuildBot/5cce37197c44792f3401b63d876795b5bc2072a4/src/BuildChecker/Classes/Helpers/EsrpDecryptor.cs
// Released under the MIT License (as of 2020-11-04)
// And Updated with original author input on 2021-04-15
//

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.ESRP
{
    public class ESRPCryptography : IDisposable
    {
        private readonly EsrpDecryptionInformation ESRP;
        private readonly Aes aes;
        private readonly byte[] key;

        public ESRPCryptography(EsrpDecryptionInformation esrp)
        {
            ESRP = esrp;

            key = new byte[32];
            Array.Copy(Convert.FromBase64String(esrp.KeyData), 0, key, 0, 32);

            aes = Aes.Create(esrp.AlgorithmName);
            aes.Mode = CipherMode.CBC;
            aes.Key = key;
            aes.Padding = PaddingMode.None;
        }

        public async Task DecryptBufferToStreamAsync(byte[] buffer, Stream to, int bufferLength, long previousSumBlockLength,
            bool isPadded, CancellationToken cancellationToken = default)
        {
            byte[] offsetBytes = new byte[16];
            Array.Copy(BitConverter.GetBytes(previousSumBlockLength), offsetBytes, 8);

            using ICryptoTransform ivCrypter = aes.CreateEncryptor(key, new byte[16]);
            byte[] newIv = ivCrypter.TransformFinalBlock(offsetBytes, 0, 16);

            if (isPadded)
            {
                aes.Padding = PaddingMode.PKCS7;
            }

            using ICryptoTransform dec = aes.CreateDecryptor(key, newIv);
            using MemoryStream ms = new(buffer, 0, bufferLength);
            using CryptoStream cs = new(ms, dec, CryptoStreamMode.Read);
            await cs.CopyToAsync(to, cancellationToken);
        }

        public async Task DecryptStreamFullAsync(Stream encryptedFile, Stream decryptedFile, ulong encryptedSize,
            CancellationToken cancellationToken = default)
        {
            int readBytes;
            byte[] buffer = new byte[ESRP.EncryptionBufferSize];
            while ((readBytes = await encryptedFile.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
            {
                bool needsPaddingMode = encryptedSize == (ulong)encryptedFile.Position;
                long previousSumBlockLength = encryptedFile.Position - readBytes;
                await DecryptBufferToStreamAsync(buffer, decryptedFile, readBytes, previousSumBlockLength, needsPaddingMode, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public async Task DecryptFileAsync(string encryptedFilePath, string decryptedFilePath,
            CancellationToken cancellationToken = default)
        {
            using FileStream encryptedFile = File.OpenRead(encryptedFilePath);
            using FileStream decryptedFile = File.OpenWrite(decryptedFilePath);
            await DecryptStreamFullAsync(encryptedFile, decryptedFile, (ulong)encryptedFile.Length, cancellationToken);
        }

        public void Dispose()
        {
            aes.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}