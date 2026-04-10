using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Options;
using System.Linq;

namespace UUPDownload.DownloadRequest.DriversAuto
{
    public static class ComputerInformationFetcher
    {
        public static (string Manufacturer,
                    string Family,
                    string ProductName,
                    string SKUNumber,
                    string BIOSVendor,
                    string BaseboardManufacturer,
                    string BaseboardProduct,
                    ushort EnclosureType,
                    string BIOSVersion,
                    byte BIOSMajorRelease,
                    byte BIOSMinorRelease) FetchComputerInformation()
        {
            using DComSessionOptions dcomSessionOptions = new();
            using CimSession cimSession = CimSession.Create("localhost", dcomSessionOptions);

            CimInstance result = cimSession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_BIOS").Single();

            string BIOSVendor = (string)result.CimInstanceProperties["Manufacturer"].Value;
            string BIOSVersionString = (string)result.CimInstanceProperties["SMBIOSBIOSVersion"].Value;
            byte SystemBIOSMajorRelease = (byte)result.CimInstanceProperties["SystemBiosMajorVersion"].Value;
            byte SystemBIOSMinorRelease = (byte)result.CimInstanceProperties["SystemBiosMinorVersion"].Value;

            result = cimSession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_ComputerSystem").Single();

            string SystemManufacturer = (string)result.CimInstanceProperties["Manufacturer"].Value;
            string SystemFamily = (string)result.CimInstanceProperties["SystemFamily"].Value;
            string SystemProductName = (string)result.CimInstanceProperties["Model"].Value;
            string SKUNumber = (string)result.CimInstanceProperties["SystemSKUNumber"].Value;

            result = cimSession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_SystemEnclosure").Single();

            ushort SystemEnclosureorChassisType = ((ushort[])result.CimInstanceProperties["ChassisTypes"].Value)[0];

            result = cimSession.QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM Win32_BaseBoard").Single();

            string BaseboardManufacturer = (string)result.CimInstanceProperties["Manufacturer"].Value;
            string BaseboardProductName = (string)result.CimInstanceProperties["Product"].Value;

            return (SystemManufacturer,
                    SystemFamily,
                    SystemProductName,
                    SKUNumber,
                    BIOSVendor,
                    BaseboardManufacturer,
                    BaseboardProductName,
                    SystemEnclosureorChassisType,
                    BIOSVersionString,
                    SystemBIOSMajorRelease,
                    SystemBIOSMinorRelease);
        }
    }
}
