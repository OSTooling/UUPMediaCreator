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
using System.Collections.Generic;
using System.Linq;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting
{
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
                    bool IsDriverCheck = false,
                    string DriverPartnerRing = "Drivers") : base()
        {
            BuildCTAC(ReportingSku, ReportingVersion, MachineType, FlightRing, FlightingBranchName, BranchReadinessLevel, CurrentBranch, ReleaseType, SyncCurrentVersionOnly, IsStore, ContentType, IsVbsEnabled, IsDriverCheck, DriverPartnerRing);
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
            bool IsDriverCheck = false,
            string DriverPartnerRing = "Drivers"
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

            Dictionary<string, string> deviceAttributeDictionary = [];

            if (IsDriverCheck)
            {
                deviceAttributeDictionary.Add("ActivationChannel", "OEM:DM");
                deviceAttributeDictionary.Add("App", App);
                deviceAttributeDictionary.Add("AppVer", ReportingVersion);
                deviceAttributeDictionary.Add("AttrDataVer", "264");
                deviceAttributeDictionary.Add("BranchReadinessLevel", BranchReadinessLevel);
                deviceAttributeDictionary.Add("CurrentBranch", CurrentBranch);
                deviceAttributeDictionary.Add("DefaultUserRegion", "244");
                deviceAttributeDictionary.Add("DeviceFamily", DeviceFamily);
                if (!string.IsNullOrEmpty(DriverPartnerRing))
                {
                    deviceAttributeDictionary.Add("DriverPartnerRing", DriverPartnerRing);
                }
                deviceAttributeDictionary.Add("FirmwareVersion", "7.31.139");
                deviceAttributeDictionary.Add("FlightContent", content);
                deviceAttributeDictionary.Add("FlightingBranchName", FlightingBranchName);
                deviceAttributeDictionary.Add("FlightRing", FlightRing);
                deviceAttributeDictionary.Add("HidparseDriversVer", ReportingVersion);
                deviceAttributeDictionary.Add("InstallationType", InstallType);
                deviceAttributeDictionary.Add("InstallDate", "1677314327");
                deviceAttributeDictionary.Add("InstallLanguage", "en-US");
                deviceAttributeDictionary.Add("IsAutopilotRegistered", "0");
                deviceAttributeDictionary.Add("IsCloudDomainJoined", "0");
                deviceAttributeDictionary.Add("IsDeviceRetailDemo", "0");
                deviceAttributeDictionary.Add("IsEdgeWithChromiumInstalled", "1");
                deviceAttributeDictionary.Add("IsFlightingEnabled", flightEnabled.ToString());
                deviceAttributeDictionary.Add("IsMDMEnrolled", "0");
                deviceAttributeDictionary.Add("IsVbsEnabled", (IsVbsEnabled ? 1 : 0).ToString());
                deviceAttributeDictionary.Add("OEMModel", "Windows Dev Kit 2023");
                deviceAttributeDictionary.Add("OEMModelBaseBoard", "Windows Dev Kit 2023");
                deviceAttributeDictionary.Add("OEMName_Uncleaned", "Microsoft Corporation");
                deviceAttributeDictionary.Add("OEMSubModel", "2043");
                deviceAttributeDictionary.Add("OSArchitecture", "ARM64");
                deviceAttributeDictionary.Add("OSSkuId", ((int)ReportingSku).ToString());
                deviceAttributeDictionary.Add("OSUILocale", "en-US");
                deviceAttributeDictionary.Add("OSVersion", ReportingVersion);
                deviceAttributeDictionary.Add("ProcessorClockSpeed", "1440");
                deviceAttributeDictionary.Add("ProcessorCores", "8");
                deviceAttributeDictionary.Add("ProcessorIdentifier", "ARMv8 (64-bit) Family 8 Model D4B Revision   0");
                deviceAttributeDictionary.Add("ProcessorManufacturer", "Qualcomm Technologies Inc");
                deviceAttributeDictionary.Add("ProcessorModel", "Snapdragon Compute Platform");
                deviceAttributeDictionary.Add("SecureBootCapable", "1");
                deviceAttributeDictionary.Add("TelemetryLevel", "3");
                deviceAttributeDictionary.Add("TotalPhysicalRAM", "32768");
                deviceAttributeDictionary.Add("TPMVersion", "2");
                deviceAttributeDictionary.Add("UpdateManagementGroup", "2");
                deviceAttributeDictionary.Add("WuClientVer", "1101.2301.31021.0");
            }
            else
            {
                deviceAttributeDictionary.Add("IsContainerMgrInstalled", "1");
                deviceAttributeDictionary.Add("FlightRing", FlightRing);
                deviceAttributeDictionary.Add("TelemetryLevel", "3");
                deviceAttributeDictionary.Add("HidOverGattReg", "C:\\WINDOWS\\System32\\DriverStore\\FileRepository\\hidbthle.inf_amd64_0fc6b7cd4ccbc55c\\Microsoft.Bluetooth.Profiles.HidOverGatt.dll");
                deviceAttributeDictionary.Add("AppVer", "0.0.0.0");
                deviceAttributeDictionary.Add("IsAutopilotRegistered", "0");
                deviceAttributeDictionary.Add("ProcessorIdentifier", "Intel64 Family 6 Model 151 Stepping 2");
                deviceAttributeDictionary.Add("OEMModel", "RM-1085_1045");
                deviceAttributeDictionary.Add("ProcessorManufacturer", "GenuineIntel");
                deviceAttributeDictionary.Add("InstallDate", "1577722757");
                deviceAttributeDictionary.Add("OEMModelBaseBoard", "OEM Board Name");
                deviceAttributeDictionary.Add("BranchReadinessLevel", BranchReadinessLevel);
                deviceAttributeDictionary.Add("DataExpDateEpoch_20H1", "1593425114");
                deviceAttributeDictionary.Add("IsCloudDomainJoined", "0");
                deviceAttributeDictionary.Add("Bios", "2019");
                deviceAttributeDictionary.Add("DchuAmdGrfxVen", "4098");
                deviceAttributeDictionary.Add("IsDeviceRetailDemo", "0");
                deviceAttributeDictionary.Add("FlightingBranchName", FlightingBranchName);
                deviceAttributeDictionary.Add("OSUILocale", "en-US");
                deviceAttributeDictionary.Add("DeviceFamily", DeviceFamily);
                deviceAttributeDictionary.Add("UpgEx_20H1", "Green");
                deviceAttributeDictionary.Add("WuClientVer", ReportingVersion);
                deviceAttributeDictionary.Add("IsFlightingEnabled", flightEnabled.ToString());
                deviceAttributeDictionary.Add("OSSkuId", ((int)ReportingSku).ToString());
                deviceAttributeDictionary.Add("GStatus_20H1", "2");
                deviceAttributeDictionary.Add("App", App);
                deviceAttributeDictionary.Add("CurrentBranch", CurrentBranch);
                deviceAttributeDictionary.Add("InstallLanguage", "en-US");
                deviceAttributeDictionary.Add("OEMName_Uncleaned", "MICROSOFTMDG");
                deviceAttributeDictionary.Add("InstallationType", InstallType);
                deviceAttributeDictionary.Add("AttrDataVer", "264");
                deviceAttributeDictionary.Add("IsEdgeWithChromiumInstalled", "1");
                deviceAttributeDictionary.Add("TimestampEpochString_20H1", "1593425114");
                deviceAttributeDictionary.Add("OSVersion", ReportingVersion);
                deviceAttributeDictionary.Add("IsMDMEnrolled", "0");
                deviceAttributeDictionary.Add("TencentType", "1");
                deviceAttributeDictionary.Add("FlightContent", content);
                deviceAttributeDictionary.Add("ActivationChannel", "Retail");
                deviceAttributeDictionary.Add("Steam", "URL:steam protocol");
                deviceAttributeDictionary.Add("Free", "gt64");
                deviceAttributeDictionary.Add("TencentReg", "79 d0 01 d7 9f 54 d5 01");
                deviceAttributeDictionary.Add("FirmwareVersion", "7704");
                deviceAttributeDictionary.Add("DchuAmdGrfxExists", "1");
                deviceAttributeDictionary.Add("SdbVer_20H1", "2000000000");
                deviceAttributeDictionary.Add("UpgEx_CO21H2", "Green");
                //deviceAttributeDictionary.Add("OSArchitecture", MachineType.ToString().ToUpper());
                deviceAttributeDictionary.Add("OSArchitecture", "AMD64");
                deviceAttributeDictionary.Add("DefaultUserRegion", "244");
                deviceAttributeDictionary.Add("ReleaseType", ReleaseType);
                deviceAttributeDictionary.Add("UpdateManagementGroup", "2");
                deviceAttributeDictionary.Add("MobileOperatorCommercialized", "000-88");
                deviceAttributeDictionary.Add("PhoneTargetingName", "Lumia 950 XL");
                deviceAttributeDictionary.Add("AllowInPlaceUpgrade", "1");
                deviceAttributeDictionary.Add("AllowUpgradesWithUnsupportedTPMOrCPU", "1");
                deviceAttributeDictionary.Add("CloudPBR", "1");
                deviceAttributeDictionary.Add("DataExpDateEpoch_19H1", "1593425114");
                deviceAttributeDictionary.Add("DataExpDateEpoch_21H1", "1593425114");
                deviceAttributeDictionary.Add("DataExpDateEpoch_21H2", "1593425114");
                deviceAttributeDictionary.Add("DataExpDateEpoch_CO21H2", "1593425114");
                deviceAttributeDictionary.Add("DataExpDateEpoch_CO21H2Setup", "1593425114");
                deviceAttributeDictionary.Add("DataVer_RS5", "2000000000");
                deviceAttributeDictionary.Add("DUScan", "1");
                deviceAttributeDictionary.Add("EKB19H2InstallCount", "1");
                deviceAttributeDictionary.Add("EKB19H2InstallTimeEpoch", "1255000000");
                deviceAttributeDictionary.Add("GenTelRunTimestamp_19H1", "1593425114");
                deviceAttributeDictionary.Add("GStatus_19H1", "2");
                deviceAttributeDictionary.Add("GStatus_19H1Setup", "2");
                deviceAttributeDictionary.Add("GStatus_20H1Setup", "2");
                deviceAttributeDictionary.Add("GStatus_21H2", "2");
                deviceAttributeDictionary.Add("GStatus_CO21H2", "2");
                deviceAttributeDictionary.Add("GStatus_CO21H2Setup", "2");
                deviceAttributeDictionary.Add("GStatus_RS5", "2");
                deviceAttributeDictionary.Add("MediaBranch", "");
                deviceAttributeDictionary.Add("ProcessorModel", "12th Gen Intel(R) Core(TM) i9-12900K");
                deviceAttributeDictionary.Add("SdbVer_19H1", "2000000000");
                deviceAttributeDictionary.Add("SecureBootCapable", "1");
                deviceAttributeDictionary.Add("TimestampEpochString_19H1", "1593425114");
                deviceAttributeDictionary.Add("TimestampEpochString_21H1", "1593425114");
                deviceAttributeDictionary.Add("TimestampEpochString_21H2", "1593425114");
                deviceAttributeDictionary.Add("TimestampEpochString_CO21H2", "1593425114");
                deviceAttributeDictionary.Add("TimestampEpochString_CO21H2Setup", "1593425114");
                deviceAttributeDictionary.Add("TPMVersion", "2");
                deviceAttributeDictionary.Add("UpdateOfferedDays", "0");
                deviceAttributeDictionary.Add("UpgEx_19H1", "Green");
                deviceAttributeDictionary.Add("UpgEx_21H1", "Green");
                deviceAttributeDictionary.Add("UpgEx_21H2", "Green");
                deviceAttributeDictionary.Add("UpgEx_NI22H2", "Green");
                deviceAttributeDictionary.Add("UpgEx_RS5", "Green");
                deviceAttributeDictionary.Add("UpgradeEligible", "1");
                deviceAttributeDictionary.Add("Version_RS5", "2000000000");
                deviceAttributeDictionary.Add("IsRetailOS", (FlightRing == "Retail").ToString());
                deviceAttributeDictionary.Add("MediaVersion", ReportingVersion);
                deviceAttributeDictionary.Add("IsVbsEnabled", (IsVbsEnabled ? 1 : 0).ToString());
            }

            DeviceAttributes = $"E:{string.Join("&", deviceAttributeDictionary.Select(x => $"{x.Key}={x.Value}"))}";

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
    }
}
