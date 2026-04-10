/*
 * Copyright (c) Gustave Monce and Contributors
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using UnifiedUpdatePlatform.Services.WindowsUpdate.ESRP;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.JSON.ESRP;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Downloads
{
    public class HttpDownloader : IDisposable
    {
        private const long CHUNK_SIZE = 8_388_608 + 65_536; //Slice 8MB+64KB
        private const string TEMP_DOWNLOAD_EXTENSION = ".dlTmp";

        private readonly HttpClient _hc;
        public string DownloadFolderPath
        {
            get; set;
        }
        public int DownloadThreads
        {
            get; set;
        }
        public int DownloadRetries
        {
            get; set;
        }
        public bool VerifyFiles
        {
            get; set;
        }

        public HttpDownloader(string downloadFolderPath, int downloadThreads = 4, bool verifyFiles = true, IWebProxy proxy = null, bool useSystemProxy = true)
        {
            HttpClientHandler filter = new()
            {
                AutomaticDecompression = DecompressionMethods.All,
                MaxConnectionsPerServer = 512,
            };

            if (proxy != null || !useSystemProxy)
            {
                filter.Proxy = proxy;
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                filter.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator; //For Linux, MS cert isn't trusted lol ¯\_(ツ)_/¯
            }

            _hc = new HttpClient(filter)
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
            _hc.DefaultRequestHeaders.Add("User-Agent", "Windows-Update-Agent/10.0.10011.16384 Client-Protocol/2.1");
            _hc.DefaultRequestHeaders.Connection.Add("keep-alive");

            DownloadThreads = downloadThreads;
            DownloadFolderPath = downloadFolderPath;
            VerifyFiles = verifyFiles;
        }

        public async Task<bool> DownloadAsync(List<UUPFile> Files, IProgress<GeneralDownloadProgress> generalDownloadProgress, CancellationToken cancellationToken = default)
        {
            return await ParallelQueue(Files, DownloadFile, generalDownloadProgress, DownloadThreads, cancellationToken);
        }

        public async Task<bool> DownloadAsync(UUPFile File, IProgress<FileDownloadStatus> downloadProgress = null, CancellationToken cancellationToken = default)
        {
            return await DownloadFile(File, downloadProgress, cancellationToken);
        }

        private static async ValueTask<bool> ParallelQueue(List<UUPFile> items, Func<UUPFile, IProgress<FileDownloadStatus>, CancellationToken, ValueTask<bool>> func,
            IProgress<GeneralDownloadProgress> generalProgress, int threads, CancellationToken cancellationToken)
        {
            Queue<UUPFile> pending = new(items);
            Task<bool>[] workingSlots = new Task<bool>[threads];
            int workingThreadsCount = 0;

            GeneralDownloadProgress generalDownloadProgress = new()
            {
                DownloadedStatus = new FileDownloadStatus[threads],
                NumFiles = items.Count
            };

            bool result = true;
            while (pending.Count + workingThreadsCount != 0)
            {
                if (workingThreadsCount < threads && pending.Count != 0)
                {
                    UUPFile item = pending.Dequeue();
                    FileDownloadStatus fileStatus = new(item);
                    Progress<FileDownloadStatus> progress = new();

                    workingThreadsCount++;
                    GetFreeSlotIndex(workingSlots, out int freeSlotIndex);
                    generalDownloadProgress.DownloadedStatus[freeSlotIndex] = fileStatus;

                    progress.ProgressChanged += (s, e) =>
                    {
                        generalDownloadProgress.DownloadedTotalBytes += e.DownloadedBytes - generalDownloadProgress.DownloadedStatus[freeSlotIndex].DownloadedBytes;
                        generalDownloadProgress.DownloadedStatus[freeSlotIndex].DownloadedBytes = e.DownloadedBytes;
                        generalDownloadProgress.DownloadedStatus[freeSlotIndex].HashedBytes = e.HashedBytes;
                        generalDownloadProgress.DownloadedStatus[freeSlotIndex].FileStatus = e.FileStatus;
                        generalProgress?.Report(generalDownloadProgress);
                    };

                    workingSlots[freeSlotIndex] = Task.Run(async () => await func(item, progress, cancellationToken));
                }
                else
                {
                    _ = await Task.WhenAny(workingSlots.Where(t => t != null));

                    for (int i = 0; i < workingSlots.Length; i++)
                    {
                        if (workingSlots[i]?.IsCompleted == true)
                        {
                            if (workingSlots[i].Result)
                            {
                                generalDownloadProgress.NumFilesDownloadedSuccessfully++;
                            }
                            else
                            {
                                generalDownloadProgress.NumFilesDownloadedUnsuccessfully++;
                                result = false;
                            }

                            workingThreadsCount--;
                            workingSlots[i].Dispose();
                            workingSlots[i] = null;
                        }
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
            }

            return result;
        }

        private async ValueTask<bool> DownloadFile(UUPFile downloadFile, IProgress<FileDownloadStatus> progress, CancellationToken cancellationToken)
        {
            return await HttpDownload(DownloadFolderPath, downloadFile, _hc, VerifyFiles, progress, cancellationToken: cancellationToken);
        }

        private static async ValueTask<bool> HttpDownload(string basePath, UUPFile downloadFile, HttpClient httpClient, bool verifyFiles,
            IProgress<FileDownloadStatus> downloadProgress = null, int bufferSize = 65_536, CancellationToken cancellationToken = default)
        {
            long currRange = 0;
            long chunk = CHUNK_SIZE;
            long totalBytesRead = 0;
            long hashedBytes = 0;
            int blockBufferSize = bufferSize;

            //These variables are used to decrypt files if esrp is available.
            ESRPCryptography esrpDecrypter = null; //Implements IDisposable
            byte[] backBuffer = null;
            int backBufferLength = 0;
            long blockCount = 0;
            EsrpDecryptionInformation esrp = null;

            //If esrp is available, setup all required variable
            if (downloadFile.WUFile.EsrpDecryptionInformation != null)
            {
                esrp = downloadFile.WUFile.EsrpDecryptionInformation;
                backBuffer = new byte[blockBufferSize];
                esrpDecrypter = new ESRPCryptography(esrp);
                blockBufferSize = (int)esrp.EncryptionBufferSize;
            }

            try
            {
                //This path will be used for downloaded and validated files, moved/renamed
                string filePath = Path.Combine(basePath, downloadFile.FileName);

                //This path will be used for downloading files
                string tempFilePath = filePath + TEMP_DOWNLOAD_EXTENSION;

                //If we have an already completed file, prefer this under certain conditions.
                if (File.Exists(tempFilePath) && File.Exists(filePath))
                {
                    FileInfo tmpFileInfo = new(tempFilePath);
                    FileInfo fileInfo = new(filePath);
                    if (tmpFileInfo.Length == downloadFile.FileSize)
                    {
                        File.Delete(filePath);
                    }
                    else if (fileInfo.Length == downloadFile.FileSize)
                    {
                        File.Delete(tempFilePath);
                    }
                    else
                    {
                        File.Delete(filePath);
                    }
                }

                if (File.Exists(tempFilePath))
                {
                    FileInfo tmpFileInfo = new(tempFilePath);

                    if (tmpFileInfo.Length == downloadFile.FileSize)
                    {
                        /*//Decrypted file should match estimated bytes.
                        //Imagine if it crashed during hashing, the file may be valid.
                        //So... lets rename this file so it can be verified.
                        File.Move(tempFilePath, filePath);
                        totalBytesRead = tmpFileInfo.Length;*/
                        File.Delete(tempFilePath);
                    }
                    else if (downloadFile.WUFile.EsrpDecryptionInformation != null)
                    {
                        //Imagine if it crashed during decryption. We can only take the good part, 
                        //so we need to set the position to the last good block.
                        //This will round the last good block for us (until proven otherwise).
                        currRange = tmpFileInfo.Length / esrp.EncryptionBufferSize * esrp.EncryptionBufferSize;
                        totalBytesRead = currRange;

                        if (currRange > downloadFile.FileSize)
                        {
                            //If it's bigger then something with the file must have gone wrong.
                            //Reset currRange and delete the file.
                            currRange = 0;
                            totalBytesRead = 0;
                            File.Delete(tempFilePath);
                        }
                    }
                    else if (tmpFileInfo.Length < downloadFile.FileSize)
                    {
                        //Download the remaining part
                        currRange = tmpFileInfo.Length;
                        totalBytesRead = tmpFileInfo.Length;
                    }
                    else
                    {
                        //If it's bigger then we have a problem.
                        //Just... delete the file.
                        File.Delete(tempFilePath);
                    }
                }

                if (File.Exists(filePath))
                {
                    //If the filename was renamed then the download must have been completed successfully,
                    //we just hash to be sure that the file hasn't been tampered
                    if (verifyFiles)
                    {
                        FileStream fileStreamToHash = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                        totalBytesRead = fileStreamToHash.Length;
                        bool hashResult = await HashWithProgress(fileStreamToHash);
                        fileStreamToHash.Dispose();

                        if (hashResult)
                        {
                            //Nice, the file is ok, return success.
                            return true;
                        }
                        else
                        {
                            //The file is not ok.
                            //It's better to delete this file and redownload from scratch 
                            //instead of partially downloading the missing parts and found later that the entire file was corrupted.
                            File.Delete(filePath);

                            //This range may have been changed before (e.g. when assuming for temp. file), so let's set it to 0
                            currRange = 0;
                            totalBytesRead = 0;
                            hashedBytes = 0;
                        }
                    }
                    else
                    {
                        //At your own risk lol
                        FileInfo fileInfo = new(filePath);
                        downloadProgress?.Report(new FileDownloadStatus(downloadFile)
                        {
                            DownloadedBytes = fileInfo.Length,
                            FileStatus = FileStatus.Completed
                        });
                        return true;
                    }
                }

                //Before we need to create a directory.
                _ = Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                //Open the file as stream.
                using FileStream streamToWriteTo = File.Open(tempFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

                //Set the seek position to current range position (via totalBytesRead or currRange). This is needed.
                _ = streamToWriteTo.Seek(totalBytesRead, SeekOrigin.Begin);

                using HttpRequestMessage httpRequestMessageHead = new(HttpMethod.Head, new Uri(downloadFile.WUFile.DownloadUrl));
                using HttpResponseMessage response = await httpClient.SendAsync(httpRequestMessageHead, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                //Technically the  server reports both content-length and Accept-Ranges.
                long? contentLength = response.Content.Headers.ContentLength;
                bool hasAcceptRanges = response.Headers.AcceptRanges.Contains("bytes");

                //This is just a fallback in case the file delivery server goes nuts.
                if (!hasAcceptRanges)
                {
                    //We don't have a way to download from a specific range, hence we set the position to 0
                    //and download everything from scratch... Sadly.
                    //TODO
                    _ = streamToWriteTo.Seek(0, SeekOrigin.Begin);

                    downloadProgress?.Report(new FileDownloadStatus(downloadFile)
                    {
                        DownloadedBytes = 0,
                        FileStatus = FileStatus.Downloading
                    });

                    //TODO: add download reporting for this
                    //TODO: may throw an exception if the server suddently closes the connection (e.g. file expired.)

                    using HttpResponseMessage fullFileResp = await httpClient.GetAsync(downloadFile.WUFile.DownloadUrl, cancellationToken);
                    using Stream streamToReadFrom = await fullFileResp.Content.ReadAsStreamAsync();

                    if (esrpDecrypter != null)
                    {
                        await esrpDecrypter.DecryptStreamFullAsync(streamToReadFrom, streamToWriteTo, (ulong)contentLength.Value, cancellationToken);
                    }
                    else
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }

                    downloadProgress?.Report(new FileDownloadStatus(downloadFile)
                    {
                        DownloadedBytes = contentLength.Value,
                        FileStatus = FileStatus.Downloading
                    });

                    //just an assumption 

                    _ = streamToWriteTo.Seek(0, SeekOrigin.Begin);
                    return await HashWithProgress(streamToWriteTo);
                }

                while (currRange < contentLength.Value)
                {
                    //Calculate range values
                    if (currRange + chunk >= contentLength.Value)
                    {
                        chunk = contentLength.Value - currRange - 1;
                    }

                    //Create request for range and send it, return asap (we just need the header to see if the status code is ok)
                    using HttpRequestMessage requestMessageRange = CreateRequestHeaderForRange(HttpMethod.Get, downloadFile.WUFile.DownloadUrl, currRange, currRange + chunk);
                    using HttpResponseMessage filePartResp = await httpClient.SendAsync(requestMessageRange, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                    //increment to the next range
                    currRange += chunk + 1;

                    //If the server has replied with 200 ok/206 partial content
                    if (filePartResp.IsSuccessStatusCode)
                    {
                        //get the underlying stream
                        using Stream streamToReadFrom = await filePartResp.Content.ReadAsStreamAsync();

                        int bytesRead;
                        byte[] buffer = new byte[blockBufferSize];

                        //read the content
                        //TODO: it may throw an exception (stream closed because file expired?)
                        //In that case we would wrap into another try catch and try to read the reason behind this
                        while ((bytesRead = await streamToReadFrom.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                        {
                            totalBytesRead += bytesRead;

                            //If esrp is not null, we must use a backbuffer
                            //Basically we need exactly (bufferSize) to transform a block for (symmetric crypto algorithm)
                            //This does all the calculation to store in a back buffer because (bytesRead) does NOT always equal to (bufferSize)
                            if (esrpDecrypter != null)
                            {
                                int diff = blockBufferSize - (backBufferLength + bytesRead);
                                diff = Math.Min(diff, 0);

                                Array.Copy(buffer, 0, backBuffer, backBufferLength, bytesRead + diff);
                                backBufferLength += bytesRead + diff;

                                if (backBufferLength >= blockBufferSize)
                                {
                                    //The last block is always padded.
                                    bool needsPadding = totalBytesRead == contentLength.Value;
                                    long previousSumBlockLength = blockBufferSize * blockCount;
                                    await esrpDecrypter.DecryptBufferToStreamAsync(backBuffer, streamToWriteTo, backBufferLength,
                                                previousSumBlockLength, needsPadding, cancellationToken);

                                    //if there is still data in buffer, copy the remaining one into the backbuffer
                                    if (diff < 0)
                                    {
                                        Array.Copy(buffer, bytesRead + diff, backBuffer, 0, -diff);
                                        backBufferLength = -diff;
                                    }
                                    else
                                    {
                                        backBufferLength = 0;
                                    }

                                    //increment the block count, this is used for IV transform for the encryption.
                                    blockCount++;
                                }
                            }
                            else
                            {
                                //simply write to the file
                                await streamToWriteTo.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                            }

                            //report progress
                            downloadProgress?.Report(new FileDownloadStatus(downloadFile)
                            {
                                DownloadedBytes = totalBytesRead,
                                FileStatus = FileStatus.Downloading
                            });
                        }
                    }
                    else
                    {
                        if (filePartResp.StatusCode is HttpStatusCode.Forbidden or
                            HttpStatusCode.NotFound or
                            HttpStatusCode.Unauthorized)
                        {
                            //The url is expired.
                            //Report that is expired and return false
                            //We need to keep the file, because it's just incomplete, not corrupted.
                            downloadProgress?.Report(new FileDownloadStatus(downloadFile)
                            {
                                DownloadedBytes = totalBytesRead,
                                FileStatus = FileStatus.Expired
                            });

                            return false;
                        }
                        else
                        {
                            throw new Exception(filePartResp.ReasonPhrase);
                        }
                    }
                }

                //last left block if any

                if (esrpDecrypter != null && backBufferLength > 0)
                {
                    await esrpDecrypter.DecryptBufferToStreamAsync(backBuffer, streamToWriteTo, backBufferLength,
                                            blockBufferSize * blockCount, true, cancellationToken);
                }

                if (verifyFiles)
                {
                    _ = streamToWriteTo.Seek(0, SeekOrigin.Begin);
                    bool hashResult = await HashWithProgress(streamToWriteTo);

                    if (hashResult)
                    {
                        streamToWriteTo.Close();
                        File.Move(tempFilePath, filePath);
                    }

                    return hashResult;
                }
                else
                {
                    streamToWriteTo.Close();
                    File.Move(tempFilePath, filePath);

                    downloadProgress?.Report(new FileDownloadStatus(downloadFile)
                    {
                        DownloadedBytes = totalBytesRead,
                        FileStatus = FileStatus.Completed
                    });

                    return true;
                }
            }
            catch //(Exception ex)
            {
                downloadProgress?.Report(new FileDownloadStatus(downloadFile)
                {
                    DownloadedBytes = totalBytesRead,
                    FileStatus = FileStatus.Failed
                });

                return false;
            }
            finally
            {
                esrpDecrypter?.Dispose();
            }

            //C# 7 - Local Functions
            async ValueTask<bool> HashWithProgress(Stream strm)
            {
                Progress<long> progressHashedBytes = new();
                progressHashedBytes.ProgressChanged += (s, e) =>
                {
                    hashedBytes = e;
                    downloadProgress?.Report(new FileDownloadStatus(downloadFile)
                    {
                        DownloadedBytes = totalBytesRead,
                        HashedBytes = hashedBytes,
                        FileStatus = FileStatus.Verifying
                    });
                };

                bool hashMatches = true;
                switch (downloadFile.HashAlgorithm.ToLower())
                {
                    case "sha1":
                        hashMatches = await IsDownloadedFileValidSHA1(strm, downloadFile.Hash,
                                                        progressHashedBytes, cancellationToken);
                        break;
                    case "sha256":
                        hashMatches = await IsDownloadedFileValidSHA256(strm, downloadFile.Hash,
                                                        progressHashedBytes, cancellationToken);
                        break;
                }

                if (hashMatches)
                {
                    downloadProgress?.Report(new FileDownloadStatus(downloadFile)
                    {
                        DownloadedBytes = totalBytesRead,
                        HashedBytes = hashedBytes,
                        FileStatus = FileStatus.Completed
                    });
                    return true;
                }
                else
                {
                    downloadProgress?.Report(new FileDownloadStatus(downloadFile)
                    {
                        DownloadedBytes = totalBytesRead,
                        HashedBytes = hashedBytes,
                        FileStatus = FileStatus.Failed
                    });
                    return false;
                }
            }
        }

        #region Helpers

        private static async ValueTask<bool> IsDownloadedFileValidSHA256(Stream fileStream, string base64Hash, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {
            using SHA256 hashAlgo = SHA256.Create();
            byte[] hashByte = await ComputeHashAsyncT(hashAlgo, fileStream, progress, cancellationToken: cancellationToken);
            return ByteArraySpanCompare(Convert.FromBase64String(base64Hash), hashByte);
        }

        private static async ValueTask<bool> IsDownloadedFileValidSHA1(Stream fileStream, string base64Hash, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {
            using SHA1 hashAlgo = SHA1.Create();
            byte[] hashByte = await ComputeHashAsyncT(hashAlgo, fileStream, progress, cancellationToken: cancellationToken);
            return ByteArraySpanCompare(Convert.FromBase64String(base64Hash), hashByte);
        }

        public static async ValueTask<byte[]> ComputeHashAsyncT(HashAlgorithm hashAlgorithm, Stream fileStream, IProgress<long> progress = null,
                    int bufferSize = 1_048_576, CancellationToken cancellationToken = default)
        {
            int readBytes;
            long totalBytesRead = 0;
            long bufSizeEffective = Math.Min(bufferSize, fileStream.Length);
            byte[] buffer = new byte[bufSizeEffective];
            using MemoryStream ms = new(buffer);
            using CryptoStream cs = new(ms, hashAlgorithm, CryptoStreamMode.Write);
            while ((readBytes = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await cs.WriteAsync(buffer, 0, readBytes, cancellationToken);
                ms.Position = 0;
                totalBytesRead += readBytes;
                progress?.Report(totalBytesRead);
                cancellationToken.ThrowIfCancellationRequested();
            }
            cs.FlushFinalBlock();
            return hashAlgorithm.Hash;
        }

        private static bool ByteArraySpanCompare(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
        {
            return a1.SequenceEqual(a2);
        }

        private static void GetFreeSlotIndex<T>(T[] array, out int firstFreeTaskIndex)
        {
            firstFreeTaskIndex = -1;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    firstFreeTaskIndex = i;
                    break;
                }
            }
        }

        private static HttpRequestMessage CreateRequestHeaderForRange(HttpMethod method, string url, long from, long to)
        {
            HttpRequestMessage request = new(method, new Uri(url));
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(from, to);
            return request;
        }

        #endregion

        public void Dispose()
        {
            //Release all resources that have been instanced and used
            _hc.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}