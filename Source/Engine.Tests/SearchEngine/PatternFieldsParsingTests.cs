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
    public class PatternFieldsParsingTests
    {
        [TestMethod]
        public void PatternReferenceWithoutFieldsParsing()
        {
            string patterns =
                "P1(X) = X: Symbol;\n" +
                "#P2(Y, Z) = Y: P1 + Z: Symbol;";
            TestParseAndToString(patterns);

        }
        [TestMethod]
        public void PatternReferenceWithFieldsParsing()
        {
            string patterns =
                "P1(X) = X: Symbol;\n" +
                "#P2(Y, Z) = P1(Y: X) + Z: Symbol;";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void PatternReferenceWithoutFieldsToPatternWithFieldReferenceParsing()
        {
            string patterns =
                "P1(X) = X: Symbol + X;\n" +
                "#P2(Y, Z) = Y: P1 + Z: Symbol;";
           TestParseAndToString(patterns);
        }

        [TestMethod]
        public void PatternReferenceWithFieldsToPatternWithFieldReferenceParsing()
        {
            string patterns =
                "P1(X) = X: Symbol + X;\n" +
                "#P2(Y, Z) = P1(Y: X) + Z: Symbol;";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void TwoPatternReferencesWithoutFieldsToPatternWithFieldReferenceParsing()
        {
            string patterns =
                "P1(X) = X: Symbol + X;\n" +
                "#P2(Y, Z) = Y: P1 + Z: P1;";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void ExtractionOfPatternReferenceWithFieldsToPatternWithFieldReferenceParsing()
        {
            string patterns =
                "P1(X) = X: Symbol + X;\n" +
                "#P2(U, W, Y, Z) = U: P1(W: X) + Y: P1(Z: X);";
            TestParseAndToString(patterns);
        }
    }
}
