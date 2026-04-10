using System.Diagnostics;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting;

namespace ComputerHardwareIds
{
    internal class Program
    {
        public static void Main(string[] _)
        {
            Process currentProcess = Process.GetCurrentProcess();
            ProcessModule MainModule = currentProcess.MainModule!;

            (string Manufacturer,
                   string Family,
                   string ProductName,
                   string SKUNumber,
                   string BIOSVendor,
                   string BaseboardManufacturer,
                   string BaseboardProduct,
                   ushort EnclosureType,
                   string BIOSVersion,
                   byte BIOSMajorRelease,
                   byte BIOSMinorRelease) = ComputerInformationFetcher.FetchComputerInformation();

            string HardwareId1 = ComputerHardwareID.GenerateHardwareId1(Manufacturer, Family, ProductName, SKUNumber, BIOSVendor, BIOSVersion, BIOSMajorRelease.ToString("X").ToLower(), BIOSMinorRelease.ToString("X").ToLower());
            string HardwareId2 = ComputerHardwareID.GenerateHardwareId2(Manufacturer, Family, ProductName, BIOSVendor, BIOSVersion, BIOSMajorRelease.ToString("X").ToLower(), BIOSMinorRelease.ToString("X").ToLower());
            string HardwareId3 = ComputerHardwareID.GenerateHardwareId3(Manufacturer, ProductName, BIOSVendor, BIOSVersion, BIOSMajorRelease.ToString("X").ToLower(), BIOSMinorRelease.ToString("X").ToLower());
            string HardwareId4 = ComputerHardwareID.GenerateHardwareId4(Manufacturer, Family, ProductName, SKUNumber, BaseboardManufacturer, BaseboardProduct);
            string HardwareId5 = ComputerHardwareID.GenerateHardwareId5(Manufacturer, Family, ProductName, SKUNumber);
            string HardwareId6 = ComputerHardwareID.GenerateHardwareId6(Manufacturer, Family, ProductName);
            string HardwareId7 = ComputerHardwareID.GenerateHardwareId7(Manufacturer, SKUNumber, BaseboardManufacturer, BaseboardProduct);
            string HardwareId8 = ComputerHardwareID.GenerateHardwareId8(Manufacturer, SKUNumber);
            string HardwareId9 = ComputerHardwareID.GenerateHardwareId9(Manufacturer, ProductName, BaseboardManufacturer, BaseboardProduct);
            string HardwareId10 = ComputerHardwareID.GenerateHardwareId10(Manufacturer, ProductName);
            string HardwareId11 = ComputerHardwareID.GenerateHardwareId11(Manufacturer, Family, BaseboardManufacturer, BaseboardProduct);
            string HardwareId12 = ComputerHardwareID.GenerateHardwareId12(Manufacturer, Family);
            string HardwareId13 = ComputerHardwareID.GenerateHardwareId13(Manufacturer, EnclosureType.ToString());
            string HardwareId14 = ComputerHardwareID.GenerateHardwareId14(Manufacturer, BaseboardManufacturer, BaseboardProduct);
            string HardwareId15 = ComputerHardwareID.GenerateHardwareId15(Manufacturer);

            Console.WriteLine("Using the BIOS to gather information");
            Console.WriteLine();
            Console.WriteLine("Tool Information");
            Console.WriteLine("----------------");
            Console.WriteLine($"{Path.GetFileName(MainModule.FileName)} version: {MainModule.FileVersionInfo.FileVersion}");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Computer Information");
            Console.WriteLine("--------------------");
            Console.WriteLine($"BIOS Vendor: {BIOSVendor}");
            Console.WriteLine($"BIOS Version String: {BIOSVersion}");
            Console.WriteLine($"System BIOS Major Release: {BIOSMajorRelease}");
            Console.WriteLine($"System BIOS Minor Release: {BIOSMinorRelease}");
            Console.WriteLine();
            Console.WriteLine($"System Manufacturer: {Manufacturer}");
            Console.WriteLine($"System Family: {Family}");
            Console.WriteLine($"System Product Name: {ProductName}");
            Console.WriteLine($"SKU Number: {SKUNumber}");
            Console.WriteLine();
            Console.WriteLine($"System Enclosure or Chassis Type: {EnclosureType:X2}h");
            Console.WriteLine();
            Console.WriteLine($"Baseboard Manufacturer: {BaseboardManufacturer}");
            Console.WriteLine($"Baseboard Product Name: {BaseboardProduct}");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Hardware IDs");
            Console.WriteLine("------------");
            Console.WriteLine($"{HardwareId1}    <- Manufacturer + Family + ProductName + SKUNumber + BIOS Vendor + BIOS Version + BIOS Major Release + BIOS Minor Release");
            Console.WriteLine($"{HardwareId2}    <- Manufacturer + Family + ProductName + BIOS Vendor + BIOS Version + BIOS Major Release + BIOS Minor Release");
            Console.WriteLine($"{HardwareId3}    <- Manufacturer + ProductName + BIOS Vendor + BIOS Version + BIOS Major Release + BIOS Minor Release");
            Console.WriteLine($"{HardwareId4}    <- Manufacturer + Family + ProductName + SKUNumber + Baseboard Manufacturer + Baseboard Product");
            Console.WriteLine($"{HardwareId5}    <- Manufacturer + Family + ProductName + SKUNumber");
            Console.WriteLine($"{HardwareId6}    <- Manufacturer + Family + ProductName");
            Console.WriteLine($"{HardwareId7}    <- Manufacturer + SKUNumber + Baseboard Manufacturer + Baseboard Product");
            Console.WriteLine($"{HardwareId8}    <- Manufacturer + SKUNumber");
            Console.WriteLine($"{HardwareId9}    <- Manufacturer + ProductName + Baseboard Manufacturer + Baseboard Product");
            Console.WriteLine($"{HardwareId10}    <- Manufacturer + ProductName");
            Console.WriteLine($"{HardwareId11}    <- Manufacturer + Family + Baseboard Manufacturer + Baseboard Product");
            Console.WriteLine($"{HardwareId12}    <- Manufacturer + Family");
            Console.WriteLine($"{HardwareId13}    <- Manufacturer + Enclosure Type");
            Console.WriteLine($"{HardwareId14}    <- Manufacturer + Baseboard Manufacturer + Baseboard Product");
            Console.WriteLine($"{HardwareId15}    <- Manufacturer");
        }
    }
}