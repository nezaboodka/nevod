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

using static Nezaboodka.Nevod.Engine.Tests.LineParsedTextSourceTests;
using static Nezaboodka.Nevod.Engine.Tests.TestHelper;
using System.Text;
using Nezaboodka.Text.Parsing;
using FluentAssertions;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("LineStreamTextSource")]
    public class LineStreamTextSourceTests
    {
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CheckTokensContextSimpleSingleBlock(bool withReader)
        {
            string text = "First line\nSecond line\nThird to end";
            int bufferSizeInChars = 100;
            // Tokens:
            //    0      1      2     3       4         5       6     7       8         9     10    11   12    13   14
            // [Start] First [Space] line [LineBreak] Second [Space] line [LineBreak] Third [Space] to [Space] end [End]

            // Tokens by line:
            // 0: [Start] First [Space] line [LineBreak]
            // 1: Second [Space] line [LineBreak]
            // 2: Third [Space] to [Space] end [End]

            List<Token> expectedTokens = GetExpectedTokensWithLineContext(text);
            List<Token> actualTokens = GetLineStreamTextSourceTokens(text, withReader, bufferSizeInChars);

            TestTokensWithContext(expectedTokens, actualTokens);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CheckTokensContextSimpleSmallBuffer(bool withReader)
        {
            string text = "First line\nSecond line\nThird to end";
            int bufferSizeInChars = 12;  // первый блок до первого перевода строки включительно

            List<Token> expectedTokens = GetExpectedTokensWithLineContext(text);
            List<Token> actualTokens = GetLineStreamTextSourceTokens(text, withReader, bufferSizeInChars);

            TestTokensWithContext(expectedTokens, actualTokens);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CheckTokensContextSingleBlock(bool withReader)
        {
            string text = "First\n\nThird";
            int bufferSizeInChars = 100;
            // Tokens:
            //    0      1       2           3         4     5
            // [Start] First [LineBreak] [LineBreak] Third [End]

            // Tokens by line:
            // 0: [Start] First [Space] line [LineBreak]
            // 1: Second [Space] line [LineBreak]
            // 2: Third [Space] to [Space] end [End]

            List<Token> expectedTokens = GetExpectedTokensWithLineContext(text);
            List<Token> actualTokens = GetLineStreamTextSourceTokens(text, withReader, bufferSizeInChars);

            TestTokensWithContext(expectedTokens, actualTokens);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CheckTokensContextSmallBuffer(bool withReader)
        {
            string text = "First\n\nThird";
            int bufferSizeInChars = 7;  // первый блок до первого перевода строки включительно

            List<Token> expectedTokens = GetExpectedTokensWithLineContext(text);
            List<Token> actualTokens = GetLineStreamTextSourceTokens(text, withReader, bufferSizeInChars);

            TestTokensWithContext(expectedTokens, actualTokens);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CheckTokensContextNoLineBreakInFirstBlock(bool withReader)
        {
            string text = "First\n\nThird";
            int bufferSizeInChars = 6;  // первый блок не содержит перевода строки

            List<Token> expectedTokens = GetExpectedTokensWithLineContext(text);
            // Из-за отсутствия перевода строки в первом блоке, у токена Start не будет контекста.
            expectedTokens[0].Location.Context = null;
            List<Token> actualTokens = GetLineStreamTextSourceTokens(text, withReader, bufferSizeInChars);

            TestTokensWithContext(expectedTokens, actualTokens);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CheckTokensContextEmptyText(bool withReader)
        {
            string text = string.Empty;
            int bufferSizeInChars = 100;
            // Tokens:
            //    0      1
            // [Start] [End]

            // Tokens by line:
            // 0: [Start] [End]

            List<Token> expectedTokens = GetExpectedTokensWithLineContext(text);
            List<Token> actualTokens = GetLineStreamTextSourceTokens(text, withReader, bufferSizeInChars);

            TestTokensWithContext(expectedTokens, actualTokens);
        }

        // Internal

        private List<Token> GetLineStreamTextSourceTokens(string text, bool withReader, int bufferSizeInChars)
        {
            ITextSource lineParsedTextSource = LineStreamTextSource.FromString(text, withReader, bufferSizeInChars);
            List<Token> tokens = GetTokensFromTextSource(lineParsedTextSource);
            return tokens;
        }
    }
}
