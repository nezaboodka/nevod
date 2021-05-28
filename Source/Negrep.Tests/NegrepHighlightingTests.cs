//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Nezaboodka.Nevod.Negrep.Tests
{
    [TestFixture]
    public class NegrepHighlightingTests
    {
        private readonly string FilenameColor = "{DarkGray}";
        private readonly string TagnameColor = "{Green}";
        private readonly string MatchColor = "{Red}";

        // Issue #24
        [Test]
        public async Task SearchPatternFromStdinHighlightedWhenInputRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}" };
            string expected = $@"
                IS {MatchColor}ANDROID{MatchColor} OR {MatchColor}IPHONE{MatchColor} THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenInputRedirected(args, expected);
        }

        // Issue #24
        [Test]
        public async Task SearchPatternFromStdin_o_HighlightedWhenInputRedirected()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-o" };
            string expected = $@"
                {MatchColor}ANDROID{MatchColor}
                {MatchColor}IPHONE{MatchColor}
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenInputRedirected(args, expected);
        }

        // Issue #24
        [Test]
        public async Task SearchPatternFromStdinAndOneFileHighlighted()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "file1" };
            string expected = $@"
                {FilenameColor}file1:{FilenameColor}
                IS {MatchColor}ANDROID{MatchColor} OR {MatchColor}IPHONE{MatchColor} THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdout(args, expected);
        }

        // Issue #24
        [Test]
        public async Task SearchPatternFromStdinAndOneFile_o_Highlighted()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-o", "file1" };
            string expected = $@"
                {FilenameColor}file1:{FilenameColor}
                {MatchColor}ANDROID{MatchColor}
                {MatchColor}IPHONE{MatchColor}
            ";
            await NegrepTestsRunner.CompareDataInStdout(args, expected);
        }

        // Issue #24
        [Test]
        public async Task SearchPatternFromStdinAndTwoFilesHighlighted()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "file1", "file2" };
            string expected = $@"
                {FilenameColor}file1:{FilenameColor}
                IS {MatchColor}ANDROID{MatchColor} OR {MatchColor}IPHONE{MatchColor} THE BETTER SMARTPHONE?
                {FilenameColor}file2:{FilenameColor}
                CHINA'S {MatchColor}HUAWEI{MatchColor} BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
            ";
            await NegrepTestsRunner.CompareDataInStdout(args, expected);
        }

        // Issue #24
        [Test]
        public async Task SearchPatternFromStdinAndTwoFiles_h_Highlighted()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-h", "file1", "file2" };
            string expected = $@"
                IS {MatchColor}ANDROID{MatchColor} OR {MatchColor}IPHONE{MatchColor} THE BETTER SMARTPHONE?
                CHINA'S {MatchColor}HUAWEI{MatchColor} BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
            ";
            await NegrepTestsRunner.CompareDataInStdout(args, expected);
        }

        // Issue #24
        [Test]
        public async Task SearchPatternFromStdinAndTwoFiles_o_Highlighted()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-o", "file1", "file2" };
            string expected = $@"
                {FilenameColor}file1:{FilenameColor}
                {MatchColor}ANDROID{MatchColor}
                {MatchColor}IPHONE{MatchColor}
                {FilenameColor}file2:{FilenameColor}
                {MatchColor}HUAWEI{MatchColor}
            ";
            await NegrepTestsRunner.CompareDataInStdout(args, expected);
        }

        // Issue #24
        [Test]
        public async Task PatternPackageFromStdinOneTagHighlightedWhenInputRedirected()
        {
            string[] args = { "-p", "#Phone = {'Android', 'iPhone', 'Huawei'};" };
            string expected = $@"
                IS {MatchColor}ANDROID{MatchColor} OR {MatchColor}IPHONE{MatchColor} THE BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenInputRedirected(args, expected);
        }

        // Issue #24
        [Test]
        public async Task PatternPackageFromStdinTwoTagsHighlightedWhenInputRedirected()
        {
            string[] args = { "-p", "#Phone = {'Android', 'iPhone', 'Huawei'}; #Article='the';" };
            string expected = $@"
                {TagnameColor}Phone:{TagnameColor}IS {MatchColor}ANDROID{MatchColor} OR {MatchColor}IPHONE{MatchColor} THE BETTER SMARTPHONE?
                {TagnameColor}Article:{TagnameColor}IS ANDROID OR IPHONE {MatchColor}THE{MatchColor} BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenInputRedirected(args, expected);
        }

        // Issue #24
        [Test]
        public async Task PatternPackageFromStdinTwoTags_o_HighlightedWhenInputRedirected()
        {
            string[] args = { "-p", "#Phone = {'Android', 'iPhone', 'Huawei'}; #Article='the';", "-o" };
            string expected = $@"
                {TagnameColor}Phone:{TagnameColor}{MatchColor}ANDROID{MatchColor}
                {TagnameColor}Phone:{TagnameColor}{MatchColor}IPHONE{MatchColor}
                {TagnameColor}Article:{TagnameColor}{MatchColor}THE{MatchColor}
            ";
            await NegrepTestsRunner.CompareDataInStdoutWhenInputRedirected(args, expected);
        }

        // Issue #24
        [Test]
        public async Task PatternPackageFromStdinTwoTagsAndOneFileHighlighted()
        {
            string[] args = { "-p", "#Phone = {'Android', 'iPhone', 'Huawei'}; #Article='the';", "file1" };
            string expected = $@"
                {FilenameColor}file1:{FilenameColor}
                {TagnameColor}Phone:{TagnameColor}IS {MatchColor}ANDROID{MatchColor} OR {MatchColor}IPHONE{MatchColor} THE BETTER SMARTPHONE?
                {TagnameColor}Article:{TagnameColor}IS ANDROID OR IPHONE {MatchColor}THE{MatchColor} BETTER SMARTPHONE?
            ";
            await NegrepTestsRunner.CompareDataInStdout(args, expected);
        }

        // Issue #24
        [Test]
        public async Task PatternPackageFromStdinTwoTagsAndOneFile_o_Highlighted()
        {
            string[] args = { "-p", "#Phone = {'Android', 'iPhone', 'Huawei'}; #Article='the';", "-o", "file1" };
            string expected = $@"
                {FilenameColor}file1:{FilenameColor}
                {TagnameColor}Phone:{TagnameColor}{MatchColor}ANDROID{MatchColor}
                {TagnameColor}Phone:{TagnameColor}{MatchColor}IPHONE{MatchColor}
                {TagnameColor}Article:{TagnameColor}{MatchColor}THE{MatchColor}
            ";
            await NegrepTestsRunner.CompareDataInStdout(args, expected);
        }

        // Issue #24
        [Test]
        public async Task PatternPackageFromStdinTwoTagsAndTwoFilesHighlighted()
        {
            string[] args = { "-p", "#Phone = {'Android', 'iPhone', 'Huawei'}; #Article='the';", "file1", "file2" };
            string expected = $@"
                {FilenameColor}file1:{FilenameColor}
                {TagnameColor}Phone:{TagnameColor}IS {MatchColor}ANDROID{MatchColor} OR {MatchColor}IPHONE{MatchColor} THE BETTER SMARTPHONE?
                {TagnameColor}Article:{TagnameColor}IS ANDROID OR IPHONE {MatchColor}THE{MatchColor} BETTER SMARTPHONE?
                {FilenameColor}file2:{FilenameColor}
                {TagnameColor}Phone:{TagnameColor}CHINA'S {MatchColor}HUAWEI{MatchColor} BOOKS RECORD SALES IN ITS SMARTPHONE BUSINESS
            ";
            await NegrepTestsRunner.CompareDataInStdout(args, expected);
        }

        // Issue #24
        [Test]
        public async Task PatternPackageFromStdinTwoTagsAndTwoFiles_o_Highlighted()
        {
            string[] args = { "-p", "#Phone = {'Android', 'iPhone', 'Huawei'}; #Article='the';", "-o", "file1", "file2" };
            string expected = $@"
                {FilenameColor}file1:{FilenameColor}
                {TagnameColor}Phone:{TagnameColor}{MatchColor}ANDROID{MatchColor}
                {TagnameColor}Phone:{TagnameColor}{MatchColor}IPHONE{MatchColor}
                {TagnameColor}Article:{TagnameColor}{MatchColor}THE{MatchColor}
                {FilenameColor}file2:{FilenameColor}
                {TagnameColor}Phone:{TagnameColor}{MatchColor}HUAWEI{MatchColor}
            ";
            await NegrepTestsRunner.CompareDataInStdout(args, expected);
        }
    }
}
