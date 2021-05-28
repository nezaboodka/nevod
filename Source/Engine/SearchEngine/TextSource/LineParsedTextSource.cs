//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Nezaboodka.Text.Parsing;

using NTTokenKind = Nezaboodka.Text.Parsing.TokenKind;

namespace Nezaboodka.Nevod
{
    public class LineParsedTextSource : ITextSource
    {
        public ParsedText Text { get; }

        public int TokenCount => Text.PlainTextTokens.Count;

        public LineParsedTextSource(ParsedText text)
        {
            Text = text;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            int lineStart = 0;
            int lineEnd = 0;
            TextLocation previousLineBreakLocation;
            TextLocation currentLineBreakLocation = new TextLocation(0, 0, 0);
            while (lineStart < Text.PlainTextTokens.Count)
            {
                while (Text.PlainTextTokens[lineEnd].TokenKind != NTTokenKind.LineFeed
                    && Text.PlainTextTokens[lineEnd].TokenKind != NTTokenKind.End)
                {
                    lineEnd++;
                }
                Token lineBreakToken = GetToken(lineEnd);
                // ! Все контексты собираются в цепочку через PreviousLineBreak !
                previousLineBreakLocation = currentLineBreakLocation;
                currentLineBreakLocation = lineBreakToken.Location;
                var tokenContextLine = new TextLocationContext(previousLineBreakLocation, currentLineBreakLocation);
                lineBreakToken.Location.Context = tokenContextLine;
                for (int i = lineStart; i < lineEnd; i++)
                {
                    Token token = GetToken(i);
                    token.Location.Context = tokenContextLine;
                    yield return token;
                }
                yield return lineBreakToken;
                lineEnd++;
                lineStart = lineEnd;
            }
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

        internal static LineParsedTextSource FromString(string text)
        {
            ParsedText parsedText = PlainTextParser.Parse(text);
            var result = new LineParsedTextSource(parsedText);
            return result;
        }

        // Internal

        private Token GetToken(int tokenNumber)
        {
            TokenReference tokenReference = Text.PlainTextTokens[tokenNumber];
            Token result = new Token(TokenKindByTokenReferenceKind[(int)tokenReference.TokenKind],
                WordClassByTokenReferenceKind[(int)tokenReference.TokenKind],
                Text.GetTokenText(tokenReference),
                new TextLocation(tokenNumber, tokenReference.StringPosition, tokenReference.StringLength));
            return result;
        }

        internal static readonly TokenKind[] TokenKindByTokenReferenceKind =
            ParsedTextSource.TokenKindByTokenReferenceKind;

        internal static readonly WordClass[] WordClassByTokenReferenceKind =
            ParsedTextSource.WordClassByTokenReferenceKind;
    }
}
