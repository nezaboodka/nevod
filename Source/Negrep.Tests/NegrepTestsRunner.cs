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
    public static class NegrepTestsRunner
    {
        private const string StdinValue = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE?";

        public static async Task CompareDataInStdoutWhenInputRedirected(string[] args, string expected)
        {
            await CompareDataInStdout(args, expected, isInputRedirected: true, isOutputRedirected: false);
        }

        public static async Task CompareDataInStdoutWhenOutputRedirected(string[] args, string expected)
        {
            await CompareDataInStdout(args, expected, isInputRedirected: false, isOutputRedirected: true);
        }

        public static async Task CompareDataInStdoutWhenBothRedirected(string[] args, string expected)
        {
            await CompareDataInStdout(args, expected, isInputRedirected: true, isOutputRedirected: true);
        }

        public static async Task CompareDataInStdout(string[] args, string expected, bool isInputRedirected = false,
            bool isOutputRedirected = false)
        {
            List<string> arguments = args.ToList();
            await RunNegrep(arguments, expected, isStreamModeEnabled: false, isInputRedirected, isOutputRedirected);
            await RunNegrep(arguments, expected, isStreamModeEnabled: true, isInputRedirected, isOutputRedirected);
        }

        private static async Task RunNegrep(List<string> args, string expected, bool isStreamModeEnabled,
            bool isInputRedirected, bool isOutputRedirected)
        {
            FakeConsole console = new FakeConsole(StdinValue, isInputRedirected, isOutputRedirected);
            var negrep = new Negrep(args, console, isStreamModeEnabled);
            await negrep.Execute();
            string actual = console.Stdout;

            var sortedActual = actual.SortAllLinesByHashCode();
            var sortedExpected = expected.TrimEachLine().AddLineBreak().SortAllLinesByHashCode();
            Assert.That(sortedActual, Is.EqualTo(sortedExpected));
        }
    }
}
