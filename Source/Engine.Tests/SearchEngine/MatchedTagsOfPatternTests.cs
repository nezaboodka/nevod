//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static Nezaboodka.Nevod.Engine.Tests.TestHelper;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("SearchEngine"), TestCategory("MatchedTagsOfPattern")]
    public class MatchedTagsOfPatternTests
    {
        [TestMethod]
        public void TwoNonOverlappingTags()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: false);
            MatchedTag first = CreateTestMatchedTag(1, 5);
            MatchedTag second = CreateTestMatchedTag(7, 10);

            uut.Add(first);
            uut.Add(second);

            CollectionAssert.AreEquivalent(new[] { first, second }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoNonOverlappingTagsWithSelfOverlaps()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: true);
            MatchedTag first = CreateTestMatchedTag(1, 5);
            MatchedTag second = CreateTestMatchedTag(7, 10);

            uut.Add(first);
            uut.Add(second);

            CollectionAssert.AreEquivalent(new[] { first, second }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoTagsWithSameStartShorterFirst()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: false);
            MatchedTag toBeGone = CreateTestMatchedTag(1, 5);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 10);

            uut.Add(toBeGone);
            uut.Add(toBeLeft);

            CollectionAssert.AreEquivalent(new[] { toBeLeft }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoTagsWithSameStartShorterFirstWithSelfOverlaps()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: true);
            MatchedTag toBeGone = CreateTestMatchedTag(1, 5);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 10);

            uut.Add(toBeGone);
            uut.Add(toBeLeft);

            CollectionAssert.AreEquivalent(new[] { toBeGone, toBeLeft }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoTagsWithSameStartLongerFirst()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: false);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 10);
            MatchedTag toBeGone = CreateTestMatchedTag(1, 5);

            uut.Add(toBeLeft);
            uut.Add(toBeGone);

            CollectionAssert.AreEquivalent(new[] { toBeLeft }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoTagsWithSameStartLongerFirstWithSelfOverlaps()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: true);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 10);
            MatchedTag toBeGone = CreateTestMatchedTag(1, 5);

            uut.Add(toBeLeft);
            uut.Add(toBeGone);

            CollectionAssert.AreEquivalent(new[] { toBeGone, toBeLeft }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoTagsWithSameEndShorterFirst()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: false);
            MatchedTag toBeGone = CreateTestMatchedTag(5, 10);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 10);

            uut.Add(toBeGone);
            uut.Add(toBeLeft);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeLeft }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoTagsWithSameEndShorterFirstWithSelfOverlaps()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: true);
            MatchedTag toBeGone = CreateTestMatchedTag(5, 10);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 10);

            uut.Add(toBeGone);
            uut.Add(toBeLeft);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeLeft, toBeGone }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoTagsWithSameEndLongerFirst()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: false);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 10);
            MatchedTag toBeGone = CreateTestMatchedTag(5, 10);

            uut.Add(toBeLeft);
            uut.Add(toBeGone);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeLeft }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoTagsWithSameEndLongerFirstWithSelfOverlaps()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: true);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 10);
            MatchedTag toBeGone = CreateTestMatchedTag(5, 10);

            uut.Add(toBeLeft);
            uut.Add(toBeGone);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeLeft, toBeGone }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoOverlappingTags()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: false);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 5);
            MatchedTag toBeGone = CreateTestMatchedTag(3, 10);

            uut.Add(toBeLeft);
            uut.Add(toBeGone);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeLeft }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoOverlappingTagsWithSelfOverlaps()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: true);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 5);
            MatchedTag toBeGone = CreateTestMatchedTag(3, 10);

            uut.Add(toBeLeft);
            uut.Add(toBeGone);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeLeft, toBeGone }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoOverlappingTagsAnotherOrder()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: false);
            MatchedTag toBeGone = CreateTestMatchedTag(3, 10);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 5);

            uut.Add(toBeGone);
            uut.Add(toBeLeft);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeLeft }, uut.MatchedTags);
        }

        [TestMethod]
        public void TwoOverlappingTagsAnotherOrderWithSelfOverlaps()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: true);
            MatchedTag toBeGone = CreateTestMatchedTag(3, 10);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 5);

            uut.Add(toBeGone);
            uut.Add(toBeLeft);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeLeft, toBeGone }, uut.MatchedTags);
        }

        [TestMethod]
        public void ThreeOverlappingTags()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: false);
            MatchedTag toBeLeft1 = CreateTestMatchedTag(1, 5);
            MatchedTag toBeGone = CreateTestMatchedTag(3, 12);
            MatchedTag toBeLeft2 = CreateTestMatchedTag(10, 15);

            uut.Add(toBeLeft1);
            uut.Add(toBeGone);
            uut.Add(toBeLeft2);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeLeft1, toBeLeft2 }, uut.MatchedTags);
        }

        [TestMethod]
        public void ThreeOverlappingTagsWithSelfOverlaps()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: true);
            MatchedTag toBeLeft1 = CreateTestMatchedTag(1, 5);
            MatchedTag toBeGone = CreateTestMatchedTag(3, 12);
            MatchedTag toBeLeft2 = CreateTestMatchedTag(10, 15);

            uut.Add(toBeLeft1);
            uut.Add(toBeGone);
            uut.Add(toBeLeft2);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeLeft1, toBeGone, toBeLeft2 }, uut.MatchedTags);
        }

        [TestMethod]
        public void ThreeOverlappingTagsAndTwoWithSameStart()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: false);
            MatchedTag toBeGone1 = CreateTestMatchedTag(1, 5);
            MatchedTag toBeGone2 = CreateTestMatchedTag(3, 12);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 9);

            uut.Add(toBeGone1);
            uut.Add(toBeGone2);
            uut.Add(toBeLeft);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeLeft }, uut.MatchedTags);
        }

        [TestMethod]
        public void ThreeOverlappingTagsAndTwoWithSameStartWithSelfOverlaps()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: true);
            MatchedTag toBeGone1 = CreateTestMatchedTag(1, 5);
            MatchedTag toBeGone2 = CreateTestMatchedTag(3, 12);
            MatchedTag toBeLeft = CreateTestMatchedTag(1, 9);

            uut.Add(toBeGone1);
            uut.Add(toBeGone2);
            uut.Add(toBeLeft);
            uut.TryRemoveOverlaps(int.MaxValue);

            CollectionAssert.AreEquivalent(new[] { toBeGone1, toBeLeft, toBeGone2 }, uut.MatchedTags);
        }

        [TestMethod]
        public void ThreeOverlappingTagsComplex()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: false);
            MatchedTag first = CreateTestMatchedTag(5, 15);
            MatchedTag second = CreateTestMatchedTag(12, 20);
            MatchedTag third = CreateTestMatchedTag(1, 10);

            uut.Add(first);
            uut.Add(second);
            CollectionAssert.AreEqual(new[] { first, second }, uut.MatchedTags);

            uut.Add(third);
            CollectionAssert.AreEqual(new[] { third, first, second }, uut.MatchedTags);

            uut.TryRemoveOverlaps(int.MaxValue);
            CollectionAssert.AreEqual(new[] { third, second }, uut.MatchedTags);
        }

        [TestMethod]
        public void ThreeOverlappingTagsComplexWithSelfOverlaps()
        {
            MatchedTagsOfPattern uut = new MatchedTagsOfPattern(null, selfOverlapping: true);
            MatchedTag first = CreateTestMatchedTag(5, 15);
            MatchedTag second = CreateTestMatchedTag(12, 20);
            MatchedTag third = CreateTestMatchedTag(1, 10);

            uut.Add(first);
            uut.Add(second);
            CollectionAssert.AreEqual(new[] { first, second }, uut.MatchedTags);

            uut.Add(third);
            CollectionAssert.AreEqual(new[] { third, first, second }, uut.MatchedTags);

            uut.TryRemoveOverlaps(int.MaxValue);
            CollectionAssert.AreEqual(new[] { third, first, second }, uut.MatchedTags);
        }

        // Internal

        private MatchedTag CreateTestMatchedTag(int startTokenNumber, int endTokenNumber)
        {
            var startLocation = new TextLocation(startTokenNumber, -1, -1);
            var endLocation = new TextLocation(endTokenNumber, -1, -1);
            var result = new MatchedTag(timestamp: 0, patternFullName: null, extractions: null,
                textSource: null, startLocation, endLocation);
            return result;
        }
    }
}
