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
        /// Enables a feature from the specified package name.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="packageName">The name of the package that contains the feature.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void EnableFeatureByPackageName(DismSession session, string featureName, string packageName, bool limitAccess, bool enableAll)
        {
            EnableFeatureByPackageName(session, featureName, packageName, limitAccess, enableAll, sourcePaths: null, progressCallback: null);
        }

        /// <summary>
        /// Enables a feature from the specified package name.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="packageName">The name of the package that contains the feature.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <param name="sourcePaths">A list of source locations to check for files needed to enable the feature.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void EnableFeatureByPackageName(DismSession session, string featureName, string packageName, bool limitAccess, bool enableAll, List<string> sourcePaths)
        {
            EnableFeatureByPackageName(session, featureName, packageName, limitAccess, enableAll, sourcePaths, progressCallback: null);
        }

        /// <summary>
        /// Enables a feature from the specified package name.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="packageName">The name of the package that contains the feature.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <param name="sourcePaths">A list of source locations to check for files needed to enable the feature.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void EnableFeatureByPackageName(DismSession session, string featureName, string packageName, bool limitAccess, bool enableAll, List<string>? sourcePaths, Microsoft.Dism.DismProgressCallback? progressCallback)
        {
            EnableFeatureByPackageName(session, featureName, packageName, limitAccess, enableAll, sourcePaths, progressCallback, null);
        }

        /// <summary>
        /// Enables a feature from the specified package name.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="packageName">The name of the package that contains the feature.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <param name="sourcePaths">A list of source locations to check for files needed to enable the feature.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void EnableFeatureByPackageName(DismSession session, string featureName, string packageName, bool limitAccess, bool enableAll, List<string>? sourcePaths, Microsoft.Dism.DismProgressCallback? progressCallback, object? userData)
        {
            EnableFeature(session, featureName, packageName, DismPackageIdentifier.Name, limitAccess, enableAll, sourcePaths, progressCallback, userData);
        }

        /// <summary>
        /// Enables a feature from the specified package path.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="packagePath">The path of the package that contains the feature.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void EnableFeatureByPackagePath(DismSession session, string featureName, string packagePath, bool limitAccess, bool enableAll)
        {
            EnableFeatureByPackagePath(session, featureName, packagePath, limitAccess, enableAll, sourcePaths: null, progressCallback: null);
        }

        /// <summary>
        /// Enables a feature from the specified package path.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="packagePath">The path of the package that contains the feature.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <param name="sourcePaths">A list of source locations to check for files needed to enable the feature.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void EnableFeatureByPackagePath(DismSession session, string featureName, string packagePath, bool limitAccess, bool enableAll, List<string>? sourcePaths)
        {
            EnableFeatureByPackagePath(session, featureName, packagePath, limitAccess, enableAll, sourcePaths, progressCallback: null);
        }

        /// <summary>
        /// Enables a feature from the specified package path.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="packagePath">The path of the package that contains the feature.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <param name="sourcePaths">A list of source locations to check for files needed to enable the feature.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void EnableFeatureByPackagePath(DismSession session, string featureName, string packagePath, bool limitAccess, bool enableAll, List<string>? sourcePaths, Microsoft.Dism.DismProgressCallback? progressCallback)
        {
            EnableFeatureByPackagePath(session, featureName, packagePath, limitAccess, enableAll, sourcePaths, progressCallback, userData: null);
        }

        /// <summary>
        /// Enables a feature from the specified package path.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="packagePath">The path of the package that contains the feature.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <param name="sourcePaths">A list of source locations to check for files needed to enable the feature.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        /// <exception cref="OperationCanceledException">When the user requested the operation be canceled.</exception>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void EnableFeatureByPackagePath(DismSession session, string featureName, string packagePath, bool limitAccess, bool enableAll, List<string>? sourcePaths, Microsoft.Dism.DismProgressCallback? progressCallback, object? userData)
        {
            EnableFeature(session, featureName, packagePath, DismPackageIdentifier.Path, limitAccess, enableAll, sourcePaths, progressCallback, userData);
        }

        /// <summary>
        /// Enables a feature from the specified package path.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void EnableFeature(DismSession session, string featureName, bool limitAccess, bool enableAll)
        {
            EnableFeature(session, featureName, identifier: null, DismPackageIdentifier.None, limitAccess, enableAll, sourcePaths: null, progressCallback: null, userData: null);
        }

        /// <summary>
        /// Enables a feature from the specified package path.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <param name="sourcePaths">A list of source locations to check for files needed to enable the feature.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void EnableFeature(DismSession session, string featureName, bool limitAccess, bool enableAll, List<string>? sourcePaths)
        {
            EnableFeature(session, featureName, identifier: null, DismPackageIdentifier.None, limitAccess, enableAll, sourcePaths, progressCallback: null, userData: null);
        }

        /// <summary>
        /// Enables a feature from the specified package path.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <param name="sourcePaths">A list of source locations to check for files needed to enable the feature.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void EnableFeature(DismSession session, string featureName, bool limitAccess, bool enableAll, List<string>? sourcePaths, Microsoft.Dism.DismProgressCallback? progressCallback)
        {
            EnableFeature(session, featureName, identifier: null, DismPackageIdentifier.None, limitAccess, enableAll, sourcePaths, progressCallback, userData: null);
        }

        /// <summary>
        /// Enables a feature from the specified package path.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <param name="sourcePaths">A list of source locations to check for files needed to enable the feature.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        public static void EnableFeature(DismSession session, string featureName, bool limitAccess, bool enableAll, List<string>? sourcePaths, Microsoft.Dism.DismProgressCallback? progressCallback, object? userData)
        {
            EnableFeature(session, featureName, identifier: null, DismPackageIdentifier.None, limitAccess, enableAll, sourcePaths, progressCallback, userData);
        }

        /// <summary>
        /// Enables a feature from the specified package path.
        /// </summary>
        /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
        /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
        /// <param name="identifier">A package name or absolute path.</param>
        /// <param name="packageIdentifier">A DismPackageIdentifier value.</param>
        /// <param name="limitAccess">Specifies whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
        /// <param name="enableAll">Specifies whether to enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
        /// <param name="sourcePaths">A list of source locations to check for files needed to enable the feature.</param>
        /// <param name="progressCallback">A progress callback method to invoke when progress is made.</param>
        /// <param name="userData">Optional user data to pass to the DismProgressCallback method.</param>
        /// <exception cref="DismException">When a failure occurs.</exception>
        private static void EnableFeature(DismSession session, string featureName, string? identifier, DismPackageIdentifier packageIdentifier, bool limitAccess, bool enableAll, List<string>? sourcePaths, Microsoft.Dism.DismProgressCallback? progressCallback, object? userData)
        {
            // Get the list of source paths as an array
            string[] sourcePathsArray = sourcePaths?.ToArray() ?? [];

            // Create a DismProgress object to wrap the callback and allow cancellation
            DismProgress progress = new(progressCallback, userData);

            int hresult = NativeMethods.DismEnableFeature(
                session: session,
                featureName: featureName,
                identifier: identifier,
                packageIdentifier: identifier == null ? DismPackageIdentifier.None : packageIdentifier,
                limitAccess: limitAccess,
                sourcePaths: sourcePathsArray,
                sourcePathCount: (uint)sourcePathsArray.Length,
                enableAll: enableAll,
                cancelEvent: progress.EventHandle,
                progress: progress.DismProgressCallbackNative,
                userData: IntPtr.Zero);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Enables a feature in an image. Features are identified by a name and can optionally be tied to a package.
            /// </summary>
            /// <param name="session">A valid DISM Session. The DISM Session must be associated with an image. You can associate a session with an image by using the DismOpenSession Function.</param>
            /// <param name="featureName">The name of the feature that is being enabled. To enable more than one feature, separate each feature name with a semicolon.</param>
            /// <param name="identifier">Optional. Either an absolute path to a .cab file or the package name for the parent package of the feature to be enabled.</param>
            /// <param name="packageIdentifier">Optional. A valid DismPackageIdentifier Enumeration value. DismPackageName should be used when the Identifier parameter is pointing to a package name, and DismPackagePath should be used when Identifier points to the absolute path of a .cab file. If Identifier field is not NULL, you must specify a valid PackageIdentifier parameter. If the Identifier field is NULL, the PackageIdentifier parameter is ignored.</param>
            /// <param name="limitAccess">A Boolean value indicating whether Windows Update (WU) should be contacted as a source location for downloading files if none are found in other specified locations. Before checking WU, DISM will check for the files in the SourcePaths provided and in any locations specified in the registry by group policy. If the files required to enable the feature are still present on the computer, this flag is ignored.</param>
            /// <param name="sourcePaths">Optional. A list of source locations to check for files needed to enable the feature.</param>
            /// <param name="sourcePathCount">Optional. The number of source locations specified.</param>
            /// <param name="enableAll">Enable all dependencies of the feature. If the specified feature or any one of its dependencies cannot be enabled, none of them will be changed from their existing state.</param>
            /// <param name="cancelEvent">Optional. You can set a CancelEvent for this function in order to cancel the operation in progress when signaled by the client. If the CancelEvent is received at a stage when the operation cannot be canceled, the operation will continue and return a success code. If the CancelEvent is received and the operation is canceled, the image state is unknown. You should verify the image state before continuing or discard the changes and start again.</param>
            /// <param name="progress">Optional. A pointer to a client-defined DismProgressCallback Function.</param>
            /// <param name="userData">Optional. User defined custom data.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>If the feature is present in the foundation package, you do not have to specify any package information. If the feature is in an optional package or feature pack that has already been installed in the image, specify a package name in the Identifier parameter and specify DismPackageName as the PackageIdentifier.If the feature cannot be enabled due to the parent feature not being enabled, a special error code will be returned. You can use EnableAll to enable the parent features when you enable the specified features, or you can use the DismGetFeatureParent Function to enumerate the parent features and enable them first.
            ///
            /// If the feature to be enabled is not a component of the foundation package, you must add the parent optional package with the DismAddPackage Function before you enable the feature. Do not you specify a path to a .cab file of an optional package that has not been added to the image in the Identifier parameter. If you specify a package that has not been added, and you specify DismPackagePath as the PackageIdentifier, the function will complete successfully but the feature will not be enabled.
            ///
            /// <a href="http://msdn.microsoft.com/en-us/library/windows/desktop/hh824737.aspx" />
            /// HRESULT WINAPI DismEnableFeature (_In_ DismSession Session, _In_ PCWSTR FeatureName, _In_opt_ PCWSTR Identifier, _In_opt_ DismPackageIdentifier PackageIdentifier, _In_ BOOL LimitAccess, _In_reads_opt_(SourcePathCount) PCWSTR* SourcePaths, _In_opt_ UINT SourcePathCount, _In_opt_ HANDLE CancelEvent, _In_opt_ DISM_PROGRESS_CALLBACK Progress, _In_opt_ PVOID UserData);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int DismEnableFeature(DismSession session, string featureName, string? identifier, DismPackageIdentifier packageIdentifier, [MarshalAs(UnmanagedType.Bool)] bool limitAccess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 6)] string[] sourcePaths, uint sourcePathCount, [MarshalAs(UnmanagedType.Bool)] bool enableAll, SafeWaitHandle cancelEvent, DismProgressCallback progress, IntPtr userData);
        }
    }
}