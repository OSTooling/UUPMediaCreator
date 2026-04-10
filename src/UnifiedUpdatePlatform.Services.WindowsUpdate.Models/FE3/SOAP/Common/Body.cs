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

namespace UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.Common
{
    [XmlRoot(ElementName = "Body", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
    public class Body
    {
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

        //
        // Requests
        //

        [XmlElement(ElementName = "GetCookie", Namespace = Constants.ClientWebServiceServerNamespace)]
        public GetCookie.Request.GetCookie GetCookie
        {
            get; set;
        }

        [XmlElement(ElementName = "SyncUpdates", Namespace = Constants.ClientWebServiceServerNamespace)]
        public SyncUpdates.Request.SyncUpdates SyncUpdates
        {
            get; set;
        }

        [XmlElement(ElementName = "GetExtendedUpdateInfo", Namespace = Constants.ClientWebServiceServerNamespace)]
        public GetExtendedUpdateInfo.Request.GetExtendedUpdateInfo GetExtendedUpdateInfo
        {
            get; set;
        }

        [XmlElement(ElementName = "GetExtendedUpdateInfo2", Namespace = Constants.ClientWebServiceServerNamespace)]
        public GetExtendedUpdateInfo2.Request.GetExtendedUpdateInfo2 GetExtendedUpdateInfo2
        {
            get; set;
        }

        //
        // Responses
        //

        [XmlElement(ElementName = "GetCookieResponse", Namespace = Constants.ClientWebServiceServerNamespace)]
        public GetCookie.Response.GetCookieResponse GetCookieResponse
        {
            get; set;
        }

        [XmlElement(ElementName = "SyncUpdatesResponse", Namespace = Constants.ClientWebServiceServerNamespace)]
        public SyncUpdates.Response.SyncUpdatesResponse SyncUpdatesResponse
        {
            get; set;
        }

        [XmlElement(ElementName = "GetExtendedUpdateInfoResponse", Namespace = Constants.ClientWebServiceServerNamespace)]
        public GetExtendedUpdateInfo.Response.GetExtendedUpdateInfoResponse GetExtendedUpdateInfoResponse
        {
            get; set;
        }

        [XmlElement(ElementName = "GetExtendedUpdateInfo2Response", Namespace = Constants.ClientWebServiceServerNamespace)]
        public GetExtendedUpdateInfo2.Response.GetExtendedUpdateInfo2Response GetExtendedUpdateInfo2Response
        {
            get; set;
        }
    }
}