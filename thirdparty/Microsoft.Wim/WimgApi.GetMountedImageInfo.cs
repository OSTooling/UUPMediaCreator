// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

using DWORD = System.UInt32;

namespace Microsoft.Wim
{
    /// <summary>
    /// The mounted image info level.
    /// </summary>
    internal enum WimMountedImageInfoLevels : uint
    {
        /// <summary>
        /// Level zero
        /// </summary>
        Level0,

        /// <summary>
        /// Level one
        /// </summary>
        Level1,

        /// <summary>
        /// Invalid
        /// </summary>
        Invalid,
    }

    public static partial class WimgApi
    {
        /// <summary>
        /// Gets a <see cref="WimMountInfoCollection"/> containing <see cref="WimMountInfo"/> objects that represent a list of images that are currently mounted.
        /// </summary>
        /// <returns>A <see cref="WimMountInfoCollection"/> containing <see cref="WimMountInfo"/> objects.</returns>
        /// <exception cref="Win32Exception">The Windows® Imaging API reported a failure.</exception>
        public static WimMountInfoCollection GetMountedImageInfo()
        {
            // Call the native function first to get the necessary buffer size
            _ = WimgApi.NativeMethods.WIMGetMountedImageInfo(WimMountInfo.MountInfoLevel, out _, IntPtr.Zero, 0, out DWORD returnLength);

            switch (Marshal.GetLastWin32Error())
            {
                case 0:

                    // Return an empty list because there are no images
                    return new WimMountInfoCollection([]);

                case WimgApi.ERROR_INSUFFICIENT_BUFFER:

                    // Continue on because we now know how much memory is needed
                    break;

                default:

                    // Throw a Win32Exception based on the last error code
                    throw new Win32Exception();
            }

            // Create a collection of WimMountInfo objects
            List<WimMountInfo> wimMountInfos = [];

            // Allocate enough memory for the return array
            IntPtr mountInfoPtr = Marshal.AllocHGlobal((int)returnLength);

            try
            {
                // Call the native function a second time so it can fill the array of pointers
                if (!WimgApi.NativeMethods.WIMGetMountedImageInfo(WimMountInfo.MountInfoLevel, out uint imageCount, mountInfoPtr, returnLength, out returnLength))
                {
                    // Throw a Win32Exception based on the last error code
                    throw new Win32Exception();
                }

                // Loop through each image
                for (int i = 0; i < imageCount; i++)
                {
                    // Get the current pointer based on the index
                    IntPtr currentImageInfoPtr = new(mountInfoPtr.ToInt64() + (i * (returnLength / imageCount)));

                    // Read a pointer and add a new WimMountInfo object to the collection
                    wimMountInfos.Add(new WimMountInfo(currentImageInfoPtr));
                }
            }
            finally
            {
                // Free the native array
                Marshal.FreeHGlobal(mountInfoPtr);
            }

            // Return the WimMountInfo list as a read-only collection
            return new WimMountInfoCollection(wimMountInfos);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Returns a list of images that are currently mounted.
            /// </summary>
            /// <param name="fInfoLevelId">A class of attribute information to retrieve.</param>
            /// <param name="pdwImageCount">A pointer to a DWORD that receives the number of mounted images.</param>
            /// <param name="pMountInfo">
            /// Pointer to a variable that receives the array of mounted image structures. The size of the
            /// information written varies depending on the type of structured defined by the fInfoLevelId parameter.
            /// </param>
            /// <param name="cbMountInfoLength">The size of the buffer pointed to by the pMountInfo parameter, in bytes.</param>
            /// <param name="pcbReturnLength">
            /// A pointer to a variable in which the function returns the size of the requested
            /// information. If the function was successful, this is the size of the information written to the buffer pointed to by
            /// the pMountInfo parameter, but if the buffer was too small; this is the minimum size of buffer needed to receive the
            /// information successfully.
            /// </param>
            /// <returns>
            /// If the function succeeds, then the return value is nonzero. If the function fails, then the return value is
            /// zero. To obtain extended error information, call the GetLastError function. If the buffer specified by the
            /// cbMountInfoLength parameter is not large enough to hold the data, the function set LastError to
            /// ERROR_INSUFFICIENT_BUFFER and stores the required buffer size in the variable pointed to by pcbReturnLength.
            /// </returns>
            [DllImport(WimgApiDllName, CallingConvention = WimgApiCallingConvention, CharSet = WimgApiCharSet, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool WIMGetMountedImageInfo(WimMountedImageInfoLevels fInfoLevelId, out DWORD pdwImageCount, [Out][Optional] IntPtr pMountInfo, DWORD cbMountInfoLength, out DWORD pcbReturnLength);
        }
    }
}