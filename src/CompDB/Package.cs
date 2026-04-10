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
    [XmlRoot(ElementName = "Package")]
    public class Package
    {
        [XmlAttribute(AttributeName = "ID")]
        public string ID
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "PackageType")]
        public string PackageType
        {
            get; set;
        }
        [XmlElement(ElementName = "SatelliteInfo")]
        public SatelliteInfo SatelliteInfo
        {
            get; set;
        }
        [XmlElement(ElementName = "Payload")]
        public Payload Payload
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "InstalledSize")]
        public string InstalledSize
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "Version")]
        public string Version
        {
            get; set;
        }
        [XmlAttribute(AttributeName = "UpdateType")]
        public string UpdateType
        {
            get; set;
        }
    }
}