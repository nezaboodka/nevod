using System;
using System.IO;
using System.Reflection;

namespace Nezaboodka.Nevod
{
    internal static class PathUtils
    {
        private static readonly Func<string, string> NormalizePathCaseFunc;
        
        public static bool IsFileSystemCaseSensitive { get; }
        
        static PathUtils()
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            // If file system is not case sensitive, File.Exists for assembly path converted to upper case will return true.
            if (File.Exists(assemblyPath.ToUpper()))
            {
                IsFileSystemCaseSensitive = false;
                NormalizePathCaseFunc = path => path?.ToLower();
            }
            else
            {
                IsFileSystemCaseSensitive = true;
                NormalizePathCaseFunc = path => path;
            }
        }

        public static string NormalizePathCase(string path) => NormalizePathCaseFunc(path);
    }
}
