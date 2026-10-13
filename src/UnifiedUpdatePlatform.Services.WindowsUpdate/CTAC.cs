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

namespace UnifiedUpdatePlatform.Services.WindowsUpdate
{
    public enum MachineType : ushort
    {
        unknown = 0x0,
        x86 = 0x14c,
        r4000 = 0x166,
        wcemipsv2 = 0x169,
        axp = 0x184,
        sh3 = 0x1a2,
        sh3dsp = 0x1a3,
        sh4 = 0x1a6,
        sh5 = 0x1a8,
        arm = 0x1c0,
        thumb = 0x1c2,
        woa = 0x1c4,
        am33 = 0x1d3,
        powerpc = 0x1f0,
        powerpcfp = 0x1f1,
        ia64 = 0x200,
        mips16 = 0x266,
        mipsfpu = 0x366,
        mipsfpu16 = 0x466,
        ebc = 0xebc,
        amd64 = 0x8664,
        m32r = 0x9041,
        arm64 = 0xaa64,
    }

    public enum OSSkuId
    {
        Undefined = 0x00000000,
        Ultimate = 0x00000001,
        HomeBasic = 0x00000002,
        HomePremium = 0x00000003,
        Enterprise = 0x00000004,
        HomeBasicN = 0x00000005,
        Business = 0x00000006,
        StandardServer = 0x00000007,
        DatacenterServer = 0x00000008,
        SmallBusinessServer = 0x00000009,
        EnterpriseServer = 0x0000000A,
        Starter = 0x0000000B,
        DatacenterServerCore = 0x0000000C,
        StandardServerCore = 0x0000000D,
        EnterpriseServerCore = 0x0000000E,
        EnterpriseServerIA64 = 0x0000000F,
        BusinessN = 0x00000010,
        WebServer = 0x00000011,
        ClusterServer = 0x00000012,
        HomeServer = 0x00000013,
        StorageExpressServer = 0x00000014,
        StorageStandardServer = 0x00000015,
        StorageWorkgroupServer = 0x00000016,
        StorageEnterpriseServer = 0x00000017,
        ServerForSmallBusiness = 0x00000018,
        SmallBusinessServerPremium = 0x00000019,
        HomePremiumN = 0x0000001A,
        EnterpriseN = 0x0000001B,
        UltimateN = 0x0000001C,
        WebServerCore = 0x0000001D,
        MediumBusinessServerManagement = 0x0000001E,
        MediumBusinessServerSecurity = 0x0000001F,
        MediumBusinessServerMessaging = 0x00000020,
        ServerFoundation = 0x00000021,
        HomePremiumServer = 0x00000022,
        ServerForSmallBusinessV = 0x00000023,
        StandardServerV = 0x00000024,
        DatacenterServerV = 0x00000025,
        EnterpriseServerV = 0x00000026,
        DatacenterServerCoreV = 0x00000027,
        StandardServerCoreV = 0x00000028,
        EnterpriseServerCoreV = 0x00000029,
        HyperV = 0x0000002A,
        StorageExpressServerCore = 0x0000002B,
        StorageServerStandardCore = 0x0000002C,
        StorageWorkgroupServerCore = 0x0000002D,
        StorageEnterpriseServerCore = 0x0000002E,
        StarterN = 0x0000002F,
        Professional = 0x00000030,
        ProfessionalN = 0x00000031,
        SBSolutionServer = 0x00000032,
        ServerForSBSolutions = 0x00000033,
        StandardServerSolutions = 0x00000034,
        StandardServerSolutionsCore = 0x00000035,
        SBSolutionServerEM = 0x00000036,
        ServerForSBSolutionsEM = 0x00000037,
        SolutionEmbeddedServer = 0x00000038,
        SolutionEmbeddedServerCore = 0x00000039,
        ProfessionalEmbedded = 0x0000003A,
        EssentialBusinessServerMGMT = 0x0000003B,
        EssentialBusinessServerADDL = 0x0000003C,
        EssentialBusinessServerMGMTSVC = 0x0000003D,
        EssentialBusinessServerADDLSVC = 0x0000003E,
        SmallBusinessServerPremiumCore = 0x0000003F,
        ClusterServerV = 0x00000040,
        Embedded = 0x00000041,
        StarterE = 0x00000042,
        HomeBasicE = 0x00000043,
        HomePremiumE = 0x00000044,
        ProfessionalE = 0x00000045,
        EnterpriseE = 0x00000046,
        UltimateE = 0x00000047,
        EnterpriseEvaluation = 0x00000048,
        Unknown49,
        Prerelease = 0x0000004A,
        Unknown4B,
        MultipointStandardServer = 0x0000004C,
        MultipointPremiumServer = 0x0000004D,
        Unknown4E = 0x0000004E,
        StandardEvaluationServer = 0x0000004F,
        DatacenterEvaluationServer = 0x00000050,
        PrereleaseARM = 0x00000051,
        PrereleaseN = 0x00000052,
        Unknown53,
        EnterpriseNEvaluation = 0x00000054,
        EmbeddedAutomotive = 0x00000055,
        EmbeddedIndustryA = 0x00000056,
        ThinPC = 0x00000057,
        EmbeddedA = 0x00000058,
        EmbeddedIndustry = 0x00000059,
        EmbeddedE = 0x0000005A,
        EmbeddedIndustryE = 0x0000005B,
        EmbeddedIndustryAE = 0x0000005C,
        Unknown5D,
        Unknown5E,
        StorageWorkgroupEvaluationServer = 0x0000005F,
        StorageStandardEvaluationServer = 0x00000060,
        CoreARM = 0x00000061,
        CoreN = 0x00000062,
        CoreCountrySpecific = 0x00000063,
        CoreSingleLanguage = 0x00000064,
        Core = 0x00000065,
        Unknown66,
        ProfessionalWMC = 0x00000067,
        MobileCore = 0x00000068,
        EmbeddedIndustryEval = 0x00000069,
        EmbeddedIndustryEEval = 0x0000006A,
        EmbeddedEval = 0x0000006B,
        EmbeddedEEval = 0x0000006C,
        NanoServer = 0x0000006D,
        CloudStorageServer = 0x0000006E,
        CoreConnected = 0x0000006F,
        ProfessionalStudent = 0x00000070,
        CoreConnectedN = 0x00000071,
        ProfessionalStudentN = 0x00000072,
        CoreConnectedSingleLanguage = 0x00000073,
        CoreConnectedCountrySpecific = 0x00000074,
        ConnectedCAR = 0x00000075,
        IndustryHandheld = 0x00000076,
        PPIPro = 0x00000077,
        ARM64Server = 0x00000078,
        Education = 0x00000079,
        EducationN = 0x0000007A,
        IoTUAP = 0x0000007B,
        CloudHostInfrastructureServer = 0x0000007C,
        EnterpriseS = 0x0000007D,
        EnterpriseSN = 0x0000007E,
        ProfessionalS = 0x0000007F,
        ProfessionalSN = 0x00000080,
        EnterpriseSEvaluation = 0x00000081,
        EnterpriseSNEvaluation = 0x00000082,
        Unknown83,
        Unknown84,
        Unknown85,
        Unknown86,
        Holographic = 0x00000087,
        HolographicBusiness = 0x00000088,
        Unknown89 = 0x00000089,
        ProSingleLanguage = 0x0000008A,
        ProChina = 0x0000008B,
        EnterpriseSubscription = 0x0000008C,
        EnterpriseSubscriptionN = 0x0000008D,
        Unknown8E,
        DatacenterNanoServer = 0x0000008F,
        StandardNanoServer = 0x00000090,
        DatacenterAServerCore = 0x00000091,
        StandardAServerCore = 0x00000092,
        DatacenterWSServerCore = 0x00000093,
        StandardWSServerCore = 0x00000094,
        UtilityVM = 0x00000095,
        Unknown96,
        Unknown97,
        Unknown98,
        Unknown99,
        Unknown9A,
        Unknown9B,
        Unknown9C,
        Unknown9D,
        Unknown9E,
        DatacenterEvaluationServerCore = 0x0000009F,
        StandardEvaluationServerCore = 0x000000A0,
        ProWorkstation = 0x000000A1,
        ProWorkstationN = 0x000000A2,
        UnknownA3,
        ProForEducation = 0x000000A4,
        ProForEducationN = 0x000000A5,
        UnknownA6,
        UnknownA7,
        AzureServerCore = 0x000000A8,
        AzureNanoServer = 0x000000A9,
        UnknownAA = 0x000000AA,
        EnterpriseG = 0x000000AB,
        EnterpriseGN = 0x000000AC,
        UnknownAD,
        UnknownAE,
        ServerRDSH = 0x000000AF,
        UnknownB0,
        UnknownB1,
        Cloud = 0x000000B2,
        CloudN = 0x000000B3,
        HubOS = 0x000000B4,
        UnknownB5,
        OneCoreUpdateOS = 0x000000B6,
        CloudE = 0x000000B7,
        Andromeda = 0x000000B8,
        IoTOS = 0x000000B9,
        CloudEN = 0x000000BA,
        IoTEdgeOS = 0x000000BB,
        IoTEnterprise = 0x000000BC,
        Lite = 0x000000BD,
        UnknownBE,
        IoTEnterpriseS = 0x000000BF,
        XboxSystemOS = 0x000000C0,
        XboxNativeOS = 0x000000C1,
        XboxGameOS = 0x000000C2,
        XboxERAOS = 0x000000C3,
        XboxDurangoHostOS = 0x000000C4,
        XboxScarlettHostOS = 0x000000C5,
        XboxKeystone = 0x000000C6,
        WNC = 0x000000D2,
        AzureStackHCIServerCore = 0x00000196,
        DatacenterServerAzureEdition = 0x00000197,
        DatacenterServerCoreAzureEdition = 0x00000198
    }

    public class CTAC
    {
        public string DeviceAttributes
        {
            get; set;
        }
        public string CallerAttributes
        {
            get; set;
        }
        public string Products
        {
            get; set;
        }
        public bool SyncCurrentVersionOnly
        {
            get; set;
        }

        public CTAC()
        {
        }

        public CTAC(OSSkuId ReportingSku,
                    string ReportingVersion,
                    MachineType MachineType,
                    string FlightRing,
                    string FlightingBranchName,
                    string BranchReadinessLevel,
                    string CurrentBranch,
                    string ReleaseType,
                    bool SyncCurrentVersionOnly,
                    bool IsStore = false,
                    string ContentType = "Mainline",
                    bool IsVbsEnabled = true,
                    bool IsEsu = false) : base()
        {
            BuildCTAC(ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, IsStore, ContentType, IsVbsEnabled, IsEsu);
        }

        private void BuildCTAC(
            OSSkuId ReportingSku,
            string ReportingVersion,
            MachineType MachineType,
            string FlightRing,
            string FlightingBranchName,
            string BranchReadinessLevel,
            string CurrentBranch,
            string ReleaseType,
            bool SyncCurrentVersionOnly,
            bool IsStore = false,
            string content = "Mainline",
            bool IsVbsEnabled = true,
            bool IsEsu = false
        )
        {
            int flightEnabled = FlightRing == "Retail" ? 0 : 1;
            string App = IsStore ? "WU_STORE" : "WU_OS";

            string InstallType = "Client";
            string ReportingPFN = "Client.OS.rs2";
            string DeviceFamily = "Windows.Desktop";

            if (ReportingSku == OSSkuId.Holographic)
            {
                InstallType = "FactoryOS";
                ReportingPFN = "HOLOLENS.OS.rs2";
                DeviceFamily = "Windows.Holographic";
            }
            else if (ReportingSku == OSSkuId.IoTUAP)
            {
                InstallType = "IoTUAP";
                ReportingPFN = "IoTCore.OS.rs2";
                DeviceFamily = "Windows.IoTUAP";
            }
            else if (ReportingSku == OSSkuId.MobileCore)
            {
                InstallType = "MobileCore";
                ReportingPFN = "Mobile.OS.rs2";
                DeviceFamily = "Windows.Mobile";
            }
            else if (ReportingSku == OSSkuId.Lite)
            {
                InstallType = "FactoryOS";
                ReportingPFN = "WCOSDevice0.OS";
                DeviceFamily = "Windows.Core";
            }
            else if (ReportingSku == OSSkuId.WNC)
            {
                InstallType = "Client";
                ReportingPFN = "WNC.OS";
                DeviceFamily = "Windows.Desktop";
            }
            else if (ReportingSku == OSSkuId.Andromeda)
            {
                InstallType = "FactoryOS";
                ReportingPFN = "WCOSDevice1.OS";
                DeviceFamily = "Windows.Core";
            }
            else if (ReportingSku == OSSkuId.HubOS)
            {
                InstallType = "FactoryOS";
                ReportingPFN = "WCOSDevice2.OS";
                DeviceFamily = "Windows.Core";
            }
            else if (ReportingSku.ToString().Contains("Server", StringComparison.InvariantCultureIgnoreCase) && ReportingSku.ToString().Contains("Core", StringComparison.InvariantCultureIgnoreCase))
            {
                InstallType = "Server Core";
                ReportingPFN = "Server.OS";
                DeviceFamily = "Windows.Server";
            }
            else if (ReportingSku.ToString().Contains("Server", StringComparison.InvariantCultureIgnoreCase) && !ReportingSku.ToString().Contains("Core", StringComparison.InvariantCultureIgnoreCase))
            {
                InstallType = "Server";
                ReportingPFN = "Server.OS";
                DeviceFamily = "Windows.Server";
            }
            else if (ReportingSku == OSSkuId.PPIPro)
            {
                DeviceFamily = "Windows.Team";
            }

            DeviceAttributes = "E:IsContainerMgrInstalled=1&" +
                                    $"FlightRing={FlightRing}&" +
                                    "TelemetryLevel=3&" +
                                    "HidOverGattReg=C:\\WINDOWS\\System32\\DriverStore\\FileRepository\\hidbthle.inf_amd64_0fc6b7cd4ccbc55c\\Microsoft.Bluetooth.Profiles.HidOverGatt.dll&" +
                                    "AppVer=0.0.0.0&" +
                                    "IsAutopilotRegistered=0&" +
                                    "ProcessorIdentifier=Intel64 Family 6 Model 151 Stepping 2&" +
                                    "OEMModel=RM-1085_1045&" +
                                    "ProcessorManufacturer=GenuineIntel&" +
                                    "InstallDate=1577722757&" +
                                    "OEMModelBaseBoard=OEM Board Name&" +
                                    $"BranchReadinessLevel={BranchReadinessLevel}&" +
                                    "DataExpDateEpoch_20H1=1593425114&" +
                                    "IsCloudDomainJoined=0&" +
                                    "Bios=2019&" +
                                    "DchuAmdGrfxVen=4098&" +
                                    "IsDeviceRetailDemo=0&" +
                                    $"FlightingBranchName={FlightingBranchName}&" +
                                    "OSUILocale=en-US&" +
                                    $"DeviceFamily={DeviceFamily}&" +
                                    "UpgEx_20H1=Green&" +
                                    $"WuClientVer={ReportingVersion}&" +
                                    $"IsFlightingEnabled={flightEnabled}&" +
                                    $"OSSkuId={(int)ReportingSku}&" +
                                    "GStatus_20H1=2&" +
                                    $"App={App}&" +
                                    $"CurrentBranch={CurrentBranch}&" +
                                    "InstallLanguage=en-US&" +
                                    "OEMName_Uncleaned=MICROSOFTMDG&" +
                                    $"InstallationType={InstallType}&" +
                                    "AttrDataVer=264&" +
                                    "IsEdgeWithChromiumInstalled=1&" +
                                    "TimestampEpochString_20H1=1593425114&" +
                                    $"OSVersion={ReportingVersion}&" +
                                    "IsMDMEnrolled=0&" +
                                    "TencentType=1&" +
                                    $"FlightContent={content}&" +
                                    "ActivationChannel=Retail&" +
                                    "Steam=URL:steam protocol&" +
                                    "Free=gt64&" +
                                    "TencentReg=79 d0 01 d7 9f 54 d5 01&" +
                                    "FirmwareVersion=7704&" +
                                    "DchuAmdGrfxExists=1&" +
                                    "SdbVer_20H1=2000000000&" +
                                    "UpgEx_CO21H2=Green&" +
                                    //$"OSArchitecture={MachineType.ToString().ToUpper()}&" +
                                    $"OSArchitecture=AMD64&" +
                                    "DefaultUserRegion=244&" +
                                    $"ReleaseType={ReleaseType}&" +
                                    "UpdateManagementGroup=2&" +
                                    "MobileOperatorCommercialized=000-88&" +
                                    "PhoneTargetingName=Lumia 950 XL&" +
                                    "AllowInPlaceUpgrade=1&" +
                                    "AllowUpgradesWithUnsupportedTPMOrCPU=1&" +
                                    "CloudPBR=1&" +
                                    "DataExpDateEpoch_19H1=1593425114&" +
                                    "DataExpDateEpoch_21H1=1593425114&" +
                                    "DataExpDateEpoch_21H2=1593425114&" +
                                    "DataExpDateEpoch_CO21H2=1593425114&" +
                                    "DataExpDateEpoch_CO21H2Setup=1593425114&" +
                                    "DataVer_RS5=2000000000&" +
                                    "DUScan=1&" +
                                    "EKB19H2InstallCount=1&" +
                                    "EKB19H2InstallTimeEpoch=1255000000&" +
                                    "GenTelRunTimestamp_19H1=1593425114&" +
                                    "GStatus_19H1=2&" +
                                    "GStatus_19H1Setup=2&" +
                                    "GStatus_20H1Setup=2&" +
                                    "GStatus_21H2=2&" +
                                    "GStatus_CO21H2=2&" +
                                    "GStatus_CO21H2Setup=2&" +
                                    "GStatus_RS5=2&" +
                                    "MediaBranch=&" +
                                    "ProcessorModel=12th Gen Intel(R) Core(TM) i9-12900K&" +
                                    "SdbVer_19H1=2000000000&" +
                                    "SecureBootCapable=1&" +
                                    "TimestampEpochString_19H1=1593425114&" +
                                    "TimestampEpochString_21H1=1593425114&" +
                                    "TimestampEpochString_21H2=1593425114&" +
                                    "TimestampEpochString_CO21H2=1593425114&" +
                                    "TimestampEpochString_CO21H2Setup=1593425114&" +
                                    "TPMVersion=2&" +
                                    "UpdateOfferedDays=0&" +
                                    "UpgEx_19H1=Green&" +
                                    "UpgEx_21H1=Green&" +
                                    "UpgEx_21H2=Green&" +
                                    "UpgEx_NI22H2=Green&" +
                                    "UpgEx_RS5=Green&" +
                                    "UpgradeEligible=1&" +
                                    "Version_RS5=2000000000&" +
                                    $"IsRetailOS={FlightRing == "Retail"}&" +
                                    $"MediaVersion={ReportingVersion}" +
                                    $"IsVbsEnabled={(IsVbsEnabled ? 1 : 0)}";

            if (IsEsu)
            {
                // Windows 10 Extended Security Updates (ESU): once Windows 10 22H2
                // reached end of support (2025-10-14) the monthly cumulative updates
                // are only offered to devices that report ESU enrollment. Reporting
                // these attributes makes the request look like an enrolled
                // Windows 10 22H2 (vb_release) device, so the servers hand out the
                // ESU builds (for example 10.0.19045.7727).
                DeviceAttributes += "&Win10ConsumerESUStatus=3&" +
                                        "Win10ConsumerESUAY=10&" +
                                        "Win10CommercialAzureESUEligible=1&" +
                                        "Win10CommercialKeybasedESUEligible=2&" +
                                        "Win10CommercialW365ESUEligible=1";
            }

            if (ReportingSku is OSSkuId.EnterpriseS or OSSkuId.EnterpriseSN || ReportingSku.ToString().Contains("Server", StringComparison.InvariantCultureIgnoreCase))
            {
                DeviceAttributes += "&BlockFeatureUpdates=1";
            }

            if (ReportingSku == OSSkuId.Holographic)
            {
                DeviceAttributes += $"&OneCoreFwV={ReportingVersion}&" +
                                        $"OneCoreSwV={ReportingVersion}&" +
                                        "OneCoreManufacturerModelName=HoloLens&" +
                                        "OneCoreManufacturer=Microsoft Corporation&" +
                                        "OneCoreOperatorName=000-88";
            }
            else if (ReportingSku == OSSkuId.HubOS)
            {
                DeviceAttributes += $"&OneCoreFwV={ReportingVersion}&" +
                                        $"OneCoreSwV={ReportingVersion}&" +
                                        "OneCoreManufacturerModelName=Surface Hub 2X&" +
                                        "OneCoreManufacturer=Microsoft Corporation&" +
                                        "OneCoreOperatorName=000-88";
            }
            else if (ReportingSku == OSSkuId.Andromeda)
            {
                DeviceAttributes += $"&OneCoreFwV={ReportingVersion}&" +
                                        $"OneCoreSwV={ReportingVersion}&" +
                                        "OneCoreManufacturerModelName=Andromeda&" +
                                        "OneCoreManufacturer=Microsoft Corporation&" +
                                        "OneCoreOperatorName=000-88";
            }
            else if (ReportingSku == OSSkuId.Lite)
            {
                DeviceAttributes += "&Product=ModernPC";
                DeviceAttributes += $"&OneCoreFwV={ReportingVersion}&" +
                                        $"OneCoreSwV={ReportingVersion}&" +
                                        "OneCoreManufacturerModelName=Santorini&" +
                                        "OneCoreManufacturer=Microsoft Corporation&" +
                                        "OneCoreOperatorName=000-88";
            }

            CallerAttributes = "E:Interactive=1&IsSeeker=1&SheddingAware=1&";
            if (IsStore)
            {
                CallerAttributes += "Acquisition=1&Id=Acquisition%3BMicrosoft.WindowsStore_8wekyb3d8bbwe&";
            }
            else
            {
                CallerAttributes += "Id=UpdateOrchestrator&";
            }
            Products = "";
            if (!IsStore)
            {
                Products = $"PN={ReportingPFN}.{MachineType}&Branch={CurrentBranch}&PrimaryOSProduct=1&Repairable=1&V={ReportingVersion};";
            }

            this.SyncCurrentVersionOnly = SyncCurrentVersionOnly;
        }

        public static string GenerateDeviceId(string Manufacturer, string Family, string Product, string Sku)
        {
            string inputString = Manufacturer + "&" + Family + "&" + Product + "&" + Sku;
            Guid guidFromString = ComputerHardwareIDProvider.Class5GuidFromString(inputString);
            string guidString = $"{{{guidFromString}}}";
            return guidString;
        }
    }
}
