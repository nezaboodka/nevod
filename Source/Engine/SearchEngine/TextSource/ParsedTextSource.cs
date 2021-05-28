//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    public class ParsedTextSource : ITextSource
    {
        public ParsedText Text { get; }

        public int TokenCount => Text.PlainTextTokens.Count;

        public ParsedTextSource(ParsedText text)
        {
            Text = text;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            for (int i = 0; i < Text.PlainTextTokens.Count; i++)
                yield return GetToken(i);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string GetText(TextLocation start, TextLocation end)
        {
            string result;
            long startTokenNumber = start.TokenNumber;
            if (startTokenNumber < 0)
                startTokenNumber = ~startTokenNumber;
            long endTokenNumber;
            if (start != end)
            {
                endTokenNumber = end.TokenNumber;
                if (endTokenNumber < 0)
                    endTokenNumber = ~endTokenNumber;
            }
            else
                endTokenNumber = startTokenNumber;
            int internalStartTokenNumber = (int)startTokenNumber;
            if (startTokenNumber != endTokenNumber)
            {
                var sb = new StringBuilder();
                for (int i = internalStartTokenNumber; i <= endTokenNumber; i++)
                    sb.Append(Text.GetTokenText(Text.PlainTextTokens[i]));
                result = sb.ToString();
            }
            else
                result = Text.GetTokenText(Text.PlainTextTokens[internalStartTokenNumber]);
            return result;
        }

        public Token GetToken(int tokenNumber)
        {
            TokenReference tokenReference = Text.PlainTextTokens[tokenNumber];
            Token result = new Token(TokenKindByTokenReferenceKind[(int)tokenReference.TokenKind],
                WordClassByTokenReferenceKind[(int)tokenReference.TokenKind],
                Text.GetTokenText(tokenReference),
                new TextLocation(tokenNumber, tokenReference.StringPosition, tokenReference.StringLength));
            return result;
        }

        internal static readonly TokenKind[] TokenKindByTokenReferenceKind =
        {
            TokenKind.Undefined,    // Undefined = 0,
            TokenKind.Start,        // Start = 1,
            TokenKind.End,          // End = 2,
            TokenKind.Word,         // Alphabetic = 3,
            TokenKind.Word,         // AlphaNumeric = 4,
            TokenKind.Word,         // NumericAlpha = 5,
            TokenKind.LineBreak,    // LineFeed = 6,
            TokenKind.Word,         // Numeric = 7,
            TokenKind.Punctuation,  // Punctuation = 8,
            TokenKind.Symbol,       // Symbol = 9,
            TokenKind.Space         // WhiteSpace = 10
        };

        internal static readonly WordClass[] WordClassByTokenReferenceKind =
        {
            WordClass.Any,           // Undefined = 0,
            WordClass.Any,           // Start = 1,
            WordClass.Any,           // End = 2,
            WordClass.Alpha,         // Alphabetic = 3,
            WordClass.AlphaNum,      // AlphaNumeric = 4,
            WordClass.NumAlpha,      // NumericAlpha = 5,
            WordClass.Any,           // LineFeed = 6,
            WordClass.Num,           // Numeric = 7,
            WordClass.Any,           // Punctuation = 8,
            WordClass.Any,           // Symbol = 9,
            WordClass.Any            // WhiteSpace = 10
        };
    }
}
