//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Nezaboodka.Text.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nezaboodka.Text.Tests
{
    [TestClass]
    [TestCategory("Text"), TestCategory("Text.Parsing")]
    public class ParsedTextTests
    {
        private static readonly string[] Xhtml = { "<p>", "Hello, ", "<b>", "my w", "</b>", "orld!", "</p>" };
        private static readonly ISet<int> PlainTextInXhtml = new HashSet<int> { 1, 3, 5 };

        // Public

        [TestMethod]
        public void PlainTextForTokenReferenceInSinglePlainTextElementTest()
        {
            ParsedText parsedText = CreateParsedText(Xhtml, PlainTextInXhtml);
            TokenReference testTokenReference = new TokenReference
            {
                XhtmlIndex = 1,
                StringPosition = 0,
                StringLength = 5,
                TokenKind = TokenKind.Alphabetic
            };
            string expected = "Hello";
            string actual = parsedText.GetTokenText(testTokenReference);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void PlainTextForCompoundTokenReferenceTest()
        {
            ParsedText parsedText = CreateParsedText(Xhtml, PlainTextInXhtml);
            TokenReference testTokenReference = new TokenReference
            {
                XhtmlIndex = 3,
                StringPosition = 3,
                StringLength = 5
            };
            string expected = "world";
            string actual = parsedText.GetTokenText(testTokenReference);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void AllPlainTextTest()
        {
            ParsedText parsedText = CreateParsedText(Xhtml, PlainTextInXhtml);
            string expected = "Hello, my world!";
            string actual = parsedText.GetPlainText();
            Assert.AreEqual(expected, actual);;
        }

        [TestMethod]
        public void PlainTextForTagTest()
        {
            string[] xhtml = { "<html>", "<p>", "First paragraph", "</p>", "<p>", "Second paragraph", "</p>", "</html>" };
            HashSet<int> plainTextInXhtml = new HashSet<int> { 2, 5 };
            ParsedText parsedText = CreateParsedText(xhtml, plainTextInXhtml);
            FormattingTag[] testTags = {
                new FormattingTag
                {
                    TagName = string.Empty,
                    TokenPosition = 0,
                    TokenLength = 3

                },
                new FormattingTag
                {
                    TagName = string.Empty,
                    TokenPosition = 3,
                    TokenLength = 3
                }
            };
            TokenReference[] TokenReferences =
            {
                new TokenReference {XhtmlIndex = 2, StringPosition = 0, StringLength = 5, TokenKind = TokenKind.Alphabetic},
                new TokenReference {XhtmlIndex = 2, StringPosition = 5, StringLength = 1, TokenKind = TokenKind.WhiteSpace},
                new TokenReference {XhtmlIndex = 2, StringPosition = 6, StringLength = 9, TokenKind = TokenKind.Alphabetic},
                new TokenReference {XhtmlIndex = 5, StringPosition = 0, StringLength = 6, TokenKind = TokenKind.Alphabetic},
                new TokenReference {XhtmlIndex = 5, StringPosition = 6, StringLength = 1, TokenKind = TokenKind.WhiteSpace},
                new TokenReference {XhtmlIndex = 5, StringPosition = 7, StringLength = 9, TokenKind = TokenKind.Alphabetic}
            };
            foreach (TokenReference TokenReference in TokenReferences)
            {
                parsedText.AddToken(TokenReference);
            }
            string[] expected = { "First paragraph", "Second paragraph" };
            string[] actual = testTags.Select(tag => parsedText.GetTagText(tag)).ToArray();
            CollectionAssert.AreEqual(expected, actual);
        }

        // Internal

        private ParsedText CreateParsedText(string[] xhtml, ISet<int> plainTextInXhtml)
        {
            var result = new ParsedText();
            result.SetXhtml(xhtml, plainTextInXhtml);
            return result;
        }
    }
}
