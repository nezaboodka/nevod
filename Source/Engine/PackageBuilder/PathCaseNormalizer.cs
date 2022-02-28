using System;
using System.IO;
using System.Reflection;

namespace Nezaboodka.Nevod
{
    internal static class PathCaseNormalizer
    {
        private static readonly Func<string, string> NormalizePathCase;

        public static bool IsFileSystemCaseSensitive { get; }

        static PathCaseNormalizer()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            // If file system is not case sensitive, File.Exists for assembly path converted to upper case will return true.
            if (File.Exists(assemblyPath.ToUpper()))
            {
                IsFileSystemCaseSensitive = false;
                NormalizePathCase = path => path?.ToLower();
            }
            else
            {
                IsFileSystemCaseSensitive = true;
                NormalizePathCase = path => path;
            }
        }

        public static string Normalize(string path) => NormalizePathCase(path);
    }
}
