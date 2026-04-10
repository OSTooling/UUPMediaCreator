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
using System.Xml;
using System.Xml.Serialization;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.XML.ExtendedUpdateInfo
{
    [XmlRoot(ElementName = "ExtendedProperties")]
    public class ExtendedProperties
    {
        [XmlAttribute(AttributeName = "DefaultPropertiesLanguage")]
        public string DefaultPropertiesLanguage
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "Handler")]
        public string Handler
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "CreationDate")]
        public string CreationDate
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "IsAppxFramework")]
        public string IsAppxFramework
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "CompatibleProtocolVersion")]
        public string CompatibleProtocolVersion
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "FromStoreService")]
        public string FromStoreService
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "ContentType")]
        public string ContentType
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "PackageIdentityName")]
        public string PackageIdentityName
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "LegacyMobileProductId")]
        public string LegacyMobileProductId
        {
            get; set;
        }

        [XmlElement(ElementName = "InstallationBehavior")]
        public string InstallationBehavior
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "MaxDownloadSize")]
        public string MaxDownloadSize
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "MinDownloadSize")]
        public string MinDownloadSize
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "PackageContentId")]
        public string PackageContentId
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "ProductName")]
        public string ProductName
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "ReleaseVersion")]
        public string ReleaseVersion
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "AutoSelectOnWebsites")]
        public string AutoSelectOnWebsites
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "BrowseOnly")]
        public string BrowseOnly
        {
            get; set;
        }
    }
}