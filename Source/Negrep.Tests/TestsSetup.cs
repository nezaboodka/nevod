using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Nezaboodka.Nevod.Negrep.Tests
{
    [SetUpFixture]
    public class TestsSetup
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Directory.CreateDirectory("TestFiles");
            Directory.SetCurrentDirectory("TestFiles");
            MakeFileWithCRLF("file1");
            MakeFileWithLF("file2");
            MakeFileWithCRLFandCRandLF("file3");
            MakeFileWithPatterns("patterns.np");
        }

        private void MakeFileWithCRLF(string filepath)
        {
            string fileContent = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE?\r\nCRLF";
            File.WriteAllText(filepath, fileContent);
        }

        private void MakeFileWithLF(string filepath)
        {
            string fileContent = "CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS\nLF";
            File.WriteAllText(filepath, fileContent);
        }

        private void MakeFileWithCRLFandCRandLF(string filepath)
        {
            string fileContent = "Yu said it\rwas the world's\nfirst 5G modem\r\nCRLF CR LF";
            File.WriteAllText(filepath, fileContent);
        }

        private void MakeFileWithPatterns(string filepath)
        {
            string fileContent = "#Phone = {'Android', 'iPhone', 'Huawei'};\n#FirstWord = Start + Word;";
            File.WriteAllText(filepath, fileContent);
        }
    }
}