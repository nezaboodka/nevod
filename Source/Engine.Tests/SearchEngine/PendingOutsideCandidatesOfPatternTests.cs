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
    [TestCategory("SearchEngine"), TestCategory("PendingOutsideCandidatesOfPattern")]
    public class PendingOutsideCandidatesOfPatternTests
    {
        [TestMethod]
        public void ThreeOuterPatterns()
        {
            PendingOutsideCandidatesOfOuterPattern uut = new PendingOutsideCandidatesOfOuterPattern();
            // Проверка упорядоченности OuterPattern по Start.TokenNumber
            PatternCandidate first = CreateOuterPatternCandidate(10, 20);
            PatternCandidate second = CreateOuterPatternCandidate(30, 40);
            PatternCandidate third = CreateOuterPatternCandidate(5, 15);

            uut.AddOuterPatternCandidate(first);
            uut.AddOuterPatternCandidate(second);

            CollectionAssert.AreEqual(new[] { first, second }, uut.MatchedCandidatesOfOuterPatterns);

            uut.AddOuterPatternCandidate(third);

            CollectionAssert.AreEqual(new[] { third, first, second }, uut.MatchedCandidatesOfOuterPatterns);
        }

        [TestMethod]
        public void ThreePendingCandidates()
        {
            PendingOutsideCandidatesOfOuterPattern uut = new PendingOutsideCandidatesOfOuterPattern();
            // Проверка упорядоченности OutsideCandidate по End.TokenNumber
            OutsideCandidate first = CreatePendingOutsideCandidate(10, 20);
            OutsideCandidate second = CreatePendingOutsideCandidate(30, 40);
            OutsideCandidate third = CreatePendingOutsideCandidate(5, 15);

            uut.AddPendingCandidate(first);
            uut.AddPendingCandidate(second);

            CollectionAssert.AreEqual(new[] { first, second }, uut.PendingCandidates);

            uut.AddPendingCandidate(third);

            CollectionAssert.AreEqual(new[] { third, first, second }, uut.PendingCandidates);
        }

        // Internal

        private PatternCandidate CreateOuterPatternCandidate(int startTokenNumber, int endTokenNumber)
        {
            var result = new PatternCandidate(null);
            result.Start = new TextLocation(startTokenNumber, -1, -1);
            result.End = new TextLocation(endTokenNumber, -1, -1);
            return result;
        }

        private OutsideCandidate CreatePendingOutsideCandidate(int startTokenNumber, int endTokenNumber)
        {
            var result = new OutsideCandidate(null);
            result.Start = new TextLocation(startTokenNumber, -1, -1);
            result.End = new TextLocation(endTokenNumber, -1, -1);
            return result;
        }
    }
}
