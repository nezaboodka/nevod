using System;
using System.IO;
using System.Text;

namespace Nezaboodka.Nevod.Negrep
{
    public static class NegrepFileContentProvider
    {
        private static readonly string NegrepLocation = Path.GetDirectoryName(typeof(Negrep).Assembly.Location);

        public static string FileContentProvider(string path)
        {
            string relative = Path.GetRelativePath(Environment.CurrentDirectory, path);
            string fromNegrepLocation = Path.Combine(NegrepLocation, relative);
            string result = File.Exists(fromNegrepLocation) 
                ? File.ReadAllText(fromNegrepLocation, Encoding.UTF8) 
                : File.ReadAllText(path, Encoding.UTF8);
            return result;
        }
    }
}