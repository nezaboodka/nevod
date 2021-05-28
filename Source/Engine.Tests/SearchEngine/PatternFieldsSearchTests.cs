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
    [TestCategory("PatternPackageSearch")]
    public class PatternFieldsSearchTests
    {
        [TestMethod]
        public void PatternReferenceWithoutFieldsSimplest()
        {
            string patterns = @"
                P1(X) = X: Symbol;
                #P2(Y) = Y: P1;
            ";
            string text = "@ *";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P2.Y", new[] { "@" }),
                ("P2.Y", new[] { "*" })
            );
        }

        [TestMethod]
        public void PatternReferenceWithoutFields()
        {
            string patterns = @"
                P1(X) = X: Symbol;
                #P2(Y, Z) = Y: P1 + Z: Symbol;
            ";
            string text = "@# ^*";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P2.Y", new[] { "@" }),
                ("P2.Z", new[] { "#" }),
                ("P2.Y", new[] { "^" }),
                ("P2.Z", new[] { "*" })
            );
        }

        [TestMethod]
        public void PatternReferenceWithFields()
        {
            string patterns = @"
                P1(X) = X: Symbol;
                #P2(Y, Z) = P1(Y: X) + Z: Symbol;
            ";
            string text = "@# ^*";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P2.Y", new[] { "@" }),
                ("P2.Z", new[] { "#" }),
                ("P2.Y", new[] { "^" }),
                ("P2.Z", new[] { "*" })
            );
        }

        [TestMethod]
        public void PatternReferenceWithoutFieldsToPatternWithFieldReference()
        {
            string patterns = @"
                P1(X) = X: Symbol + X;
                #P2(Y, Z) = Y: P1 + Z: Symbol;
            ";
            string text = "@@* ##^";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P2.Y", new[] { "@@" }),
                ("P2.Z", new[] { "*" }),
                ("P2.Y", new[] { "##" }),
                ("P2.Z", new[] { "^" })
            );
        }

        [TestMethod]
        public void PatternReferenceWithFieldsToPatternWithFieldReference()
        {
            string patterns = @"
                P1(X) = X: Symbol + X;
                #P2(Y, Z) = P1(Y: X) + Z: Symbol;
            ";
            string text = "@@* ##^";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P2.Y", new[] { "@" }),
                ("P2.Z", new[] { "*" }),
                ("P2.Y", new[] { "#" }),
                ("P2.Z", new[] { "^" })
            );
        }

        [TestMethod]
        public void TwoPatternReferencesWithoutFieldsToPatternWithFieldReference()
        {
            string patterns = @"
                P1(X) = X: Symbol + X;
                #P2(Y, Z) = Y: P1 + Z: P1;
            ";
            // #P2(Y, Z, ~P1.X.1, ~P1.X.2) = Y: P1(P1.X.1: X) + Z: P1(P1.X.2: X);";
            string text = "@@** ##^^";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P2.Y", new[] { "@@" }),
                ("P2.Z", new[] { "**" }),
                ("P2.Y", new[] { "##" }),
                ("P2.Z", new[] { "^^" })
            );
        }

        [TestMethod]
        public void ExtractionOfPatternReferenceWithFieldsToPatternWithFieldReference()
        {
            string patterns = @"
                P1(X) = X: Symbol + X;
                #P2(U, W, Y, Z) = U: P1(W: X) + Y: P1(Z: X);
            ";
            string text = "@@**";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P2.U", new[] { "@@" }),
                ("P2.W", new[] { "@" }),
                ("P2.Y", new[] { "**" }),
                ("P2.Z", new[] { "*" })
            );
        }
    }
}
