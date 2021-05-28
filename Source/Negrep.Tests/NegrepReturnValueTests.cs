//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Nezaboodka.Nevod.Negrep.Tests
{
    [TestFixture]
    public class NegrepReturnValueTests
    {
        private const string StdinValue = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE?";

        // Issue #10
        [Test]
        public async Task WhenNoArgumentsInCommandLineShouldReturnZero()
        {
            string[] args = { };
            await ShouldReturnZero(args);
        }

        // Issue #10
        [Test]
        public async Task WhenEmptyStringPassedShouldReturnOne()
        {
            string[] args = { "" };
            await ShouldReturnOne(args);
        }

        [Test]
        public async Task WhenHelpRequestedShouldReturnZero()
        {
            string[] args = { "--help" };
            await ShouldReturnZero(args);
        }

        [Test]
        public async Task WhenNothingToMatchOnButOptionIsProvidedShouldReturnZero()
        {
            string[] args = { "-o" };
            await ShouldReturnZero(args);
        }

        [Test]
        public async Task WhenUnknownOptionProvidedShouldReturnZero()
        {
            string[] args = { "--unknown-option" };
            await ShouldReturnOne(args);
        }

        [Test]
        public async Task WhenSyntaxExceptionThrownShouldReturnZero()
        {
            string[] args = { "Number" };
            await ShouldReturnOne(args);
        }

        [Test]
        public async Task WhenNotFoundMatchesShouldReturnOne()
        {
            string[] args = { "{'Dell', 'HP', 'Acer'}", "-o", "file1", "file2" };
            await ShouldReturnOne(args);
        }

        [Test]
        public async Task WhenMatchesFoundShouldReturnZero()
        {
            string[] args = { "{'Android', 'iPhone', 'Huawei'}", "-o", "file1", "file2" };
            await ShouldReturnZero(args);
        }

        // Issue #13
        [Test]
        public async Task WhenExpressionExpectedButPatternPackageProvidedShouldReturnOne()
        {
            string[] args = { "#Phone = {'Android', 'iPhone', 'Huawei'};" };
            await ShouldReturnOne(args);
        }

        // Issue #13
        [Test]
        public async Task WhenExpressionFromOptionExpectedButPatternPackageProvidedShouldReturnOne()
        {
            string[] args = { "-e", "#Phone = {'Android', 'iPhone', 'Huawei'};" };
            await ShouldReturnOne(args);
        }

        // Issue #13
        [Test]
        public async Task WhenPatternPackageExpectedButExpressionProvidedShouldReturnOne()
        {
            string[] args = { "-p", "{'Android', 'iPhone', 'Huawei'}" };
            await ShouldReturnOne(args);
        }

        // Issue #18
        [Test]
        public async Task WhenInputFileIsAlsoTheOutputShouldReturnOne()
        {
            string[] args = { "Any", "in_out_file" };
            using (FileStream fs = File.Open("in_out_file", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await ShouldReturnOne(args);
            }
            File.Delete("in_out_file");
        }

        private async Task ShouldReturnZero(string[] args)
        {
            List<string> arguments = args.ToList();
            await RunNegrep(arguments, expected: 0);
        }

        private async Task ShouldReturnOne(string[] args)
        {
            List<string> arguments = args.ToList();
            await RunNegrep(arguments, expected: 1);
        }

        private async Task RunNegrep(List<string> args, int expected)
        {
            FakeConsole console = new FakeConsole(StdinValue);
            var negrep = new Negrep(args, console, isStreamModeEnabled: false);
            int actual = await negrep.Execute();
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
