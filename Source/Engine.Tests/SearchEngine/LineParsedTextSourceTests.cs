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
    [TestCategory("LineParsedTextSource")]
    public class LineParsedTextSourceTests
    {
        [TestMethod]
        public void CheckTokensContextSimple()
        {
            string text = "First line\nSecond line\nThird to end";
            // Tokens:
            //    0      1      2     3       4         5       6     7       8         9     10    11   12    13   14
            // [Start] First [Space] line [LineBreak] Second [Space] line [LineBreak] Third [Space] to [Space] end [End]

            // Tokens by line:
            // 0: [Start] First [Space] line [LineBreak]
            // 1: Second [Space] line [LineBreak]
            // 2: Third [Space] to [Space] end [End]

            List<Token> expectedTokens = GetExpectedTokensWithLineContext(text);
            List<Token> actualTokens = GetLineParsedTextSourceTokens(text);

            TestTokensWithContext(expectedTokens, actualTokens);
        }

        [TestMethod]
        public void CheckTokensContext()
        {
            string text = "First\n\nThird";
            // Tokens:
            //    0      1       2           3         4     5
            // [Start] First [LineBreak] [LineBreak] Third [End]

            // Tokens by line:
            // 0: [Start] First [Space] line [LineBreak]
            // 1: Second [Space] line [LineBreak]
            // 2: Third [Space] to [Space] end [End]

            List<Token> expectedTokens = GetExpectedTokensWithLineContext(text);
            List<Token> actualTokens = GetLineParsedTextSourceTokens(text);

            TestTokensWithContext(expectedTokens, actualTokens);
        }

        [TestMethod]
        public void CheckTokensContextEmptyText()
        {
            string text = string.Empty;
            // Tokens:
            //    0      1
            // [Start] [End]

            // Tokens by line:
            // 0: [Start] [End]

            List<Token> expectedTokens = GetExpectedTokensWithLineContext(text);
            List<Token> actualTokens = GetLineParsedTextSourceTokens(text);

            TestTokensWithContext(expectedTokens, actualTokens);
        }

        public static void TestTokensWithContext(List<Token> expectedTokens, List<Token> actualTokens)
        {
            actualTokens.Should().BeEquivalentTo(expectedTokens, opt => opt
                .Excluding(x => x.Location.Context.PreviousLineBreak.Context)
                .Excluding(x => x.Location.Context.CurrentLineBreak.Context)
                .IgnoringCyclicReferences()
                .WithStrictOrdering());
        }

        public static List<Token> GetExpectedTokensWithLineContext(string text)
        {
            List<Token> expectedTokens = ParsedTextSourceTokens(text);
            List<int> lineBreaks = expectedTokens.Select((token, index) => (index, token))
                .Where(x => x.token.Kind == TokenKind.LineBreak || x.token.Kind == TokenKind.End)
                .Select(x => x.index).ToList();
            TextLocation startLocation;
            TextLocation endLocation = new TextLocation(0, 0, 0);
            TextLocationContext lineContext;
            int tokenNumber = 0;
            for (int i = 0; i < lineBreaks.Count; i++)
            {
                int lineBreakTokenNumber = lineBreaks[i];
                startLocation = endLocation;
                endLocation = expectedTokens[lineBreakTokenNumber].Location;
                lineContext = new TextLocationContext(startLocation, endLocation);
                while (tokenNumber <= lineBreakTokenNumber)
                {
                    expectedTokens[tokenNumber].Location.Context = lineContext;
                    tokenNumber++;
                }
            }
            return expectedTokens;
        }

        public static List<Token> ParsedTextSourceTokens(string text)
        {
            ParsedText parsedText = PlainTextParser.Parse(text);
            ITextSource parsedTextSource = new ParsedTextSource(parsedText);
            List<Token> tokens = GetTokensFromTextSource(parsedTextSource);
            return tokens;
        }

        public static List<Token> GetTokensFromTextSource(ITextSource textSource)
        {
            List<Token> tokens = new List<Token>();
            foreach (var token in textSource)
            {
                tokens.Add(token);
            }
            return tokens;
        }

        // Internal

        private static List<Token> GetLineParsedTextSourceTokens(string text)
        {
            ParsedText parsedText = PlainTextParser.Parse(text);
            ITextSource lineParsedTextSource = new LineParsedTextSource(parsedText);
            List<Token> tokens = GetTokensFromTextSource(lineParsedTextSource);
            return tokens;
        }
    }
}
