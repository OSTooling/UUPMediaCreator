using System;
using System.Security.Cryptography;
using System.Text;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting
{
    public static class ComputerHardwareIDProvider
    {
        private static readonly byte[] hardwareIDIv = [0x70, 0xFF, 0xD8, 0x12, 0x4C, 0x7F, 0x4C, 0x7D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];

        public static Guid Class5GuidFromString(string input)
        {
            byte[] hash = GetPartialHash(input);
            ScrambleHash(hash);

            return new Guid(hash);
        }

        private static byte[] GetPartialHash(string input)
        {
            byte[] partialHash;

            using SHA1 sha1Csp = SHA1.Create();

            _ = sha1Csp.TransformBlock(hardwareIDIv, 0, hardwareIDIv.Length, null, 0);

            byte[] dataBin = Encoding.Unicode.GetBytes(input);
            _ = sha1Csp.TransformFinalBlock(dataBin, 0, dataBin.Length);

            partialHash = new byte[16];
            Array.Copy(sha1Csp.Hash, partialHash, partialHash.Length);

            return partialHash;
        }

        // Changes little endian GUID components to big endian, then does some ANDs and ORs
        private static unsafe void ScrambleHash(byte[] hash)
        {
            fixed (byte* shPtr = hash)
            {
                *(uint*)shPtr = SwapBytes32(*(uint*)shPtr);
                *((ushort*)shPtr + 2) = SwapBytes16(*((ushort*)shPtr + 2));
                *((ushort*)shPtr + 3) = (ushort)((SwapBytes16(*((ushort*)shPtr + 3)) & 0xFFF) | 0x5000);
                *(shPtr + 8) &= 0x3F;
                *(shPtr + 8) |= 0x80;
            }
        }

        private static ushort SwapBytes16(ushort x)
        {
            return (ushort)((x >> 8) | (x << 8));
        }

        private static uint SwapBytes32(uint x)
        {
            x = (x >> 16) | (x << 16);
            return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
        }
    }
}