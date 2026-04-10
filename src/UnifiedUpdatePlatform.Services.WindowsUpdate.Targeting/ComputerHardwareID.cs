using System;
using System.Collections.Generic;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting
{
    public class ComputerHardwareID
    {
        public static string GenerateHardwareId1(string Manufacturer, string Family, string ProductName, string SKUNumber, string BIOSVendor, string BIOSVersion, string BIOSMajorRelease, string BIOSMinorRelease)
        {
            return GenerateHardwareId([Manufacturer, Family, ProductName, SKUNumber, BIOSVendor, BIOSVersion, BIOSMajorRelease, BIOSMinorRelease]);
        }

        public static string GenerateHardwareId2(string Manufacturer, string Family, string ProductName, string BIOSVendor, string BIOSVersion, string BIOSMajorRelease, string BIOSMinorRelease)
        {
            return GenerateHardwareId([Manufacturer, Family, ProductName, BIOSVendor, BIOSVersion, BIOSMajorRelease, BIOSMinorRelease]);
        }

        public static string GenerateHardwareId3(string Manufacturer, string ProductName, string BIOSVendor, string BIOSVersion, string BIOSMajorRelease, string BIOSMinorRelease)
        {
            return GenerateHardwareId([Manufacturer, ProductName, BIOSVendor, BIOSVersion, BIOSMajorRelease, BIOSMinorRelease]);
        }

        public static string GenerateHardwareId4(string Manufacturer, string Family, string ProductName, string SKUNumber, string BaseboardManufacturer, string BaseboardProduct)
        {
            return GenerateHardwareId([Manufacturer, Family, ProductName, SKUNumber, BaseboardManufacturer, BaseboardProduct]);
        }

        public static string GenerateHardwareId5(string Manufacturer, string Family, string ProductName, string SKUNumber)
        {
            return GenerateHardwareId([Manufacturer, Family, ProductName, SKUNumber]);
        }

        public static string GenerateHardwareId6(string Manufacturer, string Family, string ProductName)
        {
            return GenerateHardwareId([Manufacturer, Family, ProductName]);
        }

        public static string GenerateHardwareId7(string Manufacturer, string SKUNumber, string BaseboardManufacturer, string BaseboardProduct)
        {
            return GenerateHardwareId([Manufacturer, SKUNumber, BaseboardManufacturer, BaseboardProduct]);
        }

        public static string GenerateHardwareId8(string Manufacturer, string SKUNumber)
        {
            return GenerateHardwareId([Manufacturer, SKUNumber]);
        }

        public static string GenerateHardwareId9(string Manufacturer, string ProductName, string BaseboardManufacturer, string BaseboardProduct)
        {
            return GenerateHardwareId([Manufacturer, ProductName, BaseboardManufacturer, BaseboardProduct]);
        }

        public static string GenerateHardwareId10(string Manufacturer, string ProductName)
        {
            return GenerateHardwareId([Manufacturer, ProductName]);
        }

        public static string GenerateHardwareId11(string Manufacturer, string Family, string BaseboardManufacturer, string BaseboardProduct)
        {
            return GenerateHardwareId([Manufacturer, Family, BaseboardManufacturer, BaseboardProduct]);
        }

        public static string GenerateHardwareId12(string Manufacturer, string Family)
        {
            return GenerateHardwareId([Manufacturer, Family]);
        }

        public static string GenerateHardwareId13(string Manufacturer, string EnclosureType)
        {
            return GenerateHardwareId([Manufacturer, EnclosureType]);
        }

        public static string GenerateHardwareId14(string Manufacturer, string BaseboardManufacturer, string BaseboardProduct)
        {
            return GenerateHardwareId([Manufacturer, BaseboardManufacturer, BaseboardProduct]);
        }

        public static string GenerateHardwareId15(string Manufacturer)
        {
            return GenerateHardwareId([Manufacturer]);
        }

        public static string[] GenerateHardwareIds(string Manufacturer, string Family = null, string ProductName = null, string SKUNumber = null, string BIOSVendor = null, string BaseboardManufacturer = null, string BaseboardProduct = null, string EnclosureType = null, string BIOSVersion = null, string BIOSMajorRelease = null, string BIOSMinorRelease = null)
        {
            List<string> HardwareIds = [];

            if (Manufacturer == null)
            {
                throw new Exception("At least a non null Manufacturer value is required");
            }

            if (Family != null && ProductName != null && SKUNumber != null && BIOSVendor != null && BIOSVersion != null && BIOSMajorRelease != null && BIOSMinorRelease != null)
            {
                HardwareIds.Add(GenerateHardwareId1(Manufacturer, Family, ProductName, SKUNumber, BIOSVendor, BIOSVersion, BIOSMajorRelease, BIOSMinorRelease));
            }

            if (Family != null && ProductName != null && BIOSVendor != null && BIOSVersion != null && BIOSMajorRelease != null && BIOSMinorRelease != null)
            {
                HardwareIds.Add(GenerateHardwareId2(Manufacturer, Family, ProductName, BIOSVendor, BIOSVersion, BIOSMajorRelease, BIOSMinorRelease));
            }

            if (ProductName != null && BIOSVendor != null && BIOSVersion != null && BIOSMajorRelease != null && BIOSMinorRelease != null)
            {
                HardwareIds.Add(GenerateHardwareId3(Manufacturer, ProductName, BIOSVendor, BIOSVersion, BIOSMajorRelease, BIOSMinorRelease));
            }

            if (Family != null && ProductName != null && SKUNumber != null && BaseboardManufacturer != null && BaseboardProduct != null)
            {
                HardwareIds.Add(GenerateHardwareId4(Manufacturer, Family, ProductName, SKUNumber, BaseboardManufacturer, BaseboardProduct));
            }

            if (Family != null && ProductName != null && SKUNumber != null)
            {
                HardwareIds.Add(GenerateHardwareId5(Manufacturer, Family, ProductName, SKUNumber));
            }

            if (Family != null && ProductName != null)
            {
                HardwareIds.Add(GenerateHardwareId6(Manufacturer, Family, ProductName));
            }

            if (SKUNumber != null && BaseboardManufacturer != null && BaseboardProduct != null)
            {
                HardwareIds.Add(GenerateHardwareId7(Manufacturer, SKUNumber, BaseboardManufacturer, BaseboardProduct));
            }

            if (SKUNumber != null)
            {
                HardwareIds.Add(GenerateHardwareId8(Manufacturer, SKUNumber));
            }

            if (ProductName != null && BaseboardManufacturer != null && BaseboardProduct != null)
            {
                HardwareIds.Add(GenerateHardwareId9(Manufacturer, ProductName, BaseboardManufacturer, BaseboardProduct));
            }

            if (ProductName != null)
            {
                HardwareIds.Add(GenerateHardwareId10(Manufacturer, ProductName));
            }

            if (Family != null && BaseboardManufacturer != null && BaseboardProduct != null)
            {
                HardwareIds.Add(GenerateHardwareId11(Manufacturer, Family, BaseboardManufacturer, BaseboardProduct));
            }

            if (Family != null)
            {
                HardwareIds.Add(GenerateHardwareId12(Manufacturer, Family));
            }

            if (EnclosureType != null)
            {
                HardwareIds.Add(GenerateHardwareId13(Manufacturer, EnclosureType));
            }

            if (BaseboardManufacturer != null && BaseboardProduct != null)
            {
                HardwareIds.Add(GenerateHardwareId14(Manufacturer, BaseboardManufacturer, BaseboardProduct));
            }

            HardwareIds.Add(GenerateHardwareId15(Manufacturer));

            return [.. HardwareIds];
        }

        public static string GenerateHardwareId(string[] BIOSFields)
        {
            string inputString = string.Join('&', BIOSFields);
            Guid guidFromString = ComputerHardwareIDProvider.Class5GuidFromString(inputString);
            string guidString = $"{{{guidFromString}}}";
            return guidString;
        }
    }
}
