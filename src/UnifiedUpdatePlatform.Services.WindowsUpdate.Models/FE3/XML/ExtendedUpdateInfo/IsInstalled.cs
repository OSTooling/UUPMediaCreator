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
    [XmlRoot(ElementName = "IsInstalled")]
    public class IsInstalled
    {
        [XmlElement(ElementName = "b.WindowsVersion")]
        public BWindowsVersion BWindowsVersion
        {
            get; set;
        }

        [XmlElement(ElementName = "True")]
        public string True
        {
            get; set;
        }

        [XmlElement(ElementName = "Or")]
        public Or Or
        {
            get; set;
        }

        [XmlElement(ElementName = "And")]
        public And And
        {
            get; set;
        }

        [XmlElement(ElementName = "b.WmiQuery")]
        public BWmiQuery BWmiQuery
        {
            get; set;
        }

        [XmlElement(ElementName = "b.RegSz")]
        public BRegSz BRegSz
        {
            get; set;
        }

        [XmlElement(ElementName = "b.SystemMetric")]
        public BSystemMetric BSystemMetric
        {
            get; set;
        }

        [XmlElement(ElementName = "b.Memory")]
        public BMemory BMemory
        {
            get; set;
        }

        [XmlElement(ElementName = "b.Direct3D")]
        public BDirect3D BDirect3D
        {
            get; set;
        }

        [XmlElement(ElementName = "b.SensorById")]
        public BSensorById BSensorById
        {
            get; set;
        }

        [XmlElement(ElementName = "b.Camera")]
        public BCamera BCamera
        {
            get; set;
        }

        [XmlElement(ElementName = "b.NFC")]
        public BNFC BNFC
        {
            get; set;
        }

        [XmlElement(ElementName = "b.VideoMemory")]
        public BVideoMemory BVideoMemory
        {
            get; set;
        }

        [XmlElement(ElementName = "AppxPackageInstalled")]
        public string AppxPackageInstalled
        {
            get; set;
        }
    }
}