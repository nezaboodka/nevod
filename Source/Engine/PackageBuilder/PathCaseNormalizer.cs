using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Nezaboodka.Nevod
{
    internal class PathCaseNormalizer
    {
        public bool IsFileSystemCaseSensitive { get; }

        public PathCaseNormalizer()
            : this (isFileSystemCaseSensitive: null)
        {
        }

        public PathCaseNormalizer(bool? isFileSystemCaseSensitive)
        {
            if (isFileSystemCaseSensitive != null)
                IsFileSystemCaseSensitive = isFileSystemCaseSensitive.Value;
            else
                IsFileSystemCaseSensitive = DetermineCaseSensitivityBasedOnOperatingSystem();
        }

        public string Normalize(string path)
        {
            string normalizedPath;
            if (IsFileSystemCaseSensitive)
                normalizedPath = path;
            else
                normalizedPath = path?.ToLower();
            return normalizedPath;
        }

        private bool DetermineCaseSensitivityBasedOnOperatingSystem()
        {
            bool isFileSystemCaseSensitive;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                isFileSystemCaseSensitive = false;
            else
                isFileSystemCaseSensitive = true;
            return isFileSystemCaseSensitive;
        }
    }
}
