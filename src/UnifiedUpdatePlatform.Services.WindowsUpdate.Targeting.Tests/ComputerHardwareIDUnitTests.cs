namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting.Tests
{
    [TestClass]
    public class ComputerHardwareIDUnitTests
    {
        private readonly string BIOSVendor;
        private readonly string BIOSVersionString;
        private readonly string SystemBIOSMajorRelease;
        private readonly string SystemBIOSMinorRelease;

        private readonly string SystemManufacturer;
        private readonly string SystemFamily;
        private readonly string SystemProductName;
        private readonly string SKUNumber;

        private readonly string SystemEnclosureorChassisType;

        private readonly string BaseboardManufacturer;
        private readonly string BaseboardProductName;

        private readonly string HardwareId1;
        private readonly string HardwareId2;
        private readonly string HardwareId3;
        private readonly string HardwareId4;
        private readonly string HardwareId5;
        private readonly string HardwareId6;
        private readonly string HardwareId7;
        private readonly string HardwareId8;
        private readonly string HardwareId9;
        private readonly string HardwareId10;
        private readonly string HardwareId11;
        private readonly string HardwareId12;
        private readonly string HardwareId13;
        private readonly string HardwareId14;
        private readonly string HardwareId15;

        public ComputerHardwareIDUnitTests()
        {
            BIOSVendor = "Microsoft Corporation";
            BIOSVersionString = "160.2002.235";
            SystemBIOSMajorRelease = "ff";
            SystemBIOSMinorRelease = "ff";

            SystemManufacturer = "Microsoft Corporation";
            SystemFamily = "Surface";
            SystemProductName = "Microsoft Surface Pro, 11th Edition";
            SKUNumber = "Surface_Pro_11th_Edition_2076";

            SystemEnclosureorChassisType = "9";

            BaseboardManufacturer = "Microsoft Corporation";
            BaseboardProductName = "Microsoft Surface Pro, 11th Edition";

            HardwareId1 = "{2a25cf22-7385-57f4-a540-83e187999901}";
            HardwareId2 = "{ca17c43e-840f-5ce5-8dfb-8b847cc3c9da}";
            HardwareId3 = "{83dfc1ca-dad7-5529-9805-3b4d4468b2a1}";
            HardwareId4 = "{aca467c0-5fc2-59ad-8ed5-1b7a0988d11c}";
            HardwareId5 = "{95971fb3-d478-591f-9ea3-eb0af0d1dfb5}";
            HardwareId6 = "{c9c14db9-2b61-597a-a4ba-84397fe75f63}";
            HardwareId7 = "{7cef06f5-e7e6-56d7-b123-a6d640a5d302}";
            HardwareId8 = "{48b86a5e-1955-5799-9577-150f9e1a69e4}";
            HardwareId9 = "{06128fee-87dc-50f6-8a3f-97cd9a6d8bf6}";
            HardwareId10 = "{84b2e1d1-e695-5f41-8c41-cf1f059c616a}";
            HardwareId11 = "{16a47337-1f8b-5bd3-b3bd-8e50b31cb1c9}";
            HardwareId12 = "{ca2e5189-1d32-509f-88a0-d4ebcc721899}";
            HardwareId13 = "{aca387a9-183e-5da9-8f9d-f460c3f50f54}";
            HardwareId14 = "{fdef4ae0-6bfb-5706-8aae-a565639505f5}";
            HardwareId15 = "{cc0aea32-ad2c-5013-8bed-cede6be8c9f4}";
        }

        [TestMethod]
        public void Test_GenerateHardwareId1_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId1(SystemManufacturer, SystemFamily, SystemProductName, SKUNumber, BIOSVendor, BIOSVersionString, SystemBIOSMajorRelease, SystemBIOSMinorRelease);
            Assert.AreEqual(HardwareId1, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId2_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId2(SystemManufacturer, SystemFamily, SystemProductName, BIOSVendor, BIOSVersionString, SystemBIOSMajorRelease, SystemBIOSMinorRelease);
            Assert.AreEqual(HardwareId2, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId3_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId3(SystemManufacturer, SystemProductName, BIOSVendor, BIOSVersionString, SystemBIOSMajorRelease, SystemBIOSMinorRelease);
            Assert.AreEqual(HardwareId3, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId4_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId4(SystemManufacturer, SystemFamily, SystemProductName, SKUNumber, BaseboardManufacturer, BaseboardProductName);
            Assert.AreEqual(HardwareId4, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId5_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId5(SystemManufacturer, SystemFamily, SystemProductName, SKUNumber);
            Assert.AreEqual(HardwareId5, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId6_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId6(SystemManufacturer, SystemFamily, SystemProductName);
            Assert.AreEqual(HardwareId6, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId7_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId7(SystemManufacturer, SKUNumber, BaseboardManufacturer, BaseboardProductName);
            Assert.AreEqual(HardwareId7, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId8_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId8(SystemManufacturer, SKUNumber);
            Assert.AreEqual(HardwareId8, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId9_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId9(SystemManufacturer, SystemProductName, BaseboardManufacturer, BaseboardProductName);
            Assert.AreEqual(HardwareId9, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId10_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId10(SystemManufacturer, SystemProductName);
            Assert.AreEqual(HardwareId10, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId11_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId11(SystemManufacturer, SystemFamily, BaseboardManufacturer, BaseboardProductName);
            Assert.AreEqual(HardwareId11, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId12_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId12(SystemManufacturer, SystemFamily);
            Assert.AreEqual(HardwareId12, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId13_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId13(SystemManufacturer, SystemEnclosureorChassisType);
            Assert.AreEqual(HardwareId13, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId14_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId14(SystemManufacturer, BaseboardManufacturer, BaseboardProductName);
            Assert.AreEqual(HardwareId14, GeneratedComputerHardwareID);
        }

        [TestMethod]
        public void Test_GenerateHardwareId15_Equals_Reference_Data()
        {
            string GeneratedComputerHardwareID = ComputerHardwareID.GenerateHardwareId15(SystemManufacturer);
            Assert.AreEqual(HardwareId15, GeneratedComputerHardwareID);
        }
    }
}