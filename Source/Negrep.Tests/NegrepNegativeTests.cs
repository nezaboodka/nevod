//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using NUnit.Framework;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nezaboodka.Nevod.Negrep.Tests
{
    [TestFixture]
    public class NegrepNegativeTests
    {
        private const string StdinValue = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE?";

        // Issue #8
        [Test]
        public async Task OneGlobPatternOfNonexistentFiles()
        {
            string[] args = { "-f", "patterns.np", "*.nonexistent" };
            string expected = @"
                *.nonexistent: no such file or directory
            ";
            await CompareExpectedDataToActualStoredInStderr(args, expected);
        }

        [Test]
        public async Task PathToNonexistentFile()
        {
            string[] args = { "-f", "patterns.np", "nonexistent_file" };
            string expected = @"
                nonexistent_file: no such file or directory
            ";
            await CompareExpectedDataToActualStoredInStderr(args, expected);
        }

        [Test]
        public async Task PathToNonexistentFileWithPatterns()
        {
            string[] args = { "-f", "nonexistent_file.np" };
            string expected = @"
                nonexistent_file.np: no such file or directory
            ";
            await CompareExpectedDataToActualStoredInStderr(args, expected);
        }

        private async Task CompareExpectedDataToActualStoredInStderr(string[] args, string expected)
        {
            List<string> arguments = args.ToList();
            await RunNegrep(arguments, expected);
        }

        private static async Task RunNegrep(List<string> args, string expected)
        {
            FakeConsole console = new FakeConsole(StdinValue);
            var negrep = new Negrep(args, console, isStreamModeEnabled: false);
            await negrep.Execute();
            string actual = console.Stderr;

            var sortedActual = actual.SortAllLinesByHashCode();
            var sortedExpected = expected.TrimEachLine().AddLineBreak().SortAllLinesByHashCode();
            Assert.That(sortedActual, Is.EqualTo(sortedExpected));
        }
    }
}
