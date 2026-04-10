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
namespace UnifiedUpdatePlatform.Services.WindowsUpdate
{
    public static class Constants
    {
        public const string Endpoint = "https://fe3cr.delivery.mp.microsoft.com/ClientWebService/client.asmx";
        public const string ClientProtocolVersion = "2.50";
        public const string OldCookieExpiration = "2016-07-27T07:18:09Z";
        public const string LastChangeDate = "2015-10-21T17:01:07.1472913Z";
        public const string SecurityExpirationTimestamp = "2044-08-02T20:09:03Z";

        public static readonly string Action = $"{Models.Constants.ClientWebServiceServerNamespace}/";
        public static readonly string UserAgent = $"Windows-Update-Agent/10.0.10011.16384 Client-Protocol/{ClientProtocolVersion}";
        public static readonly int[] InstalledNonLeafUpdateIDs =
        [
            1,
            2,
            3,
            10,
            11,
            17,
            19,
            2359974,
            2359977,
            5143990,
            5169043,
            5169044,
            5169047,
            8788830,
            8806526,
            9125350,
            9154769,
            10809856,
            23110993,
            23110994,
            23110995,
            23110996,
            23110999,
            23111000,
            23111001,
            23111002,
            23111003,
            23111004,
            24513870,
            28880263,
            30077688,
            30486944,
            59830006,
            59830007,
            59830008,
            60484010,
            62450018,
            62450019,
            62450020,
            98959022,
            98959023,
            98959024,
            98959025,
            98959026,
            105939029,
            105995585,
            106017178,
            107825194,
            117765322,
            129905029,
            130040030,
            130040031,
            130040032,
            130040033,
            133399034,
            138372035,
            138372036,
            139536037,
            139536038,
            139536039,
            139536040,
            142045136,
            158941041,
            158941042,
            158941043,
            158941044,
            159776047,
            160733048,
            160733049,
            160733050,
            160733051,
            160733055,
            160733056,
            161870057,
            161870058,
            161870059
        ];
    }
}