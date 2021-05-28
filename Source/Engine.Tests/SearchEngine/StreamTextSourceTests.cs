//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static Nezaboodka.Nevod.Engine.Tests.TestHelper;
using System.Text;
using Nezaboodka.Text.Parsing;
using FluentAssertions;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("StreamTextSource")]
    public class StreamTextSourceTests
    {
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void ParseStream(bool withReader)
        {
            int bufferSizeInChars = 9;
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";

            List<Token> expectedTokenList = GetTokensViaParsedTextSource(text);
            List<Token> actualTokenList = GetTokensViaStreamTextSource(text, withReader, bufferSizeInChars);

            actualTokenList.Should().BeEquivalentTo(expectedTokenList);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void ParseEmptyStream(bool withReader)
        {
            int bufferSizeInChars = 9;
            string text = string.Empty;

            List<Token> expectedTokenList = GetTokensViaParsedTextSource(text);
            List<Token> actualTokenList = GetTokensViaStreamTextSource(text, withReader, bufferSizeInChars);

            actualTokenList.Should().BeEquivalentTo(expectedTokenList);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void ParseStreamSingleBlock(bool withReader)
        {
            int bufferSizeInChars = 1000;
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";

            List<Token> expectedTokenList = GetTokensViaParsedTextSource(text);
            List<Token> actualTokenList = GetTokensViaStreamTextSource(text, withReader, bufferSizeInChars);

            actualTokenList.Should().BeEquivalentTo(expectedTokenList);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CompareStreamTextSourceOnSimplePatterns(bool withReader)
        {
            string patterns = "#S = 'smartphone';";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            int bufferSizeInChars = 10;
            CheckTextSearchEngineWithStreamTextSource(patterns, text, withReader, bufferSizeInChars);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CheckGetTextFromDifferentBlocks(bool withReader)
        {
            // Токены:
            //    0      1      2      3      4       5       6      7      8
            // [Start] "One" [Space] "Two" [Space] "Three" [Space] "Four" [End]
            string text = "One Two Three Four";
            int bufferSizeInChars = 10;

            List<Token> expectedTokenList = GetTokensViaParsedTextSource(text);
            List<Token> actualTokenList = GetTokensViaStreamTextSource(text, withReader, bufferSizeInChars);

            actualTokenList.Should().BeEquivalentTo(expectedTokenList);
            // Блоки чтения:
            // 1) "One Two Th"
            // 2) "ree Four"
            //
            // ParsedTexts:
            // 0) [Start]
            // 1) "One" [Space] "Two" [Space]
            // 2) "Three" [Space]
            // 3) "Four" [End]
            StreamTextSource textSource = StreamTextSource.FromString(text, withReader, bufferSizeInChars);
            IEnumerator<Token> enumerator = textSource.GetEnumerator();

            enumerator.MoveNext();  // 0 [Start]
            // Новый блок
            enumerator.MoveNext();  // 1 "One"
            enumerator.MoveNext();  // 2 [Space]
            enumerator.MoveNext();  // 3 "Two"

            // Текст из текущего блока
            string actual = textSource.GetText(TextLocationForTokenNumber(1), TextLocationForTokenNumber(3));
            actual.Should().BeEquivalentTo("One Two");

            // Текст из текущего блока - выход за правую границу
            actual = textSource.GetText(TextLocationForTokenNumber(1), TextLocationForTokenNumber(10));
            actual.Should().BeNull();

            // Пересечение с предыдущим блоком (включая Start)
            actual = textSource.GetText(TextLocationForTokenNumber(0), TextLocationForTokenNumber(2));
            actual.Should().BeEquivalentTo("One ");

            enumerator.MoveNext();  // 4 [Space]
            // Новый блок
            enumerator.MoveNext();  // 5 "Three"
            enumerator.MoveNext();  // 6 [Space]

            // Текст из предыдущего блока.
            actual = textSource.GetText(TextLocationForTokenNumber(1), TextLocationForTokenNumber(3));
            actual.Should().BeEquivalentTo("One Two");

            // Текст из предыдущего блока - выход за левую границу
            actual = textSource.GetText(TextLocationForTokenNumber(0), TextLocationForTokenNumber(3));
            actual.Should().BeNull();

            // Пересечение с предыдущим блоком
            actual = textSource.GetText(TextLocationForTokenNumber(2), TextLocationForTokenNumber(5));
            actual.Should().BeEquivalentTo(" Two Three");

            // Пересечение с предыдущим блоком - выход за правую границу
            actual = textSource.GetText(TextLocationForTokenNumber(2), TextLocationForTokenNumber(10));
            actual.Should().BeNull();

            enumerator.MoveNext();  // 7 "Four"
            enumerator.MoveNext();  // 8 [End] - добавлен в конец последнего блока
            bool done = enumerator.MoveNext();
            done.Should().BeFalse();

            // Текст из текущего блока (включая End)
            actual = textSource.GetText(TextLocationForTokenNumber(7), TextLocationForTokenNumber(8));
            actual.Should().BeEquivalentTo("Four");

            // Текст из текущего блока (искусственные токены)
            actual = textSource.GetText(TextLocationForTokenNumber(~5), TextLocationForTokenNumber(~7));
            actual.Should().BeEquivalentTo("Three Four");

            // Текст из текущего блока (start == end)
            TextLocation location = TextLocationForTokenNumber(7);
            actual = textSource.GetText(location, location);
            actual.Should().BeEquivalentTo("Four");

            // Текст из текущего блока (искусственные токены)
            actual = textSource.GetText(TextLocationForTokenNumber(~5), TextLocationForTokenNumber(~7));
            actual.Should().BeEquivalentTo("Three Four");

            // start > end
            textSource.Invoking(x => x.GetText(TextLocationForTokenNumber(2), TextLocationForTokenNumber(1)))
                .Should().Throw<ArgumentException>();
        }

        public static void CheckTextSearchEngineWithStreamTextSource(string patterns, string text, bool withReader,
            int bufferSizeInChars)
        {
            var package = PatternPackage.FromText(patterns);
            var engine = new TextSearchEngine(package);
            SearchResult expectedResult = engine.Search(text);

            var textSource = StreamTextSource.FromString(text, withReader, bufferSizeInChars);
            SearchResult actualResult = engine.Search(textSource);

            actualResult.ToComparableTagList().Should().BeEquivalentTo(
                expectedResult.ToComparableTagList());
        }

        // Internal

        private static List<Token> GetTokensViaParsedTextSource(string text)
        {
            ParsedText parsedText = PlainTextParser.Parse(text);
            var textSource = new ParsedTextSource(parsedText);
            List<Token> tokens = new List<Token>();
            foreach (Token token in textSource)
                tokens.Add(token);
            return tokens;
        }

        private static List<Token> GetTokensViaStreamTextSource(string text, bool withReader, int bufferSizeInChars)
        {
            StreamTextSource streamTextSource = StreamTextSource.FromString(text, withReader, bufferSizeInChars);
            List<Token> tokens = new List<Token>();
            foreach (Token token in streamTextSource)
                tokens.Add(token);
            return tokens;
        }

        private static TextLocation TextLocationForTokenNumber(long tokenNumber)
        {
            return new TextLocation(tokenNumber, 0, 0);
        }
    }

    public static class SearchResultExtension
    {
        public static object ToComparableTagList(this SearchResult searchResult)
        {
            var result = searchResult.GetTags().Select(t => (t.PatternFullName, t.Start, t.End));
            return result;
        }
    }
}
