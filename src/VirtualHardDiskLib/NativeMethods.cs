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
using System.Runtime.InteropServices;
using System.Text;

namespace VirtualHardDiskLib
{
    internal static class NativeMethods
    {
        [Flags]
        public enum ATTACH_VIRTUAL_DISK_FLAG
        {
            ATTACH_VIRTUAL_DISK_FLAG_NONE = 0x00000000,
            ATTACH_VIRTUAL_DISK_FLAG_READ_ONLY = 0x00000001,
            ATTACH_VIRTUAL_DISK_FLAG_NO_DRIVE_LETTER = 0x00000002,
            ATTACH_VIRTUAL_DISK_FLAG_PERMANENT_LIFETIME = 0x00000004,
            ATTACH_VIRTUAL_DISK_FLAG_NO_LOCAL_HOST = 0x00000008
        }

        public enum ATTACH_VIRTUAL_DISK_VERSION
        {
            ATTACH_VIRTUAL_DISK_VERSION_UNSPECIFIED = 0,
            ATTACH_VIRTUAL_DISK_VERSION_1 = 1
        }

        [Flags]
        public enum DETACH_VIRTUAL_DISK_FLAG
        {
            DETACH_VIRTUAL_DISK_FLAG_NONE = 0x00000000
        }

        [Flags]
        public enum OPEN_VIRTUAL_DISK_FLAG
        {
            OPEN_VIRTUAL_DISK_FLAG_NONE = 0x00000000,
            OPEN_VIRTUAL_DISK_FLAG_NO_PARENTS = 0x00000001,
            OPEN_VIRTUAL_DISK_FLAG_BLANK_FILE = 0x00000002,
            OPEN_VIRTUAL_DISK_FLAG_BOOT_DRIVE = 0x00000004
        }

        public enum OPEN_VIRTUAL_DISK_VERSION
        {
            OPEN_VIRTUAL_DISK_VERSION_1 = 1
        }

        public enum VIRTUAL_DISK_ACCESS_MASK
        {
            VIRTUAL_DISK_ACCESS_ATTACH_RO = 0x00010000,
            VIRTUAL_DISK_ACCESS_ATTACH_RW = 0x00020000,
            VIRTUAL_DISK_ACCESS_DETACH = 0x00040000,
            VIRTUAL_DISK_ACCESS_GET_INFO = 0x00080000,
            VIRTUAL_DISK_ACCESS_READ = 0x000d0000,
            VIRTUAL_DISK_ACCESS_CREATE = 0x00100000,
            VIRTUAL_DISK_ACCESS_METAOPS = 0x00200000,
            VIRTUAL_DISK_ACCESS_WRITABLE = 0x00320000,
            VIRTUAL_DISK_ACCESS_ALL = 0x003f0000
        }

        public const int ERROR_SUCCESS = 0;
        public const int OPEN_VIRTUAL_DISK_RW_DEPTH_DEFAULT = 1;
        public const int VIRTUAL_STORAGE_TYPE_DEVICE_VHD = 2;

        public static readonly Guid VIRTUAL_STORAGE_TYPE_VENDOR_MICROSOFT =
            new("EC984AEC-A0F9-47e9-901F-71415A66345B");

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int AttachVirtualDisk(IntPtr VirtualDiskHandle, IntPtr SecurityDescriptor,
            ATTACH_VIRTUAL_DISK_FLAG Flags, int ProviderSpecificFlags, ref ATTACH_VIRTUAL_DISK_PARAMETERS Parameters,
            IntPtr Overlapped);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int DetachVirtualDisk(IntPtr VirtualDiskHandle, DETACH_VIRTUAL_DISK_FLAG Flags,
            int ProviderSpecificFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int OpenVirtualDisk(ref VIRTUAL_STORAGE_TYPE VirtualStorageType, string Path,
            VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask, OPEN_VIRTUAL_DISK_FLAG Flags,
            ref OPEN_VIRTUAL_DISK_PARAMETERS Parameters, ref IntPtr Handle);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ATTACH_VIRTUAL_DISK_PARAMETERS
        {
            public ATTACH_VIRTUAL_DISK_VERSION Version;
            public ATTACH_VIRTUAL_DISK_PARAMETERS_Version1 Version1;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct ATTACH_VIRTUAL_DISK_PARAMETERS_Version1
        {
            public int Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OPEN_VIRTUAL_DISK_PARAMETERS
        {
            public OPEN_VIRTUAL_DISK_VERSION Version;
            public OPEN_VIRTUAL_DISK_PARAMETERS_Version1 Version1;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OPEN_VIRTUAL_DISK_PARAMETERS_Version1
        {
            public int RWDepth;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct VIRTUAL_STORAGE_TYPE
        {
            public int DeviceId;
            public Guid VendorId;
        }

        [DllImport("virtdisk.dll", CharSet = CharSet.Unicode)]
        public static extern int GetVirtualDiskPhysicalPath(IntPtr virtualDiskHandle, ref int diskPathSizeInBytes, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder diskPath);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindFirstVolume([MarshalAs(UnmanagedType.LPWStr)] StringBuilder volumeName, int bufferLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindVolumeClose(IntPtr findVolumeHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FindNextVolume(IntPtr findVolumeHandle, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder volumeName, int bufferLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPWStr)] string fileName, GENERIC_ACCESS_RIGHTS_FLAGS desiredAccess, FILE_SHARE_MODE_FLAGS shareMode, IntPtr securityAttribute, CREATION_DISPOSITION_FLAGS creationDisposition, int flagsAndAttributes, IntPtr templateFile);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeviceIoControl(IntPtr deviceHandle, IO_CONTROL_CODE controlCode, IntPtr inBuffer, uint inBufferSize, ref STORAGE_DEVICE_NUMBER outBuffer, uint outBufferSize, ref uint bytesReturned, IntPtr overlapped);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetVolumeMountPoint([MarshalAs(UnmanagedType.LPWStr)] string mountPoint, [MarshalAs(UnmanagedType.LPWStr)] string volumeName);

        public static IntPtr INVALID_HANDLE_VALUE = new(-1);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct STORAGE_DEVICE_NUMBER
        {
            public int deviceType;
            public int deviceNumber;
            public int partitionNumber;
        }

        public enum IO_CONTROL_CODE : uint
        {
            GET_VOLUME_DISK_EXTENTS = 5636096,
            STORAGE_DEVICE_NUMBER = 2953344
        }

        [Flags]
        public enum GENERIC_ACCESS_RIGHTS_FLAGS : uint
        {
            GENERIC_ALL = 0x10000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_READ = 0x80000000
        }

        [Flags]
        public enum FILE_SHARE_MODE_FLAGS : int
        {
            FILE_SHARE_READ = 0x00000001,
            FILE_SHARE_WRITE = 0x00000002
        }

        [Flags]
        public enum CREATION_DISPOSITION_FLAGS : int
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXISTING = 5
        }
    }
}