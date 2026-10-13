using System;
using System.IO;
using System.Linq;

namespace UnifiedUpdatePlatform.Media.Creator.Utils
{
    public static class FolderUtilities
    {
        public static void TrySetTimestampsRecursive(string path, DateTime dateTime)
        {
            foreach (string entry in Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories))
            {
                if (Directory.Exists(entry))
                {
                    try
                    {
                        Directory.SetCreationTimeUtc(entry, dateTime);
                    }
                    catch { }
                    try
                    {
                        Directory.SetLastAccessTimeUtc(entry, dateTime);
                    }
                    catch { }
                    try
                    {
                        Directory.SetLastWriteTimeUtc(entry, dateTime);
                    }
                    catch { }
                }
                else
                {
                    try
                    {
                        File.SetCreationTimeUtc(entry, dateTime);
                    }
                    catch { }
                    try
                    {
                        File.SetLastAccessTimeUtc(entry, dateTime);
                    }
                    catch { }
                    try
                    {
                        File.SetLastWriteTimeUtc(entry, dateTime);
                    }
                    catch { }
                }
            }
        }

        public static string GetParentPath(string path)
        {
            return path.Contains(Path.DirectorySeparatorChar) ?
                string.Join(Path.DirectorySeparatorChar, path.Split(Path.DirectorySeparatorChar).AsEnumerable().Reverse().Skip(1).Reverse()) : "";
        }
    }
}
