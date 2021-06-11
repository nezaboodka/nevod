//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Nezaboodka.Nevod.Negrep.Tests
{
    [TestFixture]
    public class NegrepPositiveTests
    {
        [Test]
        public async Task SearchPatternFromStdinWhenBothRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}" };
            string expected = @"
                IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenBothRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternFromStdinWith_h_OptionWhenBothRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-h" };
            string expected = @"
                IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenBothRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternFromStdinWith_H_OptionWhenBothRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-H" };
            string expected = @"
                IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenBothRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternFromStdinWith_o_OptionWhenBothRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-o" };
            string expected = @"
                ANDROID
                IPHONE
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenBothRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternFromStdinAndOneFileWhenOutputRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "file1" };
            string expected = @"
                file1:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternFromStdinWith_h_OptionAndOneFileWhenOutputRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-h", "file1" };
            string expected = @"
                IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternFromStdinWith_H_OptionAndOneFileWhenOutputRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-H", "file1" };
            string expected = @"
                file1:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternFromStdinWith_o_OptionAndOneFileWhenOutputRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-o", "file1" };
            string expected = @"
                file1:ANDROID
                file1:IPHONE
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternFromStdinAndTwoFilesWhenOutputRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "file1", "file2" };
            string expected = @"
                file1:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file2:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternFromStdinWith_h_OptionAndTwoFilesWhenOutputRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-h", "file1", "file2" };
            string expected = @"
                IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternFromStdinWith_H_OptionAndTwoFilesWhenOutputRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-H", "file1", "file2" };
            string expected = @"
                file1:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file2:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternFromStdinWith_o_OptionAndTwoFilesWhenOutputRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-o", "file1", "file2" };
            string expected = @"
                file1:ANDROID
                file1:IPHONE
                file2:HUAWEI
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileWhenBothRedirected()
        {
            string[] args = { "-f", "patterns.np" };
            string expected = @"
                :Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                :FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenBothRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileWith_h_OptionWhenBothRedirected()
        {
            string[] args = { "-f", "patterns.np", "-h" };
            string expected = @"
                :Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                :FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenBothRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileWith_H_OptionWhenBothRedirected()
        {
            string[] args = { "-f", "patterns.np", "-H" };
            string expected = @"
                :Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                :FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenBothRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileWith_o_OptionWhenBothRedirected()
        {
            string[] args = { "-f", "patterns.np", "-o" };
            string expected = @"
                :Phone:ANDROID
                :Phone:IPHONE
                :FirstWord:IS
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenBothRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileAndOneFileWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "file1" };
            string expected = @"
                file1:Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file1:FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileAndOneFileWhenBothRedirected()
        {
            string[] args = { "-f", "patterns.np", "file1" };
            string expected = @"
                file1:Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file1:FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenBothRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileWith_h_OptionAndOneFileWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "-h", "file1" };
            string expected = @"
                :Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                :FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileWith_H_OptionAndOneFileWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "-H", "file1" };
            string expected = @"
                file1:Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file1:FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileWith_o_OptionAndOneFileWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "-o", "file1" };
            string expected = @"
                file1:Phone:ANDROID
                file1:Phone:IPHONE
                file1:FirstWord:IS
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileAndTwoFilesWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "file1", "file2" };
            string expected = @"
                file1:Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file1:FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file2:Phone:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                file2:FirstWord:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileWith_h_OptionAndTwoFilesWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "-h", "file1", "file2" };
            string expected = @"
                :Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                :FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                :Phone:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                :FirstWord:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileWith_H_OptionAndTwoFilesWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "-H", "file1", "file2" };
            string expected = @"
                file1:Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file1:FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file2:Phone:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                file2:FirstWord:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task SearchPatternsFromFileWith_o_OptionAndTwoFilesWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "-o", "file1", "file2" };
            string expected = @"
                file1:Phone:ANDROID
                file1:Phone:IPHONE
                file1:FirstWord:IS
                file2:Phone:HUAWEI
                file2:FirstWord:CHINA
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #13
        [Test]
        public async Task SearchPatternFrom_e_OptionWhenBothRedirected()
        {
            string[] args = { "-e", "{'Android', 'iPhone', 'Huawei'}", "-o" };
            string expected = @"
                ANDROID
                IPHONE
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenBothRedirected(args, expected);
        }

        // Issue #13
        [Test]
        public async Task SearchPatternFrom_e_OptionAndOneFileWhenOutputRedirected()
        {
            string[] args = { "-e", "{'Android', 'iPhone', 'Huawei'}", "-o", "file1" };
            string expected = @"
                file1:ANDROID
                file1:IPHONE
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #13
        [Test]
        public async Task SearchPatternFrom_p_OptionWhenBothRedirected()
        {
            string[] args = { "-p", "#Phone = {'Android', 'iPhone', 'Huawei'};", "-o" };
            string expected = @"
                ANDROID
                IPHONE
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenBothRedirected(args, expected);
        }

        // Issue #13
        [Test]
        public async Task SearchPatternFrom_p_OptionAndOneFileWhenOutputRedirected()
        {
            string[] args = { "-p", "#Phone = {'Android', 'iPhone', 'Huawei'};", "-o", "file1" };
            string expected = @"
                file1:ANDROID
                file1:IPHONE
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #17
        [Test]
        public async Task SearchPatternIs_Any_And_o_OptionWhenOutputRedirected()
        {
            string[] args = { "Any", "-o", "file1" };
            string expected = @"
                file1:IS
                file1: 
                file1:ANDROID
                file1: 
                file1:OR
                file1: 
                file1:IPHONE
                file1: 
                file1:THE
                file1: 
                file1:BETTER
                file1: 
                file1:SMARTPHONE
                file1:?
                file1:" + '\0' + @"
                file1:CRLF
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #6
        [Test]
        public async Task SearchMatchesInFileWithCRLFLineBreaksWhenOutputRedirected()
        {
            ShouldHaveCRLFLineBreaks("file1");

            string[] args = { "-p", "#MultilineMatch = '?' + LineBreak + Word;", "file1" };
            string expected = @"
                file1:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?" + '\0' + @"CRLF
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #6
        [Test]
        public async Task SearchMatchesInFileWithLFLineBreaksWhenOutputRedirected()
        {
            ShouldHaveLFLineBreaks("file2");

            string[] args = { "-p", "#MultilineMatch = Word + LineBreak + Word;", "file2" };
            string expected = @"
                file2:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS" + '\0' + @"LF
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #6
        [Test]
        public async Task SearchMatchesInFileWithDifferentLineBreakTypesWhenOutputRedirected()
        {
            ShouldHaveAllLineBreakTypes("file3");

            string[] args = { "-p", "#MultilineMatch = Word + LineBreak + Word;", "file3" };
            string expected = @"
                file3:Yu said it" + '\0' + @"was the world's
                file3:was the world's" + '\0' + @"first 5G modem
                file3:first 5G modem" + '\0' + @"CRLF CR LF
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task MatchedPartBeginsWithNewLineWhenOutputRedirected()
        {
            string[] args = { "LineBreak + Word", "-o", "file1" };
            string expected = @"
                file1:" + '\0' + @"CRLF
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #30
        [Test]
        public async Task MatchedPartEndsWithLineBreakWhenOutputRedirected()
        {
            string[] args = { "Word+Any", "file*" };
            string expected = @"
                file3:Yu said it
                file3:Yu said it" + '\0' + @"
                file3:was the world's
                file3:was the world's" + '\0' + @"
                file3:first 5G modem
                file3:first 5G modem" + '\0' + @"
                file3:CRLF CR LF
                file2:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                file2:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS" + '\0' + @"
                file1:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #8
        [Test]
        public async Task RelativePathToAFileAsGlobPatternWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "file1" };
            string expected = @"
                file1:Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file1:FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #8
        [Test]
        public async Task RelativePathAsGlobPatternWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "file*" };
            string expected = @"
                file1:Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file1:FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file2:Phone:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                file2:FirstWord:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                file3:FirstWord:Yu said it
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #8
        [Test]
        public async Task AbsolutePathToAFileAsGlobPatternWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np" };
            string fullPath = Path.GetFullPath("file1");
            args = args.Append(fullPath).ToArray();
            string expected = @"
                " + fullPath + @":Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                " + fullPath + @":FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #16
        [Test]
        public async Task AbsolutePathAsGlobPatternWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np" };
            string fullPath = Path.GetFullPath("file1");
            string fullPathToDir = Path.GetDirectoryName(fullPath);
            args = args.Append(Path.Combine(fullPathToDir, "file*")).ToArray();
            string expected = @"
                " + fullPathToDir + Path.DirectorySeparatorChar + @"file1:Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                " + fullPathToDir + Path.DirectorySeparatorChar + @"file1:FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                " + fullPathToDir + Path.DirectorySeparatorChar + @"file2:Phone:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                " + fullPathToDir + Path.DirectorySeparatorChar + @"file2:FirstWord:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                " + fullPathToDir + Path.DirectorySeparatorChar + @"file3:FirstWord:Yu said it
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #16
        [Test]
        public async Task AbsolutePathAsGlobPatternAndRelativePathToAFileWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "file1" };
            string fullPath = Path.GetFullPath("file1");
            string fullPathToDir = Path.GetDirectoryName(fullPath);
            args = args.Append(Path.Combine(fullPathToDir, "file*")).ToArray();
            string expected = @"
                file1:Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file1:FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                " + fullPathToDir + Path.DirectorySeparatorChar + @"file2:Phone:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                " + fullPathToDir + Path.DirectorySeparatorChar + @"file2:FirstWord:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                " + fullPathToDir + Path.DirectorySeparatorChar + @"file3:FirstWord:Yu said it
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #8
        [Test]
        public async Task OneFileMatchesTwoGlobPatternsWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "f*", "file*" };
            string expected = @"
                file1:Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file1:FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file2:Phone:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                file2:FirstWord:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                file3:FirstWord:Yu said it
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #8
        [Test]
        public async Task RelativePathToAFileAsGlobPatternAndGlobPatternWhenOutputRedirected()
        {
            string[] args = { "-f", "patterns.np", "f*", "file1" };
            string expected = @"
                file1:Phone:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file1:FirstWord:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file2:Phone:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                file2:FirstWord:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                file3:FirstWord:Yu said it
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        // Issue #25
        [Test]
        public async Task SearchPatternFromStdinWhenNoFilesGivenWhenOutputRedirected()
        {
            Directory.CreateDirectory("dir1/dir2/dir3");
            File.Copy("file1", "dir1/file1", true);
            File.Copy("file1", "dir1/dir2/file1", true);
            File.Copy("file2", "dir1/dir2/dir3/file2", true);

            string[] args = { "{'Android', 'iPhone', 'Huawei'}" };
            string expected = @"
                file1:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                file2:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
                " + "patterns.np:#Phone = {'Android', 'iPhone', 'Huawei'};" + @"
                dir1" + Path.DirectorySeparatorChar + @"file1:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                dir1" + Path.DirectorySeparatorChar + @"dir2" + Path.DirectorySeparatorChar + @"file1:IS ANDROID OR IPHONE THE BETTER SMARTPHONE?
                dir1" + Path.DirectorySeparatorChar + @"dir2" + Path.DirectorySeparatorChar + @"dir3" + Path.DirectorySeparatorChar + @"file2:CHINA'S HUAWEI BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenOutputRedirected(args, expected);
        }

        [Test]
        public async Task ShouldResolvePatternReferencesBasedOnNegrepLocation()
        {
            string[] args = {"-p", "@require 'basic/Basic.np'; @search Basic.*;" };
            string expected = @"
                :Basic.Url:domain.com
            ";
            await NegrepTestsRunner.CompareDataInStdout(args, "domain.com", expected, isInputRedirected: true, isOutputRedirected: true);
        }

        // Issue #10
        [Test]
        public async Task WhenNoArgumentsInCommandLineShouldPrintUsage()
        {
            string[] args = { };
            string expected = @"
                Usage: negrep [OPTION]... 'EXPRESSION' [FILE]...
                Try 'negrep --help' for more information.
            ";
            FakeConsole console = new FakeConsole("");
            var negrep = new Negrep(args.ToList(), console, isStreamModeEnabled: false);
            await negrep.Execute();
            string actual = console.Stdout;

            Assert.That(actual.TrimEachLine(), Is.EqualTo(expected.TrimEachLine()));
        }

        [Test]
        public async Task ShowHelpInfo()
        {
            string[] args = { "--help" };
            string expected = @"
                Usage: negrep [OPTION]... 'EXPRESSION' [FILE]...

                e, expression         Obtain search target from an expression.
                p, pattern-package    Obtain search target from a pattern package.
                f, file               Obtain search target from FILE.
                o, only-matching      Print only the matched (non-empty) parts of a matching line, with each such part on a separate output line.
                H, with-filename      Print the file name for each match. This is the default when there is more than one file to search in.
                h, no-filename        Suppress the prefixing of file names on output. This is the default when there is only one file (or only standard input) to search in.
                help                  Output a usage message and exit.
                version               Display version information.
                files (pos. 0)        

                Examples:

                echo domain.com | negrep -p ""@require 'basic/Basic.np'; @search Basic.*;""
                cd <negrep installation directory>/examples
                negrep ""{'Android', 'IPhone'}"" example.txt
                negrep -f patterns.np example.txt
                echo Android or IPhone | negrep -e ""{'android', 'iphone'}""
            ";

            FakeConsole console = new FakeConsole("");
            var negrep = new Negrep(args.ToList(), console, isStreamModeEnabled: false);
            await negrep.Execute();
            string actual = console.Stdout;

            Assert.That(actual.TrimEachLine(), Is.EqualTo(expected.TrimEachLine()));
        }

        [Test]
        public async Task ShowVersion()
        {
            string[] args = { "--version" };
            string expected = "1.0.0";
            
            FakeConsole console = new FakeConsole("");
            var negrep = new Negrep(args.ToList(), console, isStreamModeEnabled: false);
            await negrep.Execute();
            string actual = console.Stdout;

            Assert.That(actual.TrimEachLine(), Is.EqualTo(expected.TrimEachLine()));
        }

        private void ShouldHaveCRLFLineBreaks(string filepath)
        {
            string fileContent = File.ReadAllText(filepath);
            var actual = Regex.Matches(fileContent, @"\r\n|\r|\n").All(lineBreak => lineBreak.Value == "\r\n");
            Assert.That(actual, Is.True);
        }

        private void ShouldHaveLFLineBreaks(string filepath)
        {
            string fileContent = File.ReadAllText(filepath);
            var actual = Regex.Matches(fileContent, @"\r\n|\r|\n").All(lineBreak => lineBreak.Value == "\n");
            Assert.That(actual, Is.True);
        }

        private void ShouldHaveAllLineBreakTypes(string filepath)
        {
            string fileContent = File.ReadAllText(filepath);
            var actual = Regex.Matches(fileContent, @"\r\n|\r|\n").Select(lineBreak => lineBreak.Value).ToArray();
            var expected = new[] { "\r", "\n", "\r\n" };
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
