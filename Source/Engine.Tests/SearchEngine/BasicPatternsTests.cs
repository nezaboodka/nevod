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
    [TestCategory("BasicPatterns")]
    public class BasicPatternsTests
    {
        [TestMethod]
        [Timeout(5 * 1000)]    // 5 seconds
        public void PhoneNumberLikeSequence()
        {
            string dataDir = TestHelper.GetBasicPackageDirectory();
            string patterns = $"@require '{dataDir}/Basic/Basic.np'; @search Basic.*;";
            string text = "eqweqw 11 212 323 23 ";
            SearchPatternsAndCheckMatches(patterns, text, "11", "212", "323", "23", "11 212 323 23");
        }

        [TestMethod]
        [Timeout(5 * 1000)]    // 5 seconds
        public void TimeInTextWithQuote()
        {
            string dataDir = TestHelper.GetBasicPackageDirectory();
            string patterns = $"@require '{dataDir}/Basic/Basic.np'; @search Basic.Time;";
            string text = "I'll be back at 7";
            SearchPatternsAndCheckMatches(patterns, text, "7");
        }

        [TestMethod]
        [Timeout(5 * 1000)]    // 5 seconds
        public void ReuseGlobalPackageCache()
        {
            string dataDir = TestHelper.GetBasicPackageDirectory();
            try
            {
                string patterns = $"@require '{dataDir}/Basic/Basic.np'; @search Basic.*;";
                string text = "eqweqw 11 212 323 23 ";
                var lrc = new ResourceConsumption();
                var grc = new ResourceConsumption();
                var src = new ResourceConsumption();
                SearchPatternsAndCheckMatchesAndMeasureResourceConsumption(patterns, text, lrc, grc, src, "11", "212", "323", "23", "11 212 323 23");

                string patterns2 = $"@require '{dataDir}/Basic/Basic.np'; @search Basic.Time;";
                string text2 = "I'll be back at 7";
                var lrc2 = new ResourceConsumption();
                var grc2 = new ResourceConsumption();
                var src2 = new ResourceConsumption();
                SearchPatternsAndCheckMatchesAndMeasureResourceConsumption(patterns2, text2, lrc2, grc2, src2, "7");
                Assert.IsTrue(lrc2.ConsumedBytes * 10 < lrc.ConsumedBytes, "Memory consumption degraded");
                Assert.IsTrue(lrc2.ElapsedMilliseconds * 10 < lrc.ElapsedMilliseconds, "Performance degraded");
                Assert.IsTrue(grc2.ConsumedBytes * 10 < grc.ConsumedBytes, "Memory consumption degraded");
                Assert.IsTrue(grc2.ElapsedMilliseconds * 5 < grc.ElapsedMilliseconds, "Performance degraded");
            }
            finally
            {
                PackageCache.Global.Clear();
            }
        }

        [TestMethod]
        [Timeout(5 * 1000)]    // 5 seconds
        public void DebugGlobalPackageCache()
        {
            string dataDir = TestHelper.GetBasicPackageDirectory();
            try
            {
                var lrc = new ResourceConsumption();
                var grc = new ResourceConsumption();
                var src = new ResourceConsumption();

                string patterns = $"@require '{dataDir}/Basic/Basic.np'; @search Basic.*;";
                string text = "eqweqw 11 212 323 23 ";
                SearchPatternsAndCheckMatchesAndMeasureResourceConsumption(patterns, text, lrc, grc, src, "11", "212", "323", "23", "11 212 323 23");
                Console.WriteLine($"Package linking: {lrc.ElapsedMilliseconds} ms, allocated {lrc.TotalAllocatedBytes / 1_000_000 + 1} MB, used {lrc.ConsumedBytes / 1_000_000 + 1} MB");
                Console.WriteLine($"Package generation: {grc.ElapsedMilliseconds} ms, allocated {grc.TotalAllocatedBytes / 1_000_000 + 1} MB, used {grc.ConsumedBytes / 1_000_000 + 1} MB");
                Console.WriteLine($"Text search: {src.ElapsedMilliseconds} ms, allocated {src.TotalAllocatedBytes / 1_000_000 + 1} MB, used {src.ConsumedBytes / 1_000_000 + 1} MB");

                string patterns2 = $"@require '{dataDir}/Basic/Basic.np'; @search Basic.Time;";
                string text2 = "I'll be back at 7";
                SearchPatternsAndCheckMatchesAndMeasureResourceConsumption(patterns2, text2, lrc, grc, src, "7");
                Console.WriteLine($"Package linking: {lrc.ElapsedMilliseconds} ms, allocated {lrc.TotalAllocatedBytes / 1_000_000 + 1} MB, used {lrc.ConsumedBytes / 1_000_000 + 1} MB");
                Console.WriteLine($"Package generation: {grc.ElapsedMilliseconds} ms, allocated {grc.TotalAllocatedBytes / 1_000_000 + 1} MB, used {grc.ConsumedBytes / 1_000_000 + 1} MB");
                Console.WriteLine($"Text search: {src.ElapsedMilliseconds} ms, allocated {src.TotalAllocatedBytes / 1_000_000 + 1} MB, used {src.ConsumedBytes / 1_000_000 + 1} MB");
            }
            finally
            {
                PackageCache.Global.Clear();
            }
        }

        [TestMethod]
        [Timeout(5 * 1000)]    // 5 seconds
        public void DateAndIntegerAndPhoneNumberLikeSequence()
        {
            string dataDir = TestHelper.GetBasicPackageDirectory();
            string patterns = $"@require '{dataDir}/Basic/Basic.np'; @search Basic.*;";
            string text = "eqweqw apr 11 212 323 23 412 1232131 2131313 1233123 32112312 xsdddd 1212";
            SearchPatternsAndCheckMatches(patterns, text, "apr 11", "11", "212", "323", "23", "412", "1232131",
                "2131313", "1233123", "32112312", "1212", "212 323 23 412 1232131 2131313 1233123 32112312");
        }
    }
}
