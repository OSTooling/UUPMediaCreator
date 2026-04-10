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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.JSON.AppxMetadata;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.Common;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.GetCookie.Request;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.GetCookie.Response;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.GetExtendedUpdateInfo.Request;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.GetExtendedUpdateInfo.Response;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.GetExtendedUpdateInfo2.Request;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.GetExtendedUpdateInfo2.Response;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.SyncUpdates.Request;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.SOAP.SyncUpdates.Response;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Models.FE3.XML.ExtendedUpdateInfo;
using UnifiedUpdatePlatform.Services.WindowsUpdate.Targeting;

namespace UnifiedUpdatePlatform.Services.WindowsUpdate
{
    public static class FE3Handler
    {
        private static readonly CorrelationVector correlationVector = new();
        private static string MSCV = correlationVector.GetValue();
        private static readonly HttpClient httpClient = new(new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator // Linux
        });

        #region Data manipulation

        private static async Task<string> PostToWindowsUpdateAsync(string method, string message, bool secured)
        {
            string _endpoint = Constants.Endpoint;
            if (secured)
            {
                _endpoint += "/secured";
            }

            MSCV = correlationVector.Increment();

            StringContent content = new(message, System.Text.Encoding.UTF8, "application/soap+xml");

            HttpRequestMessage req = new(HttpMethod.Post, _endpoint)
            {
                Content = content
            };

            req.Headers.Add("MS-CV", MSCV);
            req.Headers.Add("SOAPAction", Constants.Action + method);
            req.Headers.Add("User-agent", Constants.UserAgent);
            _ = req.Headers.TryAddWithoutValidation("Cache-Control", "no-cache");
            req.Headers.Pragma.Add(new System.Net.Http.Headers.NameValueHeaderValue("no-cache"));
            req.Headers.Connection.Add("keep-alive");

            HttpResponseMessage response = (await httpClient.SendAsync(req)).EnsureSuccessStatusCode();
            string resultString = await response.Content.ReadAsStringAsync();
            return resultString;
        }

        private static Envelope GetEnveloppe(string method, string authorizationToken, bool secured)
        {
            string _endpoint = Constants.Endpoint;
            if (secured)
            {
                _endpoint += "/secured";
            }

            Envelope envelope = new()
            {
                Header = new Header()
                {
                    Action = new Models.FE3.SOAP.Common.Action()
                    {
                        MustUnderstand = "1",
                        Text = Constants.Action + method
                    },
                    MessageID = $"urn:uuid:{Guid.NewGuid():D}",
                    To = new To()
                    {
                        MustUnderstand = "1",
                        Text = _endpoint
                    },
                    Security = new Security()
                    {
                        MustUnderstand = "1",
                        Timestamp = new Timestamp()
                        {
                            Created = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                            Expires = Constants.SecurityExpirationTimestamp
                        },
                        WindowsUpdateTicketsToken = new WindowsUpdateTicketsToken()
                        {
                            Id = "ClientMSA"
                        }
                    }
                },
                Body = new Body()
            };

            if (!string.IsNullOrEmpty(authorizationToken))
            {
                envelope.Header.Security.WindowsUpdateTicketsToken.TicketType =
                [
                    new()
                    {
                        Name = "MSA",
                        Version = "1.0",
                        Policy = "MBI_SSL",
                        User = authorizationToken
                    },
                    new()
                    {
                        Name = "AAD",
                        Version = "1.0",
                        Policy = "MBI_SSL"
                    }
                ];
            }
            else
            {
                envelope.Header.Security.WindowsUpdateTicketsToken.Text = "";
            }

            return envelope;
        }

        private static string SerializeSOAPEnvelope(Envelope envelope)
        {
            if (envelope == null)
            {
                return string.Empty;
            }

            XmlSerializer xmlSerializer = new(typeof(Envelope));

            XmlSerializerNamespaces ns = new();
            ns.Add("s", "http://www.w3.org/2003/05/soap-envelope");
            ns.Add("a", "http://www.w3.org/2005/08/addressing");

            using StringWriter stringWriter = new();
            using XmlWriter xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true });
            xmlSerializer.Serialize(xmlWriter, envelope, ns);
            return stringWriter.ToString().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n", "").Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>\n", "");
        }

        private static Envelope DeserializeSOAPEnvelope(string message)
        {
            if (message == null)
            {
                return null;
            }

            XmlSerializer xmlSerializer = new(typeof(Envelope));

            using StringReader stringReader = new(message);
            return (Envelope)xmlSerializer.Deserialize(stringReader);
        }

        private static Xml DeserializeInfoXML(string xml)
        {
            if (xml == null)
            {
                return null;
            }

            string message = "<Xml>" + xml + "</Xml>";

            XmlSerializer xmlSerializer = new(typeof(Xml));

            using StringReader stringReader = new(message);
            return (Xml)xmlSerializer.Deserialize(stringReader);
        }

        private static AppxMetadataJson DeserializeAppxJSON(string json)
        {
            return JsonSerializer.Deserialize<AppxMetadataJson>(json);
        }

        #endregion Data manipulation

        #region WU functions

        private static async Task<(GetCookieResponse, string)> GetCookie()
        {
            Envelope envelope = GetEnveloppe("GetCookie", null, false);

            envelope.Body.GetCookie = new GetCookie()
            {
                OldCookie = new OldCookie()
                {
                    Expiration = Constants.OldCookieExpiration
                },
                LastChange = Constants.LastChangeDate,
                CurrentTime = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                ProtocolVersion = Constants.ClientProtocolVersion
            };

            string message = SerializeSOAPEnvelope(envelope);

            string response = await PostToWindowsUpdateAsync("GetCookie", message, false);

            Envelope renvelope = DeserializeSOAPEnvelope(response);

            return (renvelope.Body.GetCookieResponse, response);
        }

        private static async Task<(GetExtendedUpdateInfoResponse, string)> GetExtendedUpdateInfo(
            Models.FE3.SOAP.Common.Cookie cookie,
            string token,
            string[] revisionId,
            CTAC ctac)
        {
            Envelope envelope = GetEnveloppe("GetExtendedUpdateInfo", token, false);

            envelope.Body.GetExtendedUpdateInfo = new GetExtendedUpdateInfo()
            {
                Cookie = cookie,
                RevisionIDs = new RevisionIDs()
                {
                    Int = revisionId
                },
                InfoTypes = new InfoTypes()
                {
                    XmlUpdateFragmentType =
                    [
                        "FileUrl",
                        "FileDecryption",
                        "Extended",
                        "LocalizedProperties",
                        "Eula",
                        "Published",
                        "Core",
                        "VerificationRule"
                    ]
                },
                Locales = new Locales()
                {
                    String = ["en-US", "en"]
                },
                DeviceAttributes = ctac.DeviceAttributes
            };

            string message = SerializeSOAPEnvelope(envelope);

            string response = await PostToWindowsUpdateAsync("GetCookie", message, false);

            Envelope renvelope = DeserializeSOAPEnvelope(response);

            return (renvelope.Body.GetExtendedUpdateInfoResponse, response);
        }

        public static async Task<(GetExtendedUpdateInfo2Response, string)> GetExtendedUpdateInfo2(
            string token,
            string UpdateID,
            string RevisionNumber,
            CTAC ctac
            )
        {
            Envelope envelope = GetEnveloppe("GetExtendedUpdateInfo2", token, true);

            envelope.Body.GetExtendedUpdateInfo2 = new GetExtendedUpdateInfo2()
            {
                UpdateIDs = new UpdateIDs()
                {
                    UpdateIdentity = new Models.FE3.SOAP.GetExtendedUpdateInfo2.Request.UpdateIdentity()
                    {
                        UpdateID = UpdateID,
                        RevisionNumber = RevisionNumber
                    }
                },
                InfoTypes = new InfoTypes()
                {
                    XmlUpdateFragmentType =
                    [
                        "FileUrl",
                        "FileDecryption",
                        "EsrpDecryptionInformation",
                        "PiecesHashUrl",
                        "BlockMapUrl"
                    ]
                },
                DeviceAttributes = ctac.DeviceAttributes
            };

            string message = SerializeSOAPEnvelope(envelope);

            string response = await PostToWindowsUpdateAsync("GetExtendedUpdateInfo2", message, true);

            Envelope renvelope = DeserializeSOAPEnvelope(response);

            return (renvelope.Body.GetExtendedUpdateInfo2Response, response);
        }

        private static async Task<(SyncUpdatesResponse, string)> SyncUpdates(
            Models.FE3.SOAP.Common.Cookie cookie,
            string token,
            IEnumerable<string> InstalledNonLeafUpdateIDs,
            IEnumerable<string> OtherCachedUpdateIDs,
            string[] CategoryIdentifiers,
            CTAC ctac
            )
        {
            string[] _InstalledNonLeafUpdateIDs = Constants.InstalledNonLeafUpdateIDs.Select(x => x.ToString()).ToArray();

            if (InstalledNonLeafUpdateIDs != null)
            {
                List<string> tmplist = [.. _InstalledNonLeafUpdateIDs];
                tmplist.AddRange(InstalledNonLeafUpdateIDs);
                _InstalledNonLeafUpdateIDs = [.. tmplist];
            }

            Envelope envelope = GetEnveloppe("SyncUpdates", token, false);

            envelope.Body.SyncUpdates = new SyncUpdates()
            {
                Cookie = cookie,
                Parameters = new Parameters()
                {
                    ExpressQuery = "false",
                    InstalledNonLeafUpdateIDs = new InstalledNonLeafUpdateIDs()
                    {
                        Int = _InstalledNonLeafUpdateIDs
                    },
                    OtherCachedUpdateIDs = new OtherCachedUpdateIDs()
                    {
                        Int = OtherCachedUpdateIDs != null ? OtherCachedUpdateIDs.ToArray() : []
                    },
                    SkipSoftwareSync = "false",
                    NeedTwoGroupOutOfScopeUpdates = "true",
                    FilterAppCategoryIds = CategoryIdentifiers != null && CategoryIdentifiers.Length != 0 ? new FilterAppCategoryIds()
                    {
                        CategoryIdentifier = new CategoryIdentifier()
                        {
                            Id = CategoryIdentifiers ?? []
                        }
                    } : null,
                    AlsoPerformRegularSync = "false",
                    ComputerSpec = "",
                    ExtendedUpdateInfoParameters = new ExtendedUpdateInfoParameters()
                    {
                        XmlUpdateFragmentTypes = new XmlUpdateFragmentTypes()
                        {
                            XmlUpdateFragmentType =
                            [
                                "Extended",
                                "LocalizedProperties",
                                "Eula",
                                "Published",
                                "Core"
                            ]
                        },
                        Locales = new Locales()
                        {
                            String = ["en-US", "en"]
                        }
                    },
                    ClientPreferredLanguages = "",
                    ProductsParameters = new ProductsParameters()
                    {
                        SyncCurrentVersionOnly = ctac.SyncCurrentVersionOnly ? "true" : "false",
                        DeviceAttributes = ctac.DeviceAttributes,
                        CallerAttributes = ctac.CallerAttributes,
                        Products = ctac.Products
                    }
                }
            };

            string message = SerializeSOAPEnvelope(envelope);

            string response = await PostToWindowsUpdateAsync("SyncUpdates", message, false);

            Envelope responseEnvelope = DeserializeSOAPEnvelope(response);

            return (responseEnvelope.Body.SyncUpdatesResponse, response);
        }

        #endregion WU functions

        #region Application specific functions

        public static async Task<IEnumerable<UpdateData>> GetUpdates(string[] categoryIds, CTAC ctac, string token, FileExchangeV3UpdateFilter filter = FileExchangeV3UpdateFilter.Application) // Or ProductRelease
        {
            (GetCookieResponse cookie, string cookieresp) = await GetCookie();

            HashSet<string> InstalledNonLeafUpdateIDs = [];
            HashSet<string> OtherCachedUpdateIDs = [];

            HashSet<(SyncUpdatesResponse, string)> responses = [];

            //
            // Scan all updates
            // WU will not return all updates in one go
            // So we need to perform multiple scans and cache the ids
            //
            while (true)
            {
                (SyncUpdatesResponse, string) result = await SyncUpdates(cookie.GetCookieResult, token, InstalledNonLeafUpdateIDs, OtherCachedUpdateIDs, categoryIds ?? [], ctac);

                // Refresh the cookie
                cookie.GetCookieResult.EncryptedData = result.Item1.SyncUpdatesResult.NewCookie.EncryptedData;
                cookie.GetCookieResult.Expiration = result.Item1.SyncUpdatesResult.NewCookie.Expiration;

                if (result.Item1.SyncUpdatesResult.ExtendedUpdateInfo == null || result.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update == null || result.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update.Length == 0)
                {
                    break;
                }

                foreach (Update update in result.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update)
                {
                    _ = InstalledNonLeafUpdateIDs.Add(update.ID);
                    _ = OtherCachedUpdateIDs.Add(update.ID);
                }

                _ = responses.Add(result);
            }

            HashSet<UpdateData> updateDatas = [];

            foreach ((SyncUpdatesResponse, string) response in responses)
            {
                foreach (Update update in response.Item1.SyncUpdatesResult.ExtendedUpdateInfo.Updates.Update)
                {
                    UpdateData data = new()
                    {
                        Update = update
                    };

                    foreach (UpdateInfo updateInfo in response.Item1.SyncUpdatesResult.NewUpdates.UpdateInfo)
                    {
                        if (ulong.Parse(update.ID) == ulong.Parse(updateInfo.ID))
                        {
                            data.UpdateInfo = updateInfo;
                            break;
                        }
                    }

                    Xml updateXml = DeserializeInfoXML(data.Update.Xml + data.UpdateInfo.Xml);
                    data.Xml = updateXml;

                    if (data.Xml.ApplicabilityRules?.Metadata?.AppxPackageMetadata?.AppxMetadata != null)
                    {
                        data.AppxMetadata = DeserializeAppxJSON(data.Xml.ApplicabilityRules.Metadata.AppxPackageMetadata.AppxMetadata.ApplicabilityBlob);
                    }

                    if (updateDatas.Any(x => x.Update.ID == update.ID))
                    {
                        UpdateData updateData = updateDatas.First(x => x.Update.ID == update.ID);
                        if (data.Xml.LocalizedProperties == null)
                        {
                            Xml backup = updateData.Xml;
                            updateData.Xml = data.Xml;

                            updateData.Xml.LocalizedProperties = backup.LocalizedProperties;
                        }

                        if (updateData.Xml.LocalizedProperties == null)
                        {
                            updateData.Xml.LocalizedProperties = data.Xml.LocalizedProperties;
                        }

                        continue;
                    }

                    data.SyncUpdatesResponse = response.Item2;

                    _ = updateDatas.Add(data);
                }
            }

            HashSet<UpdateData> relevantUpdateDatas = [];

            foreach (UpdateData updateData in updateDatas)
            {
                if (updateData.Xml.ExtendedProperties != null)
                {
                    if (updateData.Xml.ExtendedProperties.ContentType == filter.ToString() && updateData.Xml.Files != null)
                    {
                        updateData.CTAC = ctac;
                        _ = relevantUpdateDatas.Add(updateData);
                    }
                }
            }

            return relevantUpdateDatas;
        }

        #endregion Application specific functions

        #region File url specific functions

        public static async Task<FileExchangeV3FileDownloadInformation> GetFileUrl(UpdateData updateData, string fileDigest, string token = null)
        {
            (GetExtendedUpdateInfo2Response, string) result = await GetExtendedUpdateInfo2(token, updateData.Xml.UpdateIdentity.UpdateID, updateData.Xml.UpdateIdentity.RevisionNumber, updateData.CTAC);

            if (updateData.Xml?.Files?.File?.FirstOrDefault(x => x.AdditionalDigest?.Text == fileDigest) is Models.FE3.XML.ExtendedUpdateInfo.File file)
            {
                fileDigest = file.Digest;
            }

            if (result.Item1.GetExtendedUpdateInfo2Result.FileLocations != null)
            {
                foreach (FileLocation fileLocation in result.Item1.GetExtendedUpdateInfo2Result.FileLocations.FileLocation)
                {
                    if (fileLocation.FileDigest == fileDigest)
                    {
                        return new FileExchangeV3FileDownloadInformation(fileLocation);
                    }
                }
            }

            return null;
        }

        public static async Task<IEnumerable<FileExchangeV3FileDownloadInformation>> GetFileUrls(UpdateData updateData, string token = null)
        {
            (GetExtendedUpdateInfo2Response, string) result = await GetExtendedUpdateInfo2(token, updateData.Xml.UpdateIdentity.UpdateID, updateData.Xml.UpdateIdentity.RevisionNumber, updateData.CTAC);

            updateData.GEI2Response = result.Item2;

            return result.Item1.GetExtendedUpdateInfo2Result.FileLocations?.FileLocation.Select(x => new FileExchangeV3FileDownloadInformation(x));
        }

        #endregion File url specific functions
    }
}