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
using System.Xml.Serialization;

namespace UnifiedUpdatePlatform.Services.Composition.Database
{
    public class BaseManifest
    {
        [XmlElement(ElementName = "Tags")]
        public Tags Tags
        {
            get; set;
        }
        [XmlElement(ElementName = "Features")]
        public Features Features
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsi
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        public string Xsd
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "CreatedDate")]
        public string CreatedDate
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Revision")]
        public string Revision
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "SchemaVersion")]
        public string SchemaVersion
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Product")]
        public string Product
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "BuildID")]
        public string BuildID
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "BuildInfo")]
        public string BuildInfo
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "OSVersion")]
        public string OSVersion
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "BuildArch")]
        public string BuildArch
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "ReleaseType")]
        public string ReleaseType
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Type")]
        public string Type
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Name")]
        public string Name
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns
        {
            get; set;
        }
        [XmlElement(ElementName = "Packages")]
        public Packages Packages
        {
            get; set;
        }
        [XmlElement(ElementName = "AppX")]
        public Appx AppX
        {
            get; set;
        }
        [XmlElement(ElementName = "MSConditionalFeatures")]
        public MSConditionalFeatures MSConditionalFeatures
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "TargetBuildID")]
        public string TargetBuildID
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "TargetBuildInfo")]
        public string TargetBuildInfo
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "TargetOSVersion")]
        public string TargetOSVersion
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "UUPProduct")]
        public string UUPProduct
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "UUPProductVersion")]
        public string UUPProductVersion
        {
            get; set;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
