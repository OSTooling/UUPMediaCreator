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

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.SyncUpdates.Request
{
    [XmlRoot(ElementName = "parameters", Namespace = Constants.ClientWebServiceServerNamespace)]
    public class Parameters
    {
        [XmlElement(ElementName = "ExpressQuery", Namespace = Constants.ClientWebServiceServerNamespace)]
        public string ExpressQuery
        {
            get; set;
        }

        [XmlElement(ElementName = "InstalledNonLeafUpdateIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
        public InstalledNonLeafUpdateIDs InstalledNonLeafUpdateIDs
        {
            get; set;
        }

        [XmlElement(ElementName = "OtherCachedUpdateIDs", Namespace = Constants.ClientWebServiceServerNamespace)]
        public OtherCachedUpdateIDs OtherCachedUpdateIDs
        {
            get; set;
        }

        [XmlElement(ElementName = "SkipSoftwareSync", Namespace = Constants.ClientWebServiceServerNamespace)]
        public string SkipSoftwareSync
        {
            get; set;
        }

        [XmlElement(ElementName = "NeedTwoGroupOutOfScopeUpdates", Namespace = Constants.ClientWebServiceServerNamespace)]
        public string NeedTwoGroupOutOfScopeUpdates
        {
            get; set;
        }

        [XmlElement(ElementName = "FilterAppCategoryIds", Namespace = Constants.ClientWebServiceServerNamespace)]
        public FilterAppCategoryIds FilterAppCategoryIds
        {
            get; set;
        }

        [XmlElement(ElementName = "AlsoPerformRegularSync", Namespace = Constants.ClientWebServiceServerNamespace)]
        public string AlsoPerformRegularSync
        {
            get; set;
        }

        [XmlElement(ElementName = "ComputerSpec", Namespace = Constants.ClientWebServiceServerNamespace)]
        public string ComputerSpec
        {
            get; set;
        }

        [XmlElement(ElementName = "ExtendedUpdateInfoParameters", Namespace = Constants.ClientWebServiceServerNamespace)]
        public ExtendedUpdateInfoParameters ExtendedUpdateInfoParameters
        {
            get; set;
        }

        [XmlElement(ElementName = "ClientPreferredLanguages", Namespace = Constants.ClientWebServiceServerNamespace)]
        public string ClientPreferredLanguages
        {
            get; set;
        }

        [XmlElement(ElementName = "ProductsParameters", Namespace = Constants.ClientWebServiceServerNamespace)]
        public ProductsParameters ProductsParameters
        {
            get; set;
        }
    }
}