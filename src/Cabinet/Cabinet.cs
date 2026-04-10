/*
 * Copyright (c) Gustave Monce
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
using Cabinet.Xna;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Cabinet
{
    internal class CabinetHeader
    {
        /// <summary>
        /// The raw header structure of the cabinet file
        /// </summary>
        public CFHEADER CabinetFileHeader
        {
            get; set;
        }

        /// <summary>
        /// Additional Application-specific data for the header
        /// </summary>
        public byte[] AdditionalApplicationData
        {
            get; set;
        }

        /// <summary>
        /// Additional Application-specific data size for the volumes
        /// </summary>
        public byte VolumeAdditionalApplicationDataSize
        {
            get; set;
        }

        /// <summary>
        /// Additional Application-specific data size for the data
        /// </summary>
        public byte DataAdditionalApplicationDataSize
        {
            get; set;
        }
    }

    internal class CabinetVolume
    {
        /// <summary>
        /// The raw folder structure of the volume
        /// </summary>
        public CFFOLDER CabinetFileVolume
        {
            get; set;
        }

        /// <summary>
        /// Additional Application-specific data for the volume
        /// </summary>
        public byte[] AdditionalApplicationData
        {
            get; set;
        }

        public List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)> DataMap
        {
            get; set;
        }

        public List<(CabinetVolumeFile file, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset)> BlockMap
        {
            get; set;
        }
    }

    internal class CabinetVolumeFile
    {
        /// <summary>
        /// The raw folder structure of the file in the volume
        /// </summary>
        public CFFILE CabinetFileVolumeFile
        {
            get; set;
        }

        /// <summary>
        /// The file name
        /// </summary>
        public string FileName
        {
            get; set;
        }
    }

    internal class CabinetData
    {
        /// <summary>
        /// The raw folder structure of the data in the volume
        /// </summary>
        public CFDATA CabinetFileData
        {
            get; set;
        }

        /// <summary>
        /// Additional Application-specific data for the data
        /// </summary>
        public byte[] AdditionalApplicationData
        {
            get; set;
        }

        /// <summary>
        /// The offset to the data payload described by this object in the cabinet file
        /// </summary>
        public uint CabinetFileDataPayloadOffset
        {
            get; set;
        }
    }

    public class CabinetFile
    {
        /// <summary>
        /// The file size
        /// </summary>
        public uint UncompressedSize
        {
            get; set;
        }

        /// <summary>
        /// The file timestamp
        /// </summary>
        public DateTime TimeStamp
        {
            get; set;
        }

        /// <summary>
        /// The file attributes
        /// </summary>
        public FileAttributes FileAttributes
        {
            get; set;
        }

        /// <summary>
        /// The file name
        /// </summary>
        public string FileName
        {
            get; set;
        }

        internal CabinetFile(CabinetVolumeFile file)
        {
            FileName = file.FileName;
            FileAttributes = file.CabinetFileVolumeFile.GetFileAttributes();
            TimeStamp = file.CabinetFileVolumeFile.GetDateTime();
            UncompressedSize = file.CabinetFileVolumeFile.cbFile;
        }

        public override string ToString()
        {
            return FileName;
        }
    }

    public class Cabinet
    {
        private readonly byte[] CabinetMagic = "MSCF"u8.ToArray();

        private readonly CabinetHeader cabinetHeader;
        private readonly IReadOnlyCollection<CabinetVolume> volumes;
        private readonly IReadOnlyCollection<CabinetVolumeFile> files;
        //private readonly string InputFile;
        private readonly Stream InputStream;

        public Cabinet(Stream Stream)
        {
            InputStream = Stream;
            _ = InputStream.Seek(0, SeekOrigin.Begin);

            cabinetHeader = ReadHeader(InputStream);
            volumes = ReadVolumes(InputStream);
            files = ReadVolumeFiles(InputStream);

            for (int i = 0; i < volumes.Count; i++)
            {
                CabinetVolume volume = volumes.ElementAt(i);
                volume.DataMap = BuildDataMap(volume);
                volume.BlockMap = BuildBlockMap(i, volume.DataMap);
            }

            _ = InputStream.Seek(0, SeekOrigin.Begin);
        }

        public IReadOnlyCollection<CabinetFile> Files => files.Select(x => new CabinetFile(x)).ToList();

        #region Reading Metadata
        private CabinetHeader ReadHeader(Stream cabinetStream)
        {
            byte cbCFFolder = 0;
            byte cbCFData = 0;
            byte[] AdditionalData = [];

            BinaryReader cabinetBinaryReader = new(cabinetStream);

            CFHEADER header = cabinetBinaryReader.BaseStream.ReadStruct<CFHEADER>();

            if (StructuralComparisons.StructuralComparer.Compare(header.signature, CabinetMagic) != 0)
            {
                throw new Exception($"Bad Cabinet: Invalid Signature: {header.signature:X}");
            }

            if ((header.flags & CFHEADER.Options.ReservePresent) != 0)
            {
                ushort cbCFHeader = cabinetBinaryReader.ReadUInt16();
                cbCFFolder = cabinetBinaryReader.ReadByte();
                cbCFData = cabinetBinaryReader.ReadByte();
                AdditionalData = cabinetBinaryReader.ReadBytes(cbCFHeader);
            }

            if ((header.flags & CFHEADER.Options.PreviousCabinet) != 0)
            {
                string prevCab = cabinetBinaryReader.BaseStream.ReadString();
                string prevDisk = cabinetBinaryReader.BaseStream.ReadString();

                throw new Exception($"Unsupported Cabinet: Multi Part: {prevCab}/{prevDisk}");
            }

            if ((header.flags & CFHEADER.Options.NextCabinet) != 0)
            {
                string prevCab = cabinetBinaryReader.BaseStream.ReadString();
                string prevDisk = cabinetBinaryReader.BaseStream.ReadString();

                throw new Exception($"Unsupported Cabinet: Multi Part: {prevCab}/{prevDisk}");
            }

            return new CabinetHeader()
            {
                CabinetFileHeader = header,
                AdditionalApplicationData = AdditionalData,
                VolumeAdditionalApplicationDataSize = cbCFFolder,
                DataAdditionalApplicationDataSize = cbCFData
            };
        }

        private IReadOnlyCollection<CabinetVolume> ReadVolumes(Stream cabinetStream)
        {
            BinaryReader cabinetBinaryReader = new(cabinetStream);

            List<CabinetVolume> volumes = [];
            for (int i = 0; i < cabinetHeader.CabinetFileHeader.cFolders; i++)
            {
                CabinetVolume volume = new()
                {
                    CabinetFileVolume = cabinetStream.ReadStruct<CFFOLDER>(),
                    AdditionalApplicationData = cabinetHeader.VolumeAdditionalApplicationDataSize > 0 ? cabinetBinaryReader.ReadBytes(cabinetHeader.VolumeAdditionalApplicationDataSize) : []
                };

                if (volume.CabinetFileVolume.typeCompress is not CFFOLDER.CFTYPECOMPRESS.TYPE_LZX and
                    not CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP and
                    not CFFOLDER.CFTYPECOMPRESS.TYPE_NONE)
                {
                    throw new Exception("Unsupported Cabinet: Only LZX, MSZip and Store is currently supported");
                }

                if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_LZX)
                {
                    if (volume.CabinetFileVolume.typeCompressOption is < 15 or > 21)
                    {
                        throw new Exception("Unsupported Cabinet: LZX variable does not fall in supported ranges");
                    }
                }

                volumes.Add(volume);
            }

            return volumes;
        }

        private IReadOnlyCollection<CabinetVolumeFile> ReadVolumeFiles(Stream cabinetStream)
        {
            BinaryReader cabinetBinaryReader = new(cabinetStream);

            if (cabinetBinaryReader.BaseStream.Position != cabinetHeader.CabinetFileHeader.coffFiles)
            {
                throw new Exception("Bad Cabinet: First File Block does not match header");
            }

            List<CabinetVolumeFile> files = [];
            for (int i = 0; i < cabinetHeader.CabinetFileHeader.cFiles; i++)
            {
                CFFILE file = cabinetBinaryReader.BaseStream.ReadStruct<CFFILE>();

                string name = file.IsFileNameUTF8() ? cabinetBinaryReader.BaseStream.ReadUTF8tring() : cabinetBinaryReader.BaseStream.ReadString();
                files.Add(new CabinetVolumeFile()
                {
                    CabinetFileVolumeFile = file,
                    FileName = name.Replace('\\', Path.DirectorySeparatorChar)
                });
            }

            return files;
        }
        #endregion

        private List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)> BuildDataMap(CabinetVolume volume)
        {
            List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)> datas = [];

            // Build Data Map
            using BinaryReader cabinetBinaryReader = new(InputStream, System.Text.Encoding.UTF8, true);
            _ = InputStream.Seek(volume.CabinetFileVolume.firstDataBlockOffset, SeekOrigin.Begin);

            int offset = 0;
            for (int i = 0; i < volume.CabinetFileVolume.dataBlockCount; i++)
            {
                CFDATA CabinetData = cabinetBinaryReader.BaseStream.ReadStruct<CFDATA>();
                _ = cabinetBinaryReader.BaseStream.Seek(cabinetHeader.DataAdditionalApplicationDataSize, SeekOrigin.Current);
                datas.Add((CabinetData, (int)cabinetBinaryReader.BaseStream.Position, offset, offset + CabinetData.cbUncomp - 1, i));
                _ = cabinetBinaryReader.BaseStream.Seek(CabinetData.cbData, SeekOrigin.Current);
                offset += CabinetData.cbUncomp;
            }

            return datas;
        }

        private List<(CabinetVolumeFile file, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset)> BuildBlockMap(int volumeIndex, List<(CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index)> datas)
        {
            List<(CabinetVolumeFile file, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset)> fileBlockMap = [];

            // Build Block Map
            foreach (CabinetVolumeFile file in files)
            {
                if (file.CabinetFileVolumeFile.iFolder != volumeIndex || file.CabinetFileVolumeFile.cbFile == 0)
                {
                    continue;
                }

                (CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index) = datas.First(x => x.beginFolderOffset <= file.CabinetFileVolumeFile.uoffFolderStart &&
                                                    file.CabinetFileVolumeFile.uoffFolderStart <= x.endFolderOffset);
                (CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index) LastBlock = datas.First(x => x.beginFolderOffset <= (file.CabinetFileVolumeFile.uoffFolderStart + file.CabinetFileVolumeFile.cbFile - 1) &&
                                                    (file.CabinetFileVolumeFile.uoffFolderStart + file.CabinetFileVolumeFile.cbFile - 1) <= x.endFolderOffset);

                int fileBeginFolderOffset = (int)file.CabinetFileVolumeFile.uoffFolderStart;
                int fileEndFolderOffset = (int)file.CabinetFileVolumeFile.uoffFolderStart + (int)file.CabinetFileVolumeFile.cbFile - 1;

                int start = (int)file.CabinetFileVolumeFile.uoffFolderStart - beginFolderOffset;
                int end = fileEndFolderOffset - LastBlock.beginFolderOffset;

                fileBlockMap.Add((file, index, start, LastBlock.index, end));
            }

            return fileBlockMap;
        }

        public void ExtractAllFiles(string OutputDirectory, Action<int, string> progressCallBack = null)
        {
            // Cleanup existing files
            foreach (CabinetVolumeFile file in files)
            {
                string destination = Path.Combine(OutputDirectory, file.FileName);
                if (!Directory.Exists(Path.GetDirectoryName(destination)))
                {
                    _ = Directory.CreateDirectory(Path.GetDirectoryName(destination));
                }

                if (File.Exists(destination))
                {
                    File.Delete(destination);
                }
            }

            using BinaryReader cabinetBinaryReader = new(InputStream, System.Text.Encoding.UTF8, true);

            // Do the extraction
            for (int volumeIndex = 0; volumeIndex < volumes.Count; volumeIndex++)
            {
                CabinetVolume volume = volumes.ElementAt(volumeIndex);

                LzxDecoder lzx = null;
                Inflater inf = null;
                if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_LZX)
                {
                    lzx = new LzxDecoder(volume.CabinetFileVolume.typeCompressOption);
                }

                if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP)
                {
                    inf = new Inflater(true);
                }

                int fcount = 0;

                for (int i = 0; i < volume.CabinetFileVolume.dataBlockCount; i++)
                {
                    (CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index) = volume.DataMap[i];

                    _ = cabinetBinaryReader.BaseStream.Seek(dataOffsetCabinet, SeekOrigin.Begin);

                    byte[] uncompressedDataBlock = new byte[dataStruct.cbUncomp];
                    byte[] compressedDataBlock = null;

                    if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP)
                    {
                        byte[] magic = cabinetBinaryReader.ReadBytes(2);

                        if (StructuralComparisons.StructuralComparer.Compare(magic, "CK"u8.ToArray()) != 0)
                        {
                            throw new Exception("Bad Cabinet: Invalid Signature for MSZIP block");
                        }

                        compressedDataBlock = cabinetBinaryReader.ReadBytes(dataStruct.cbData - 2);
                    }
                    else
                    {
                        compressedDataBlock = cabinetBinaryReader.ReadBytes(dataStruct.cbData);
                    }

                    using (MemoryStream uncompressedDataBlockStream = new(uncompressedDataBlock))
                    using (MemoryStream compressedDataBlockStream = new(compressedDataBlock))
                    {
                        ExpandBlock(uncompressedDataBlockStream, compressedDataBlockStream, volume.CabinetFileVolume.typeCompress, lzx, inf);
                    }

                    foreach (CabinetVolumeFile file in files)
                    {
                        if (file.CabinetFileVolumeFile.iFolder != volumeIndex)
                        {
                            continue;
                        }

                        if (file.CabinetFileVolumeFile.cbFile != 0)
                        {
                            (CabinetVolumeFile file, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset) mapping = volume.BlockMap.First(x => x.file.FileName == file.FileName);

                            // This block contains this file
                            if (mapping.startingBlock <= i && i <= mapping.endingBlock)
                            {
                                int start = 0;
                                int end = dataStruct.cbUncomp - 1;

                                bool IsFirstBlock = mapping.startingBlock == i;
                                bool IsLastBlock = mapping.endingBlock == i;

                                if (IsFirstBlock)
                                {
                                    progressCallBack?.Invoke((int)Math.Round(fcount / (double)cabinetHeader.CabinetFileHeader.cFiles * 100d), mapping.file.FileName);
                                    start = mapping.startingBlockOffset;
                                }

                                if (IsLastBlock)
                                {
                                    end = mapping.endingBlockOffset;
                                    fcount++;
                                }

                                int count = end - start + 1;

                                string destination = Path.Combine(OutputDirectory, file.FileName);
                                using (FileStream uncompressedDataStream = File.Open(destination, FileMode.OpenOrCreate))
                                {
                                    _ = uncompressedDataStream.Seek(0, SeekOrigin.End);
                                    uncompressedDataStream.Write(uncompressedDataBlock, start, count);
                                }

                                if (IsLastBlock)
                                {
                                    File.SetAttributes(destination, file.CabinetFileVolumeFile.GetFileAttributes());
                                    DateTime dt = file.CabinetFileVolumeFile.GetDateTime();
                                    File.SetCreationTimeUtc(destination, dt);
                                    File.SetLastWriteTimeUtc(destination, dt);
                                    File.SetLastAccessTimeUtc(destination, dt);
                                }
                            }
                        }
                        else
                        {
                            string destination = Path.Combine(OutputDirectory, file.FileName);
                            File.Create(destination).Dispose();
                            File.SetAttributes(destination, file.CabinetFileVolumeFile.GetFileAttributes());
                            DateTime dt = file.CabinetFileVolumeFile.GetDateTime();
                            File.SetCreationTimeUtc(destination, dt);
                            File.SetLastWriteTimeUtc(destination, dt);
                            File.SetLastAccessTimeUtc(destination, dt);
                        }
                    }
                }
            }
        }

        public byte[] ReadFile(string FileName)
        {
            CabinetVolumeFile file = files.First(x => x.FileName.Equals(FileName, StringComparison.CurrentCultureIgnoreCase));
            int volumeIndex = file.CabinetFileVolumeFile.iFolder;

            byte[] destination = new byte[file.CabinetFileVolumeFile.cbFile];
            using MemoryStream uncompressedDataStream = new(destination);
            _ = uncompressedDataStream.Seek(0, SeekOrigin.Begin);

            // Do the extraction
            CabinetVolume volume = volumes.ElementAt(volumeIndex);

            LzxDecoder lzx = null;
            Inflater inf = null;
            if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_LZX)
            {
                lzx = new LzxDecoder(volume.CabinetFileVolume.typeCompressOption);
            }

            if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP)
            {
                inf = new Inflater(true);
            }

            using BinaryReader cabinetBinaryReader = new(InputStream, System.Text.Encoding.UTF8, true);

            int fcount = 0;

            for (int i = 0; i < volume.CabinetFileVolume.dataBlockCount; i++)
            {
                (CFDATA dataStruct, int dataOffsetCabinet, int beginFolderOffset, int endFolderOffset, int index) = volume.DataMap[i];

                _ = cabinetBinaryReader.BaseStream.Seek(dataOffsetCabinet, SeekOrigin.Begin);

                byte[] uncompressedDataBlock = new byte[dataStruct.cbUncomp];
                byte[] compressedDataBlock = null;

                if (volume.CabinetFileVolume.typeCompress == CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP)
                {
                    byte[] magic = cabinetBinaryReader.ReadBytes(2);

                    if (StructuralComparisons.StructuralComparer.Compare(magic, "CK"u8.ToArray()) != 0)
                    {
                        throw new Exception("Bad Cabinet: Invalid Signature for MSZIP block");
                    }

                    compressedDataBlock = cabinetBinaryReader.ReadBytes(dataStruct.cbData - 2);
                }
                else
                {
                    compressedDataBlock = cabinetBinaryReader.ReadBytes(dataStruct.cbData);
                }

                using (MemoryStream uncompressedDataBlockStream = new(uncompressedDataBlock))
                using (MemoryStream compressedDataBlockStream = new(compressedDataBlock))
                {
                    ExpandBlock(uncompressedDataBlockStream, compressedDataBlockStream, volume.CabinetFileVolume.typeCompress, lzx, inf);
                }

                if (file.CabinetFileVolumeFile.cbFile != 0)
                {
                    (CabinetVolumeFile file, int startingBlock, int startingBlockOffset, int endingBlock, int endingBlockOffset) mapping = volume.BlockMap.First(x => x.file.FileName == file.FileName);

                    // This block contains this file
                    if (mapping.startingBlock <= i && i <= mapping.endingBlock)
                    {
                        int start = 0;
                        int end = dataStruct.cbUncomp - 1;

                        bool IsFirstBlock = mapping.startingBlock == i;
                        bool IsLastBlock = mapping.endingBlock == i;

                        if (IsFirstBlock)
                        {
                            start = mapping.startingBlockOffset;
                        }

                        if (IsLastBlock)
                        {
                            end = mapping.endingBlockOffset;
                            fcount++;
                        }

                        int count = end - start + 1;

                        uncompressedDataStream.Write(uncompressedDataBlock, start, count);
                    }
                }
            }

            return destination;
        }

        private static void ExpandBlock(Stream uncompressedDataBlockStream, Stream compressedDataBlockStream, CFFOLDER.CFTYPECOMPRESS compressionType, LzxDecoder lzx, Inflater inf)
        {
            switch (compressionType)
            {
                case CFFOLDER.CFTYPECOMPRESS.TYPE_LZX:
                    {
                        _ = lzx.Decompress(compressedDataBlockStream, (int)compressedDataBlockStream.Length, uncompressedDataBlockStream, (int)uncompressedDataBlockStream.Length);
                        break;
                    }

                case CFFOLDER.CFTYPECOMPRESS.TYPE_NONE:
                    {
                        compressedDataBlockStream.CopyTo(uncompressedDataBlockStream);
                        break;
                    }
                case CFFOLDER.CFTYPECOMPRESS.TYPE_MSZIP:
                    {
                        using BinaryReader br = new(compressedDataBlockStream);
                        using BinaryWriter bw = new(uncompressedDataBlockStream);
                        byte[] inBuffer = br.ReadBytes((int)br.BaseStream.Length);
                        inf.SetInput(inBuffer);
                        byte[] outBuffer = new byte[bw.BaseStream.Length];
                        int ret = inf.Inflate(outBuffer);
                        if (ret == 0)
                        {
                            throw new Exception("Inflate failed");
                        }
                        bw.Write(outBuffer);
                        inf.Reset();
                        break;
                    }
            }
        }
    }
}