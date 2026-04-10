// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Microsoft.Dism
{
    public static partial class DismApi
    {
        /// <summary>
        /// Adds an app package (.appx) that will install for each new user to a Windows image.
        /// </summary>
        /// <param name="session">A valid DISM Session.</param>
        /// <param name="appPath">Specifies the location of the app package (.appx) to add to the Windows image.</param>
        /// <param name="dependencyPackages">Specifies the location of dependency packages.</param>
        /// <param name="licensePath">Specifies the location of the .xml file containing your application license.</param>
        /// <param name="customDataPath">Specifies the location of a custom data file. The custom data file will be renamed custom.data and saved in the app data store.</param>
        /// <param name="stubPackageOption">Specifies the stub package option.</param>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void AddProvisionedAppxPackage(DismSession session, string appPath, List<string>? dependencyPackages, string? licensePath, string customDataPath, DismStubPackageOption stubPackageOption)
        {
            AddProvisionedAppxPackage(
                session,
                appPath,
                dependencyPackages,
                optionalPackages: null,
                licensePaths: string.IsNullOrEmpty(licensePath) ? null : [licensePath!],
                customDataPath: customDataPath,
                regions: "all",
                stubPackageOption: stubPackageOption);
        }

        /// <summary>
        /// Adds an app package (.appx) that will install for each new user to a Windows image.
        /// </summary>
        /// <param name="session">A valid DISM Session.</param>
        /// <param name="appPath">Specifies the location of the app package (.appx) to add to the Windows image.</param>
        /// <param name="dependencyPackages">Specifies the location of dependency packages.</param>
        /// <param name="licensePath">Specifies the location of the .xml file containing your application license.</param>
        /// <param name="customDataPath">Specifies the location of a custom data file. The custom data file will be renamed custom.data and saved in the app data store.</param>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void AddProvisionedAppxPackage(DismSession session, string appPath, List<string>? dependencyPackages, string? licensePath, string customDataPath)
        {
            AddProvisionedAppxPackage(
                session,
                appPath,
                dependencyPackages,
                optionalPackages: null,
                licensePaths: string.IsNullOrEmpty(licensePath) ? null : [licensePath!],
                customDataPath: customDataPath,
                regions: null);
        }

        /// <summary>
        /// Adds an app package (.appx) that will install for each new user to a Windows image.
        /// </summary>
        /// <param name="session">A valid DISM Session.</param>
        /// <param name="appPath">Specifies the location of the app package (.appx) to add to the Windows image.</param>
        /// <param name="dependencyPackages">Specifies the location of dependency packages.</param>
        /// <param name="optionalPackages">Specifies the location of optional packages.</param>
        /// <param name="licensePaths">Specifies the locations of .xml files containing your application licenses.</param>
        /// <param name="customDataPath">Specifies the location of a custom data file. The custom data file will be renamed custom.data and saved in the app data store.</param>
        /// <param name="regions">Specifies regions for the package.</param>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void AddProvisionedAppxPackage(DismSession session, string appPath, List<string>? dependencyPackages, List<string>? optionalPackages, List<string>? licensePaths, string customDataPath, string? regions)
        {
            AddProvisionedAppxPackage(
                session,
                appPath,
                dependencyPackages,
                optionalPackages,
                licensePaths,
                customDataPath,
                regions,
                DismStubPackageOption.None);
        }

        /// <summary>
        /// Adds an app package (.appx) that will install for each new user to a Windows image.
        /// </summary>
        /// <param name="session">A valid DISM Session.</param>
        /// <param name="appPath">Specifies the location of the app package (.appx) to add to the Windows image.</param>
        /// <param name="dependencyPackages">Specifies the location of dependency packages.</param>
        /// <param name="optionalPackages">Specifies the location of optional packages.</param>
        /// <param name="licensePaths">Specifies the locations of .xml files containing your application licenses.</param>
        /// <param name="customDataPath">Specifies the location of a custom data file. The custom data file will be renamed custom.data and saved in the app data store.</param>
        /// <param name="regions">Specifies regions for the package.</param>
        /// <param name="stubPackageOption">Specifies the stub package option.</param>
        /// <exception cref="DismRebootRequiredException">When the operation requires a reboot to complete.</exception>
        public static void AddProvisionedAppxPackage(DismSession session, string appPath, List<string>? dependencyPackages, List<string>? optionalPackages, List<string>? licensePaths, string customDataPath, string? regions, DismStubPackageOption stubPackageOption)
        {
            int hresult = NativeMethods._DismAddProvisionedAppxPackage(
                session,
                appPath,
                dependencyPackages?.ToArray() ?? [],
                (uint)(dependencyPackages?.Count ?? 0),
                optionalPackages?.ToArray() ?? [],
                (uint)(optionalPackages?.Count ?? 0),
                licensePaths?.ToArray() ?? [],
                (uint)(licensePaths?.Count ?? 0),
                SkipLicense: licensePaths == null || licensePaths.Count == 0,
                customDataPath,
                regions,
                stubPackageOption);

            DismUtilities.ThrowIfFail(hresult, session);
        }

        internal static partial class NativeMethods
        {
            /// <summary>
            /// Adds a provisioned appx package.
            /// </summary>
            /// <param name="Session">A valid DISM Session.</param>
            /// <param name="AppPath">The application path.</param>
            /// <param name="DependencyPackages">The dependent packages.</param>
            /// <param name="DependencyPackageCount">The dependent package count.</param>
            /// <param name="OptionalPackages">The optional packages.</param>
            /// <param name="OptionalPackageCount">The optional package count.</param>
            /// <param name="LicensePaths">The license paths.</param>
            /// <param name="LicensePathCount">The license path count.</param>
            /// <param name="SkipLicense">Specifies whether the license should be skipped.</param>
            /// <param name="CustomDataPath">A custom path.</param>
            /// <param name="Regions">The regions.</param>
            /// <param name="stubPackageOption">The stub package option.</param>
            /// <returns>Returns S_OK on success.</returns>
            /// <remarks>
            ///   HRESULT WINAPI
            /// _DismAddProvisionedAppxPackage(
            ///   _In_ DismSession Session,
            ///   _In_ PCWSTR AppPath,
            ///   _In_reads_opt_(DependencyPackageCount) PCWSTR* DependencyPackages,
            ///   _In_ UINT DependencyPackageCount,
            ///   _In_reads_opt_(OptionalPackageCount) PCWSTR* OptionalPackages,
            ///   _In_ UINT OptionalPackageCount,
            ///   _In_reads_opt_(LicensePathCount) PCWSTR* LicensePaths,
            ///   _In_ UINT LicensePathCount,
            ///   _In_ BOOL SkipLicense,
            ///   _In_opt_ PCWSTR CustomDataPath,
            ///   _In_opt_ PCWSTR Region,
            ///   _In_ DismStubPackageOption stubPackageOption);
            /// </remarks>
            [DllImport(DismDllName, CharSet = DismCharacterSet)]
            [return: MarshalAs(UnmanagedType.Error)]
            public static extern int _DismAddProvisionedAppxPackage(
                DismSession Session,
                [MarshalAs(UnmanagedType.LPWStr)]
                string AppPath,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 3)]
                string[] DependencyPackages,
                uint DependencyPackageCount,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 5)]
                string[] OptionalPackages,
                uint OptionalPackageCount,
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr, SizeParamIndex = 7)]
                string[] LicensePaths,
                uint LicensePathCount,
                bool SkipLicense,
                [MarshalAs(UnmanagedType.LPWStr)]
                string CustomDataPath,
                [MarshalAs(UnmanagedType.LPWStr)]
                string? Regions,
                DismStubPackageOption stubPackageOption);
        }
    }
}