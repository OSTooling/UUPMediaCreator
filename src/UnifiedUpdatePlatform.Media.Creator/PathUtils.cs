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
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UnifiedUpdatePlatform.Media.Creator
{
    public static class PathUtils
    {
        public static string GetExecutableDirectory()
        {
            string fileName = Process.GetCurrentProcess().MainModule.FileName;
            return fileName.Contains(Path.DirectorySeparatorChar) ? string.Join(Path.DirectorySeparatorChar, fileName.Split(Path.DirectorySeparatorChar).AsEnumerable().Reverse().Skip(1).Reverse()) : "";
        }

        public static string GetParentExecutableDirectory()
        {
            string runningDirectory = GetExecutableDirectory();
            return runningDirectory.Contains(Path.DirectorySeparatorChar) ? string.Join(Path.DirectorySeparatorChar, runningDirectory.Split(Path.DirectorySeparatorChar).AsEnumerable().Reverse().Skip(1).Reverse()) : "";
        }
    }
}