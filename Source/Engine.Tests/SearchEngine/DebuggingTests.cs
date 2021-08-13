//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using static Nezaboodka.Nevod.Engine.Tests.TestHelper;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("BasicPatternsDebugging")]
    public class DebuggingTests
    {
        [TestMethod]
        public void BasicPatterns()
        {
            try
            {
                string dataDir = TestHelper.GetBasicPackageDirectory();
                string patterns = $"@require '{dataDir}/Basic/Basic.np'; @search Basic.*;";
                string text = "11";
                var lrc = new ResourceConsumption();
                var grc = new ResourceConsumption();
                var src = new ResourceConsumption();
                SearchPatternsAndCheckMatchesAndMeasureResourceConsumption(patterns, text, lrc, grc, src, "11");
                Console.WriteLine($"Package linking:\t{lrc.ElapsedMilliseconds} ms,\tallocated {lrc.TotalAllocatedBytes / 1_000_000 + 1} MB,\tused {lrc.ConsumedBytes / 1_000_000 + 1} MB");
                Console.WriteLine($"Package generation:\t{grc.ElapsedMilliseconds} ms,\tallocated {grc.TotalAllocatedBytes / 1_000_000 + 1} MB,\tused {grc.ConsumedBytes / 1_000_000 + 1} MB");
                Console.WriteLine($"Text search:\t{src.ElapsedMilliseconds} ms,\tallocated {src.TotalAllocatedBytes / 1_000_000 + 1} MB,\tused {src.ConsumedBytes / 1_000_000 + 1} MB");
            }
            finally
            {
                PackageCache.Global.Clear();
            }
        }

        [TestMethod]
        [Ignore]
        public void ShortNumberCandidateLimitExceeded()
        {
            string patterns = "#ShortNumber = Num + [0+ (?Space + ?'.' + ?Space) + Num];";
            string text = "1 2 3 4 5 6 7 8 9 10 11 12";
            SearchPatternsWithOptions(patterns, text);
        }
    }
}
