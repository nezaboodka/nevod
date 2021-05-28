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
    public class ParserTests
    {
        [TestMethod]
        public void LatinTest()
        {
            string testString = "The (\"brown\") can't 32.3 feet, right?";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("The", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("(", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("\"", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("brown", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("\"", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>(")", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("can", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("'", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("t", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("32", TokenKind.Numeric, true),
                new Tuple<string, TokenKind, bool>(".", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("3", TokenKind.Numeric, true),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("feet", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>(",", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("right", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("?", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false),
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void OneWordTest()
        {
            string testString = "word";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("word", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void WhitespacesTest()
        {
            string testString = "  \t" + (char)160;
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("  \t" + (char)160, TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void SymbolsTest()
        {
            string testString = "#hashtag @name";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("#", TokenKind.Symbol, false),
                new Tuple<string, TokenKind, bool>("hashtag", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("@", TokenKind.Symbol, false),
                new Tuple<string, TokenKind, bool>("name", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void EndTokenStringPositionTest()
        {
            string testString = "#hashtag @name and some more text...";
            ParsedText parsedText = PlainTextParser.Parse(testString);
            Assert.AreEqual(testString.Length, parsedText.PlainTextTokens.Last().StringPosition);
        }

        [TestMethod]
        public void EmptyStringTest()
        {
            string testString = "";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void EmptyStringWithoutStartAndEndTokensTest()
        {
            string testString = "";
            Tuple<string, TokenKind, bool>[] expectedResult = { };
            var options = new PlainTextParserOptions()
            {
                ProduceStartAndEndTokens = false,
                DetectParagraphs = false
            };
            ParsePlainTextAndTestTokensWithOptions(testString, options, expectedResult);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTest()
        {
            string testString = null;
            PlainTextParser.Parse(testString);
        }

        [TestMethod]
        public void CyrillicTest()
        {
            string testString = "А4 формат 1А класс; 56,31 светового, 45.1 дня!";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("А4", TokenKind.AlphaNumeric, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("формат", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("1А", TokenKind.NumericAlpha, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("класс", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>(";", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("56", TokenKind.Numeric, true),
                new Tuple<string, TokenKind, bool>(",", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("31", TokenKind.Numeric, true),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("светового", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>(",", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("45", TokenKind.Numeric, true),
                new Tuple<string, TokenKind, bool>(".", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("1", TokenKind.Numeric, true),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("дня", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("!", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void DotBetweenAlphabeticTest()
        {
            string testString = "ivan.shimko";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("ivan", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>(".", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("shimko", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void UnderscoreBetweenAlphabeticTest()
        {
            string testString = "ivan_shimko";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("ivan", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("_", TokenKind.Symbol, false),
                new Tuple<string, TokenKind, bool>("shimko", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void OneSymbolTest()
        {
            string testString = "L";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("L", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void FormatCharactersTest()
        {
            string testString = "a\u0308b\u0308cd 3.4";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("a\u0308b\u0308cd", TokenKind.Symbol, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("3", TokenKind.Numeric, true),
                new Tuple<string, TokenKind, bool>(".", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("4", TokenKind.Numeric, true),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void LineSeparatorTest()
        {
            string testString = "a\nb\r\nc\rd";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("a", TokenKind.Alphabetic, true),
                new Tuple<string, TokenKind, bool>("\n", TokenKind.LineFeed, false),
                new Tuple<string, TokenKind, bool>("b", TokenKind.Alphabetic, true),
                new Tuple<string, TokenKind, bool>("\r\n", TokenKind.LineFeed, false),
                new Tuple<string, TokenKind, bool>("c", TokenKind.Alphabetic, true),
                new Tuple<string, TokenKind, bool>("\r", TokenKind.LineFeed, false),
                new Tuple<string, TokenKind, bool>("d", TokenKind.Alphabetic, true),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void PlainTextMultipleParagraphTagsTest()
        {
            string testString = "First paragraph\n\nSecond paragraph\n\nThird paragraph ab";
            string[] expectedResult =
            {
                "First paragraph",
                "Second paragraph",
                "Third paragraph ab"
            };
            ParsePlainTextAndTestTags(testString, expectedResult);
        }

        [TestMethod]
        public void PlainTextWithoutParagraphsTagsTest()
        {
            string testString = "First paragraph\n\nSecond paragraph\n\nThird paragraph ab";
            string[] expectedResult = { };
            var options = new PlainTextParserOptions()
            {
                ProduceStartAndEndTokens = false,
                DetectParagraphs = false
            };
            ParsePlainTextAndTestTagsWithOptions(testString, options, expectedResult);
        }

        [TestMethod]
        public void PlainTextSingleParapgraphTagTest()
        {
            string testString = "A\nsingle\nparagraph ab";
            string[] expectedResult =
            {
                "A\nsingle\nparagraph ab"
            };
            ParsePlainTextAndTestTags(testString, expectedResult);
        }

        [TestMethod]
        public void XmlTagAttributeTest()
        {
            string testString = "xml:lang";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("xml", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>(":", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("lang", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        [TestMethod]
        public void SingleQuoteBetweenWordsTest()
        {
            string testString = "I’ll";
            Tuple<string, TokenKind, bool>[] expectedResult =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("I", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("’", TokenKind.Symbol, false),
                new Tuple<string, TokenKind, bool>("ll", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParsePlainTextAndTestTokens(testString, expectedResult);
        }

        // [TestMethod]
        // public void BokmalTest()
        // {
        //     string testString = "a)‬"; // there is U+x202C char after parenthesis!
        //     Tuple<string, TokenKind, bool>[] expectedResult =
        //     {
        //         new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
        //         new Tuple<string, TokenKind, bool>("a", TokenKind.Alphabetic, false),
        //         new Tuple<string, TokenKind, bool>(")", TokenKind.Punctuation, false),
        //         new Tuple<string, TokenKind, bool>("‬", TokenKind.Symbol, false), // U+x202C
        //         new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
        //     };
        //     ParsePlainTextAndTestTokens(testString, expectedResult);
        // }

        // [TestMethod]
        // public void PrinceOfPerciaTest()
        // {
        //     string testString = "Принц Пе́рсии";
        //     Tuple<string, TokenKind, bool>[] expectedResult =
        //     {
        //         new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
        //         new Tuple<string, TokenKind, bool>("Принц", TokenKind.Alphabetic, false),
        //         new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
        //         new Tuple<string, TokenKind, bool>("‬Пе́рсии", TokenKind.Alphabetic, false),
        //         new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
        //     };
        //     ParsePlainTextAndTestTokens(testString, expectedResult);
        // }

        [TestMethod]
        public void XhtmlElementsTagAfterRootTagTest()
        {
            string testString = "<html><body>Test <empty/><b>string</b></body></html>";
            string[] expectedResult = { "<html>", "<body>", "Test ", "<empty/>", "<b>", "string", "</b>", "</body>", "</html>" };
            ParsePlainTextAndTestXhtmlElements(testString, expectedResult);
        }

        [TestMethod]
        public void XhtmlElementsTextAfterRootTagTest()
        {
            string testString = "<html>Some text</html>";
            string[] expectedResult = { "<html>", "Some text", "</html>" };
            ParsePlainTextAndTestXhtmlElements(testString, expectedResult);
        }

        [TestMethod]
        public void XhtmlElementsSingleRootTagTest()
        {
            string testString = "<html></html>";
            string[] expectedResult = { "<html>", "</html>" };
            ParsePlainTextAndTestXhtmlElements(testString, expectedResult);
        }

        [TestMethod]
        public void XhtmlElementsDocumentTagsTest()
        {
            string testString = @"<?xml version=""1.0"" encoding=""UTF-8""?><html>
                <head>
                <meta name=""Author"" content=""Иван Шимко""/>                
                <title>Title</title>
                </head><body><p>First paragraph.</p>                                
                </body></html>";
            string[] expectedRestult = { "<body>", "<p>", "First paragraph.", "</p>", "</body>" };
            ParsePlainTextAndTestXhtmlElements(testString, expectedRestult);
        }

        [TestMethod]
        public void XhtmlElementsDocumentTagsEmptyTitleTest()
        {
            string testString = @"<?xml version=""1.0"" encoding=""UTF-8""?><html>
                <head>
                <meta name=""Author"" content=""Иван Шимко""/>                
                <title/>
                </head><body><p>First paragraph.</p>                                
                </body></html>";
            string[] expectedRestult = { "<body>", "<p>", "First paragraph.", "</p>", "</body>" };
            ParsePlainTextAndTestXhtmlElements(testString, expectedRestult);
        }

        [TestMethod]
        public void XhtmlTokensCompoundTokensTest()
        {
            string testString = "<p>Hello, <b>w</b>orld!</p>";
            Tuple<string, TokenKind, bool>[] expectedTokens =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("Hello", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>(",", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>(" ", TokenKind.WhiteSpace, false),
                new Tuple<string, TokenKind, bool>("world", TokenKind.Alphabetic, false),
                new Tuple<string, TokenKind, bool>("!", TokenKind.Punctuation, false),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParseXhtmlAndTestTokens(testString, expectedTokens);
        }

        [TestMethod]
        public void XhtmlTokensOneSymbolTokensTest()
        {
            string testString = "<html><p>a</p><p>b</p><p>c</p><p>d</p></html>";
            Tuple<string, TokenKind, bool>[] expectedTokens =
            {
                new Tuple<string, TokenKind, bool>("", TokenKind.Start, false),
                new Tuple<string, TokenKind, bool>("a", TokenKind.Alphabetic, true),
                new Tuple<string, TokenKind, bool>("b", TokenKind.Alphabetic, true),
                new Tuple<string, TokenKind, bool>("c", TokenKind.Alphabetic, true),
                new Tuple<string, TokenKind, bool>("d", TokenKind.Alphabetic, true),
                new Tuple<string, TokenKind, bool>("", TokenKind.End, false)
            };
            ParseXhtmlAndTestTokens(testString, expectedTokens);
        }

        [TestMethod]
        public void XhtmlTagsParagraphsTest()
        {
            string testString = "<html><p>Paragraph1</p>\n<p>Paragraph2</p></html>";
            string[] expectedTags =
            {
                "Paragraph1",
                "Paragraph2"
            };
            ParseXhtmlAndTestTags(testString, expectedTags);
        }

        [TestMethod]
        public void XhtmlTagsOneSymbolTokensTest()
        {
            string testString = "<html><p>a</p><p>b</p><p>c</p></html>";
            string[] expectedTags =
            {
                "a",
                "b",
                "c"
            };
            ParseXhtmlAndTestTags(testString, expectedTags);
        }

        [TestMethod]
        public void XhtmlTagsEmptyTagTest()
        {
            string testString = "<html><p></p></html>";
            string[] expectedTags = { };
            ParseXhtmlAndTestTags(testString, expectedTags);
        }

        [TestMethod]
        public void XhtmlTagsSelfClosingTagTest()
        {
            string testString = "<html><p/></html>";
            string[] expectedTags = { };
            ParseXhtmlAndTestTags(testString, expectedTags);
        }

        [TestMethod]
        public void XhtmlTagsCompoundTokensTest()
        {
            string testString = "<html><p>Paragraph<b>1</b>\nstill paragraph1</p>\n<p>Paragraph2</p></html>";
            string[] expectedTags =
            {
                "Paragraph1\nstill paragraph1",
                "Paragraph2"
            };
            ParseXhtmlAndTestTags(testString, expectedTags);
        }

        [TestMethod]
        public void XhtmlDocumentTagsTest()
        {
            string testString =
                @"<?xml version=""1.0"" encoding=""UTF-8""?><html>
                <head>
                <meta name=""Author"" content=""Иван Шимко""/>
                <meta name=""publisher"" content=""Home""/>
                <meta name=""meta:page-count"" content=""1""/>
                <meta name=""dc:publisher"" content=""Home""/>
                <title>Title</title>
                </head>
                <body><h1>Title</h1>                                                                
                <p>First paragraph.</p>                                
                </body></html>";
            Tuple<string, string>[] expectedResult =
            {
                new Tuple<string, string>("Author", "Иван Шимко"),
                new Tuple<string, string>("publisher", "Home"),
                new Tuple<string, string>("meta:page-count", "1"),
                new Tuple<string, string>("dc:publisher", "Home"),
                new Tuple<string, string>("title", "Title"),
            };
            ParseXhtmlAndTestDocumentTags(testString, expectedResult);
        }

        [TestMethod]
        public void XhtmlDocumentTagsEmptyTitleTest()
        {
            string testString =
                @"<?xml version=""1.0"" encoding=""UTF-8""?><html>
                <head>
                <meta name=""Author"" content=""Иван Шимко""/>
                <title/>
                </head>
                <body><h1>Title</h1>                                                                
                <p>First paragraph.</p>                                
                </body></html>";
            Tuple<string, string>[] expectedResult =
            {
                new Tuple<string, string>("Author", "Иван Шимко"),
            };
            ParseXhtmlAndTestDocumentTags(testString, expectedResult);
        }

        // Static internal

        private static void ParsePlainTextAndTestTokens(string testString, Tuple<string, TokenKind, bool>[] expectedResult)
        {
            ParsePlainTextAndTestTokensWithOptions(testString, null, expectedResult);
        }

        private static void ParsePlainTextAndTestTokensWithOptions(string testString, PlainTextParserOptions options,
            Tuple<string, TokenKind, bool>[] expectedResult)
        {
            Tuple<string, TokenKind, bool>[] result = GetTokensFromParsedText(PlainTextParser.Parse(testString, options));
            CollectionAssert.AreEqual(result, expectedResult);
        }

        private static void ParsePlainTextAndTestTags(string testString, string[] expectedTags)
        {
            ParsePlainTextAndTestTagsWithOptions(testString, null, expectedTags);
        }

        private static void ParsePlainTextAndTestTagsWithOptions(string testString, PlainTextParserOptions options, string[] expectedTags)
        {
            string[] actualTags = GetTagsFromParsedText(PlainTextParser.Parse(testString, options));
            CollectionAssert.AreEqual(expectedTags, actualTags);
        }

        private static void ParsePlainTextAndTestXhtmlElements(string testString, string[] expectedRestult)
        {
            string[] actualResult = XhtmlParser.Parse(testString).XhtmlElements.ToArray();
            CollectionAssert.AreEqual(expectedRestult, actualResult);
        }

        private static void ParseXhtmlAndTestTokens(string testString, Tuple<string, TokenKind, bool>[] expectedTokens)
        {
            ParsedText parsedText = XhtmlParser.Parse(testString);
            Tuple<string, TokenKind, bool>[] actualTokens = GetTokensFromParsedText(parsedText);
            CollectionAssert.AreEqual(expectedTokens, actualTokens);
        }

        private static void ParseXhtmlAndTestTags(string testString, string[] expectedTags)
        {
            ParsedText parsedText = XhtmlParser.Parse(testString);
            string[] actualTags = GetTagsFromParsedText(parsedText);
            CollectionAssert.AreEqual(expectedTags, actualTags);
        }

        private static void ParseXhtmlAndTestDocumentTags(string testString, Tuple<string, string>[] expectedResult)
        {
            ParsedText parsedText = XhtmlParser.Parse(testString);
            Tuple<string, string>[] actualResult = GetDocumentTagsFromParsedText(parsedText);
            CollectionAssert.AreEqual(expectedResult, actualResult);
        }

        private static string[] GetTagsFromParsedText(ParsedText parsedText)
        {
            return parsedText.FormattingTags.Select(parsedText.GetTagText).ToArray();
        }

        private static Tuple<string, TokenKind, bool>[] GetTokensFromParsedText(ParsedText parsedText)
        {
            return parsedText
                .PlainTextTokens
                .Select(x => new Tuple<string, TokenKind, bool>(parsedText.GetTokenText(x), x.TokenKind, x.IsHexadecimal))
                .ToArray();
        }

        private static Tuple<string, string>[] GetDocumentTagsFromParsedText(ParsedText parsedText)
        {
            return parsedText.DocumentTags.Select(tag => new Tuple<string, string>(tag.TagName, tag.Content)).ToArray();
        }
    }
}
