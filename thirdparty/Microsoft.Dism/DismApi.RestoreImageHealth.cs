// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Repairs a corrupted image that has been identified as repairable by the CheckImageHealth Function.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="limitAccess">Specifies whether the RestoreImageHealth method should contact Windows Update (WU) as a source location for downloading repair files. Before checking WU, DISM will check for the files in the sourcePaths provided and in any locations specified in the registry by Group Policy. If the files that are required to enable the feature are found in these other specified locations, this flag is ignored.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RestoreImageHealth(DismSession session, bool limitAccess)
        {
            RestoreImageHealth(session, limitAccess, sourcePaths: null);
        }

        /// <summary>
        /// Repairs a corrupted image that has been identified as repairable by the CheckImageHealth Function.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="limitAccess">Specifies whether the RestoreImageHealth method should contact Windows Update (WU) as a source location for downloading repair files. Before checking WU, DISM will check for the files in the sourcePaths provided and in any locations specified in the registry by Group Policy. If the files that are required to enable the feature are found in these other specified locations, this flag is ignored.</param>
        /// <param name="sourcePaths">List of source locations to check for repair files.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RestoreImageHealth(DismSession session, bool limitAccess, List<string>? sourcePaths)
        {
            RestoreImageHealth(session, limitAccess, sourcePaths, progressCallback: null);
        }

        /// <summary>
        /// Repairs a corrupted image that has been identified as repairable by the CheckImageHealth Function.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="limitAccess">Specifies whether the RestoreImageHealth method should contact Windows Update (WU) as a source location for downloading repair files. Before checking WU, DISM will check for the files in the sourcePaths provided and in any locations specified in the registry by Group Policy. If the files that are required to enable the feature are found in these other specified locations, this flag is ignored.</param>
        /// <param name="sourcePaths">List of source locations to check for repair files.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RestoreImageHealth(DismSession session, bool limitAccess, List<string>? sourcePaths, Dism.DismProgressCallback? progressCallback)
        {
            RestoreImageHealth(session, limitAccess, sourcePaths, progressCallback, userData: null);
        }

        /// <summary>
        /// Repairs a corrupted image that has been identified as repairable by the CheckImageHealth Function.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="limitAccess">Specifies whether the RestoreImageHealth method should contact Windows Update (WU) as a source location for downloading repair files. Before checking WU, DISM will check for the files in the sourcePaths provided and in any locations specified in the registry by Group Policy. If the files that are required to enable the feature are found in these other specified locations, this flag is ignored.</param>
        /// <param name="sourcePaths">List of source locations to check for repair files.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void RestoreImageHealth(DismSession session, bool limitAccess, List<string>? sourcePaths, Dism.DismProgressCallback? progressCallback, object? userData)
        {
            // Get the list of source paths as an array
            string[] sourcePathsArray = sourcePaths?.ToArray() ?? [];

            // Create a DismProgress object to wrap the callback and allow cancellation
            DismProgress progress = new(progressCallback, userData);

            int hresult = NativeMethods.DismRestoreImageHealth(session, sourcePathsArray, (uint)sourcePathsArray.Length, limitAccess, progress.EventHandle, progress.DismProgressCallbackNative, IntPtr.Zero);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Repairs a corrupted image that has been identified as repairable by the DismCheckImageHealth Function.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="sourcePaths">Optional. A list of source locations to check for repair files.</param>
            /// <param name="sourcePathCount">Optional. The number of source locations specified.</param>
            /// <param name="limitAccess">Optional. Indicates if the machine is allows to use remote resources.</param>
            /// <param name="cancelEvent">Optional. You can set a CancelEvent for this function in order to cancel the operation in progress when signaled by the client. If the CancelEvent is received at a stage when the operation cannot be canceled, the operation will continue and return a success code. If the CancelEvent is received and the operation is canceled, the image state is unknown. You should verify the image state before continuing or discard the changes and start again.</param>
            /// <param name="progress">Optional. A pointer to a client-defined DismProgressCallback Function.</param>
            /// <param name="userData">Optional. User defined custom data.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>Run the DismCheckImageHealth Function to determine if the image is corrupted and if the image is repairable. If the DismCheckImageHealth Function returns DismImageRepairable, the DismRestoreImageHealth function can repair the image.
            ///
            /// If a repair file is not found in any of the locations specified by the SourcePaths parameter or the location paths in the registry specified by Group Policy, the DismRestoreImageHealth function will contact WU to check for a repair file unless the LimitAccess parameter is set to True.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh825836.aspx" />
            /// HRESULT WINAPI DismRestoreImageHealth(_In_ DismSession Session, _In_reads_opt_(SourcePathCount) PCWSTR* SourcePaths, _In_opt_ UINT SourcePathCount, _In_ BOOL LimitAccess, _In_opt_ DISM_PROGRESS_CALLBACK Progress, _In_opt_ PVOID UserData);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismRestoreImageHealth(DismSession session, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 2)] string[] sourcePaths, uint sourcePathCount, [MarshalAs(UnmanagedType.Bool)] bool limitAccess, SafeWaitHandle cancelEvent, DismProgressCallback progress, IntPtr userData);
        }
    }
}