/*
 * Copyright (c) ADeltaX and Contributors
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

//
// Source: https://raw.githubusercontent.com/ADeltaX/ProtoBuildBot/main/src/BuildChecker/Classes/Helpers/CorrelationVector.cs
// Released under the MIT License (as of 2020-11-04)
//

namespace UnifiedUpdatePlatform.Services.WindowsUpdate
{
    internal partial class CorrelationVector
    {
        private string baseVector;
        private int currentVector;

        private readonly string base64CharSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private readonly int id0Length = 16;
        private bool isInitialized = false;

        internal void Init()
        {
            baseVector = SeedCorrelationVector();
            currentVector = 1;
            isInitialized = true;
        }

        protected static int GetCllSettingsAsInt(Settings setting)
        {
            int asInt = (int)setting;
            return asInt;
        }

        private bool CanExtend()
        {
            int vectorSize = (int)Math.Floor(Math.Log10(currentVector) + 1);

            return baseVector.Length + 1 + vectorSize + 1 + 1 <= GetCllSettingsAsInt(Settings.MAXCORRELATIONVECTORLENGTH);
        }

        private bool CanIncrement(int newVector)
        {
            if (newVector - 1 == int.MaxValue)
            {
                return false;
            }
            int vectorSize = (int)Math.Floor(Math.Log10(newVector) + 1);

            // Get the length of the existing string + length of the new extension + the length of the dot
            return baseVector.Length + vectorSize + 1 <= GetCllSettingsAsInt(Settings.MAXCORRELATIONVECTORLENGTH);
        }

        internal string Extend()
        {
            if (!isInitialized)
            {
                Init();
            }

            if (CanExtend())
            {
                baseVector = GetValue();
                currentVector = 1;
            }

            return GetValue();
        }

        internal string GetValue()
        {
            return !isInitialized ? null : baseVector + "." + currentVector;
        }

        internal string Increment()
        {
            if (!isInitialized)
            {
                Init();
            }

            int newVector = currentVector + 1;
            // Check if we can increment
            if (CanIncrement(newVector))
            {
                currentVector = newVector;
            }

            return GetValue();
        }

        private bool IsValid(string vector)
        {
            if (vector.Length > GetCllSettingsAsInt(Settings.MAXCORRELATIONVECTORLENGTH))
            {
                return false;
            }

            string validationPattern = "^[" + base64CharSet + "]{16}(.[0-9]+)+$";
            return vector == validationPattern;
        }

        private string SeedCorrelationVector()
        {
            string result = "";

            Random r = new();
            for (int i = 0; i < id0Length; i++)
            {
                result += base64CharSet[r.Next(base64CharSet.Length)];
            }

            return result;
        }

        internal void SetValue(string vector)
        {
            if (IsValid(vector))
            {
                int lastDot = vector.LastIndexOf(".");
                baseVector = vector[..lastDot];
                currentVector = int.Parse(vector[(lastDot + 1)..]);
                isInitialized = true;
            }
            else
            {
                throw new Exception("Cannot set invalid correlation vector value");
            }
        }
    }
}