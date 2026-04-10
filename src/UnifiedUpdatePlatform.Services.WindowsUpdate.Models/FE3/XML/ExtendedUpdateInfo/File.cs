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
    [XmlRoot(ElementName = "File")]
    public class File
    {
        [XmlElement(ElementName = "AdditionalDigest")]
        public AdditionalDigest AdditionalDigest
        {
            get; set;
        }

        [XmlElement(ElementName = "PiecesHashDigest")]
        public PiecesHashDigest PiecesHashDigest
        {
            get; set;
        }

        [XmlElement(ElementName = "BlockMapDigest")]
        public BlockMapDigest BlockMapDigest
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "FileName")]
        public string FileName
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "Digest")]
        public string Digest
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "DigestAlgorithm")]
        public string DigestAlgorithm
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "Size")]
        public string Size
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "Modified")]
        public string Modified
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "InstallerSpecificIdentifier")]
        public string InstallerSpecificIdentifier
        {
            get; set;
        }

        [XmlAttribute(AttributeName = "PatchingType")]
        public string PatchingType
        {
            get; set;
        }

        public override string? ToString()
        {
            return !string.IsNullOrEmpty(FileName) ? PatchingType + ":" + FileName : base.ToString();
        }
    }
}