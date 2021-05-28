//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("ExpressionIndex")]
    public class ExpressionIndexTests
    {
        [TestMethod]
        public void SingleTokenExpressionIndexSelectMatchingExpressions()
        {
            var ciIndex1 = new SingleTokenExpressionIndex(TestData.CaseInsensitiveTokenExpression1);
            var ciIndex2 = new SingleTokenExpressionIndex(TestData.CaseInsensitiveTokenExpression2);
            var csIndex1 = new SingleTokenExpressionIndex(TestData.CaseSensitiveTokenExpression1);
            var csIndex2 = new SingleTokenExpressionIndex(TestData.CaseSensitiveTokenExpression2);
            var tkIndex1 = new SingleTokenExpressionIndex(TestData.TokenKindExpression1);
            var tkIndex2 = new SingleTokenExpressionIndex(TestData.TokenKindExpression2);
            var wpIndex1 = new SingleTokenExpressionIndex(TestData.WordPrefixExpression1);
            var wpIndex2 = new SingleTokenExpressionIndex(TestData.WordPrefixExpression2);
            var waIndex1 = new SingleTokenExpressionIndex(TestData.WordPrefixAttributesExpression1);
            var waIndex2 = new SingleTokenExpressionIndex(TestData.TokenKindAttributesExpression1);

            var token = new Token(TestData.Token1Kind, TestData.Word1Class, TestData.Token1Text, location: null);

            var matchingExpressions = new HashSet<TokenExpression>();
            ciIndex1.SelectMatchingExpressions(token, null, matchingExpressions);
            Assert.AreEqual(1, matchingExpressions.Count());
            Assert.IsTrue(matchingExpressions.Contains(TestData.CaseInsensitiveTokenExpression1));
            matchingExpressions.Clear();

            csIndex1.SelectMatchingExpressions(token, null, matchingExpressions);
            Assert.AreEqual(1, matchingExpressions.Count());
            Assert.IsTrue(matchingExpressions.Contains(TestData.CaseSensitiveTokenExpression1));
            matchingExpressions.Clear();

            tkIndex1.SelectMatchingExpressions(token, null, matchingExpressions);
            Assert.AreEqual(1, matchingExpressions.Count());
            Assert.IsTrue(matchingExpressions.Contains(TestData.TokenKindExpression1));
            matchingExpressions.Clear();

            wpIndex1.SelectMatchingExpressions(token, null, matchingExpressions);
            Assert.AreEqual(1, matchingExpressions.Count());
            Assert.IsTrue(matchingExpressions.Contains(TestData.WordPrefixExpression1));
            matchingExpressions.Clear();

            waIndex1.SelectMatchingExpressions(token, null, matchingExpressions);
            Assert.AreEqual(1, matchingExpressions.Count());
            Assert.IsTrue(matchingExpressions.Contains(TestData.WordPrefixAttributesExpression1));
            matchingExpressions.Clear();

            waIndex2.SelectMatchingExpressions(token, null, matchingExpressions);
            Assert.AreEqual(1, matchingExpressions.Count());
            Assert.IsTrue(matchingExpressions.Contains(TestData.TokenKindAttributesExpression1));
            matchingExpressions.Clear();
        }

        [TestMethod]
        public void TokenExpressionIndexSelectMatchingExpressions()
        {
            var ciIndex1 = new SingleTokenExpressionIndex(TestData.CaseInsensitiveTokenExpression1);
            var ciIndex2 = new SingleTokenExpressionIndex(TestData.CaseInsensitiveTokenExpression2);
            var csIndex1 = new SingleTokenExpressionIndex(TestData.CaseSensitiveTokenExpression1);
            var csIndex2 = new SingleTokenExpressionIndex(TestData.CaseSensitiveTokenExpression2);
            var tkIndex1 = new SingleTokenExpressionIndex(TestData.TokenKindExpression1);
            var tkIndex2 = new SingleTokenExpressionIndex(TestData.TokenKindExpression2);
            var wpIndex1 = new SingleTokenExpressionIndex(TestData.WordPrefixExpression1);
            var wpIndex2 = new SingleTokenExpressionIndex(TestData.WordPrefixExpression2);
            var waIndex1 = new SingleTokenExpressionIndex(TestData.WordPrefixAttributesExpression1);
            var waIndex2 = new SingleTokenExpressionIndex(TestData.TokenKindAttributesExpression1);

            var dictionaryBasedIndex = ciIndex1.MergeFromNullable(ciIndex2)
                .MergeFromNullable(csIndex1).MergeFromNullable(csIndex2)
                .MergeFromNullable(tkIndex1).MergeFromNullable(tkIndex2)
                .MergeFromNullable(wpIndex1).MergeFromNullable(wpIndex2)
                .MergeFromNullable(waIndex1).MergeFromNullable(waIndex2);

            var token = new Token(TestData.Token2Kind, TestData.Word2Class, TestData.Token2Text, location: null);

            var matchingExpressions = new HashSet<TokenExpression>();
            dictionaryBasedIndex.SelectMatchingExpressions(token, null, matchingExpressions);
            Assert.AreEqual(7, matchingExpressions.Count());
            Assert.IsTrue(matchingExpressions.Contains(TestData.CaseInsensitiveTokenExpression2));
            Assert.IsTrue(matchingExpressions.Contains(TestData.CaseSensitiveTokenExpression2));
            Assert.IsTrue(matchingExpressions.Contains(TestData.TokenKindExpression1));
            Assert.IsTrue(matchingExpressions.Contains(TestData.TokenKindExpression2));
            Assert.IsTrue(matchingExpressions.Contains(TestData.WordPrefixExpression1));
            Assert.IsTrue(matchingExpressions.Contains(TestData.WordPrefixAttributesExpression1));
            Assert.IsTrue(matchingExpressions.Contains(TestData.TokenKindAttributesExpression1));
        }
    }
}
